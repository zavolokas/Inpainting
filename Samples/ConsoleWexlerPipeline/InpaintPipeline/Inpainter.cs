using System.Collections.Generic;
using Grapute;
using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.ImageProcessing.PatchMatch;
using Zavolokas.Structures;

namespace ConsoleWexlerPipeline
{
    public static class Inpainter
    {

        public static ZsImage Inpaint(ZsImage rgbImage, ZsImage markupRgbImage)
        {
            // TODO: that should be verifyed or smartly calculated
            const byte levelsAmount = 6;
            const double InitK = 3.0;
            const bool needBlur = true;

            Area2D markupArea = markupRgbImage.FromArgbToArea2D();
            ZsImage image = rgbImage.FromRgbToLab();

            var input = new WexlerLevelsData();
            input.OriginalImageWidth = image.Width;
            input.OriginalImageHeight = image.Height;
            input.OriginalImage = image.Clone();
            input.OriginalRemoveArea = markupArea;

            // Prepare images and markups for each level.
            var images = new Stack<ZsImage>();
            var markups = new Stack<Area2D>();
            var pixelAreas = new Stack<Area2D>();

            for (var levelIndex = levelsAmount - 1; levelIndex >= 0; levelIndex--)
            {
                var pixelsArea = Area2D.Create(0, 0, image.Width, image.Height);
                var areaToRemove = markupRgbImage.FromArgbToArea2D();
                //input.Pictures.Push(image);
                images.Push(image);
                markups.Push(areaToRemove);

                // TODO: make a research on the strange effect that occur when
                // we remove area on the each level (even when we do not perform bluring)
                if (levelIndex == 0)
                {
                    // We need to remove data about pixels at the dest image, since it will
                    // influence the NNF building. It will choose the source patches that are
                    // more similar to the area we want to inpaint.

                    #region Read more

                    // Because of the current mapping technique, the inpainted area will not be 
                    // considered as a source of pixels for the inpainted area(otherwise we will
                    // not inpaint but reconstruct the area). So, it is logical.
                    // But, the pixels of the area DO influence the NNF building process since
                    // they affect the calculated distance between the source and destination 
                    // patches. There is no such thing as an empty area in the Image, all the 
                    // pixels have values. That is why the empty/inpainted area should be indicated 
                    // with an additional layer of Area2D in the mapping that indicates empty 
                    // areas of the dest image. 
                    // Note: it is very important to differentiate the dest area and empty area of 
                    // the dest area. Dest area is the area of interest. For the dest area the NNF 
                    // should be built for example. Empty area is the area that indicates that the 
                    // information about pixels in this area should not be taken into account.

                    #endregion

                    pixelsArea = pixelsArea.Substract(areaToRemove);
                }
                pixelAreas.Push(pixelsArea);

                // Resize the image and all the markups for the next levels.
                image = image
                    .Clone()
                    .PyramidDownLab(needBlur);
                markupRgbImage = markupRgbImage
                    .PyramidDownArgb(needBlur);
                // TODO: perform the same for the donors if any
            }

            input.Settings.LevelsAmount = levelsAmount;
            input.Settings.LevelsToProcess = levelsAmount;
            input.Settings.StartLevel = 0;
            input.Settings.MaxPointsPerProcess = 4500;
            //input.Settings.Iterations = new byte[] { 20, 45, 30, 1, 2, 1 };
            //input.Settings.Iterations = new byte[] { 20, 30, 20, 16, 10, 10 };
            input.Settings.Iterations = new byte[] {3, 3, 3, 3, 3, 3};
            input.Settings.PatchDistanceCalculator = ImagePatchDistance.Cie76;
            input.Settings.ColorResolveMethod = ColorResolver.Simple;
            input.PatchMatchSettingsQueue.Enqueue(new PatchMatchSettings {PatchSize = 5});
            input.PatchMatchSettings.IterationsAmount = 3;

            // Input is ready, now we can create a pipeline.
            Node<WexlerLevelsData, WexlerLevelsData> pipeline = null;

            byte ps = input.PatchMatchSettings.PatchSize;
            double k = InitK;

            for (int levelIndex = 0; levelIndex < input.Settings.LevelsToProcess; levelIndex++)
            {
                image = images.Pop();
                input.Pictures.Enqueue(image);

                var removeArea = markups.Pop();
                input.RemoveAreas.Enqueue(removeArea);

                var pixelsArea = pixelAreas.Pop();
                input.PixelsAreas.Enqueue(pixelsArea);

                if (levelIndex == 0)
                {
                    input.CreateEmptyNnf();
                }

                if (levelIndex > 0)
                {
                    if (levelIndex > 0) ps = 9;
                    if (levelIndex > 1) ps = 11;
                    input.PatchMatchSettingsQueue.Enqueue(new PatchMatchSettings {PatchSize = ps});
                }

                // Area of interest is an area that is a bit bigger than the area
                // to inpaint. We build, update and normalize NNF in the area of interest.
                Area2D areaOfInterest;
                if (levelIndex != 0)
                    areaOfInterest = removeArea.Dilation(input.PatchMatchSettings.PatchSize * 2 + 1);
                else
                    areaOfInterest = Area2D.Create(0, 0, image.Width, image.Height);

                // Provide confidence map for remove area for each inpaint iteration.
                // Confidence map should be built for the remove area only.
                // The new colors should be calculated only for the remove area only as well.
                var confidenceMap = removeArea.CalculatePointsConfidence(input.Settings.ConfidentValue, input.Settings.Gamma);
                input.ConfidenceMaps.Enqueue(confidenceMap);

                // Prepare a mapping for each level.
                var map = new InpaintMapBuilder()
                    .InitNewMap(Area2D.Create(0, 0, image.Width, image.Height))
                    .SetInpaintArea(removeArea)
                    .ReduceDestArea(areaOfInterest)
                    .Build();

                var iterationsAmount = input.Settings.Iterations[levelIndex];
                for (int iterationIndex = 0; iterationIndex < iterationsAmount; iterationIndex++)
                {
                    input.Maps.Enqueue(map);

                    if (levelIndex == 0 || iterationIndex > 0)
                    {
                        k = InitK;

                        if (pipeline == null)
                        {
                            pipeline = new SplitNnfAndMap();
                            pipeline.SetInput(input);
                        }
                        else
                        {
                            pipeline.ForEachOutput(new SplitNnfAndMap());
                        }

                        // NNF random init
                        pipeline = pipeline.ForEachOutput(new NnfInit());

                        // A number of forward and backward neighbours check and random search
                        for (var i = 0; i < input.PatchMatchSettings.IterationsAmount; i++)
                        {
                            var direction = i % 2 == 0
                                ? NeighboursCheckDirection.Forward
                                : NeighboursCheckDirection.Backward;

                            pipeline = pipeline
                                .ForEachOutput(new PmNnfBuildIteration(direction));
                        }

                        pipeline = pipeline
                            .CollectAllOutputsToOneArray()
                            .ForArray(new MergeNnfsAndMaps())
                            .ForEachOutput(new NormalizeNnf())
                            .ForEachOutput(new Inpaint());
                    }
                    else // first iteration after nnf was scaled up
                    {
                        // Copying inpainted part from the previous level doesn't deliver 
                        // a good result because of different side effects. It is better to 
                        // reconstruct the area from the scaled NNF. In that case the inpainted
                        // part will be more detailed.

                        // In order to do that we Normalize and Inpaint the image from the 
                        // scaledup NNF that was built on the previous level.
                        pipeline = pipeline
                            .ForEachOutput(new NormalizeNnf())
                            .ForEachOutput(new Inpaint());
                    }

                    input.KQueue.Enqueue(k);

                    // TODO: the pipeline should not be responsible for that
                    if (k > input.Settings.MinKValue)
                    {
                        double dk = (InitK - input.Settings.MinKValue) / iterationsAmount;
                        k -= dk;
                    }
                }

                if (levelIndex < input.Settings.LevelsToProcess - 1)
                {
                    // Note: that we do not build a new NNF from scratch using the data for the new
                    // level, since it will discard all the previous job. 
                    // There is either disturbing pixels data of the area to inpaint or an empty hole 
                    // on its place. Dispite the fact that we have made a progress on predicting which 
                    // combination of patches will inpaint the area of interest during the processing 
                    // of the previous levels, it will be ignored in that case.
                    // For the same reasons it is also not possible to scale and imrove NNF with a pair 
                    // iterations of neighbours check and random search.
                    pipeline = pipeline.ForEachOutput(new ScaleUpNnf());
                }
            }

            //for (int i = 0; i < input.Maps.Count; i++)
            //{
            //    var m = input.Maps.Dequeue();
            //    (m as IAreasMapping)
            //        .DestArea
            //        .ToBitmap(Color.Green, input.CurrentPicture.Width, input.CurrentPicture.Height)
            //        .CloneWithScaleTo(input.OriginalImageWidth, input.OriginalImageHeight, InterpolationMode.NearestNeighbor)
            //        .SaveTo($@"..\..\d{i * 2}.png", ImageFormat.Png);

            //    input.CurrentPixelsArea
            //        .ToBitmap(Color.Aquamarine, input.CurrentPicture.Width, input.CurrentPicture.Height)
            //        .CloneWithScaleTo(input.OriginalImageWidth, input.OriginalImageHeight, InterpolationMode.NearestNeighbor)
            //        .SaveTo($@"..\..\d{i * 2 + 1}.png", ImageFormat.Png);

            //    for (int j = 0; j < 2; j++)
            //    {
            //        input.Maps.Dequeue();
            //    }

            //    input.PixelsAreas.Dequeue();
            //    input.Pictures.Dequeue();
            //}

            var data = pipeline
                .Process()
                .Output[0];

            return data.CurrentPicture;
        }
    }
}