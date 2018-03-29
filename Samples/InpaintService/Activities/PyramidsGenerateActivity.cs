using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.Math.Combinatorics;
using Zavolokas.Structures;

namespace InpaintService.Activities
{
    public static class PyramidsGenerateActivity
    {
        public const string Name = "GeneratePyramids";

        [FunctionName(Name)]
        public static async Task<CloudPyramid> GeneratePyramids([ActivityTrigger] InpaintRequest inpaintRequest)
        {
            var levelDetector = new PyramidLevelsDetector();
            var pyramidBuilder = new PyramidBuilder();
            var settings = new InpaintSettings();

            var container = BlobHelper.OpenBlobContainer(inpaintRequest.Container);
            var imageBlob = container.GetBlockBlobReference(inpaintRequest.Image);
            var removeMaskBlob = container.GetBlockBlobReference(inpaintRequest.RemoveMask);

            var imageArgb = await BlobHelper.ConvertBlobToArgbImage(imageBlob);
            var removeMaskArgb = await BlobHelper.ConvertBlobToArgbImage(removeMaskBlob);

            var levelsAmount = levelDetector.CalculateLevelsAmount(imageArgb, removeMaskArgb, settings.PatchSize);
            pyramidBuilder.Init(imageArgb, removeMaskArgb);
            var pyramid = pyramidBuilder.Build(levelsAmount, settings.PatchSize);
            var cloudPyramid = new CloudPyramid
            {
                Levels = new CloudPyramidLevel[pyramid.LevelsAmount]
            };

            for (byte levelIndex = 0; levelIndex < pyramid.LevelsAmount; levelIndex++)
            {
                var image = pyramid.GetImage(levelIndex);
                var fileName = $"{levelIndex}.png";
                await BlobHelper.SaveImageLabToBlob(image, container, fileName);
                cloudPyramid.Levels[levelIndex].ImageName = fileName;

                var inpaintArea = pyramid.GetInpaintArea(levelIndex);
                var inpaintAreaState = inpaintArea.GetState();
                var inpaintAreaFileName = $"ia{levelIndex}.json";
                var inpaintAreaData = JsonConvert.SerializeObject(inpaintAreaState);
                BlobHelper.SaveJsonToBlob(inpaintAreaData, container, inpaintAreaFileName);
                cloudPyramid.Levels[levelIndex].InpaintArea = inpaintAreaFileName;

                cloudPyramid.Levels[levelIndex].Nnf = $"nnf{levelIndex}.json";

                var mapping = pyramid.GetMapping(levelIndex);
                var mappingFileName = $"map{levelIndex}.json";
                var mapState = mapping.GetState();
                var mappingData = JsonConvert.SerializeObject(mapState);
                BlobHelper.SaveJsonToBlob(mappingData, container, mappingFileName);
                cloudPyramid.Levels[levelIndex].Mapping = mappingFileName;

                var mappings = SplitMapping(mapping, 400).ToArray();
                cloudPyramid.Levels[levelIndex].SplittedMappings = new string[mappings.Length];
                cloudPyramid.Levels[levelIndex].SplittedNnfs = new string[mappings.Length];
                for (var i = 0; i < mappings.Length; i++)
                {
                    var map = mappings[i];
                    mappingFileName = $"map{levelIndex}_p{i}.json";
                    mapState = map.GetState();
                    mappingData = JsonConvert.SerializeObject(mapState);
                    BlobHelper.SaveJsonToBlob(mappingData, container, mappingFileName);
                    cloudPyramid.Levels[levelIndex].SplittedMappings[i] = mappingFileName;
                    cloudPyramid.Levels[levelIndex].SplittedNnfs[i] = $"nnf{levelIndex}_p{i}.json";
                }
            }

            return cloudPyramid;
        }

        private static IEnumerable<Area2DMap> SplitMapping(Area2DMap currentMap, int maxPointsPerProcess)
        {
            // The input should be splitted smartly taking into account the input data and settings.

            var result = new List<Area2DMap>();

            if (currentMap.DestElementsCount > maxPointsPerProcess)
            {
                // Decide on how many parts we are going to divide input.
                var partsAmount = (double)currentMap.DestElementsCount / maxPointsPerProcess;
                if (partsAmount == 1) partsAmount++;

                // Let's calculate how many rows & columns we are going to have
                var rect = currentMap.DestBounds;
                var w = rect.Width;
                var h = rect.Height;
                var left = rect.X;
                var top = rect.Y;

                var sideSize = Math.Sqrt(maxPointsPerProcess);
                double cs, rs;

                if (w > h)
                {
                    cs = w / sideSize;
                    rs = partsAmount / cs;
                }
                else
                {
                    rs = h / sideSize;
                    cs = partsAmount / rs;
                }

                var csfloor = Math.Floor(cs);
                var columns = (int)(csfloor < cs ? csfloor + 1 : csfloor);

                var rsfloor = Math.Floor(rs);
                var rows = (int)(rsfloor < rs ? rsfloor + 1 : rsfloor);

                // Finally - what is the cell size?
                var cellWidth = w / columns;
                var rowHight = h / rows;

                // Split mapping
                var newMappings = new List<Area2DMap>();

                for (var y = top; y < top + h; y += rowHight)
                {
                    for (var x = left; x < left + w; x += cellWidth)
                    {
                        var map = new Area2DMapBuilder()
                            .InitNewMap(currentMap)
                            .ReduceDestArea(Area2D.Create(x, y, cellWidth, rowHight), true)
                            .Build();

                        if (map.DestElementsCount > 0)
                        {
                            newMappings.Add(map);
                        }
                    }
                }

                // Combine small mappings
                var bins = BinsPacker<Area2DMap>.Pack(newMappings, m => m.DestElementsCount, maxPointsPerProcess);
                newMappings.Clear();
                foreach (var bin in bins)
                {
                    var enumerable = bin as IList<Area2DMap> ?? bin.ToList();
                    if (enumerable.Count > 1)
                    {
                        newMappings.Add(
                            enumerable.Aggregate((m1, m2) => new Area2DMapBuilder()
                                .InitNewMap(m1)
                                .AddMapping(m2)
                                .Build()));
                    }
                    else
                    {
                        newMappings.Add(enumerable.First());
                    }
                }


                // Make a new input.
                for (int i = 0; i < newMappings.Count; i++)
                {
                    var map = newMappings[i];

                    if (map.DestElementsCount > 0)
                    {
                        result.Add(map);
                    }
                }
            }
            else
            {
                result.Add(currentMap);
            }

            return result;
        }
    }
}