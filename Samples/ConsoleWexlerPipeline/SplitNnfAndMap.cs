using System;
using System.Collections.Generic;
using System.Linq;
using Grapute;
using Zavolokas.Math;
using Zavolokas.Structures;

namespace ConsoleWexlerPipeline
{
    internal class SplitNnfAndMap : Node<WexlerLevelsData, WexlerLevelsData>
    {
        protected override WexlerLevelsData[] Process(WexlerLevelsData input)
        {
            // The input should be splitted smartly taking into account the input data and settings.

            var result = new List<WexlerLevelsData>();

            if (input.CurrentMap.DestElementsCount > input.Settings.MaxPointsPerProcess)
            {
                // Decide on how many parts we are going to divide input.
                var partsAmount = (double)input.CurrentMap.DestElementsCount / input.Settings.MaxPointsPerProcess;
                if (partsAmount == 1) partsAmount++;

                // Let's calculate how many rows & columns we are going to have
                var rect = input.CurrentMap.DestBounds;
                var w = rect.Width;
                var h = rect.Height;
                var left = rect.X;
                var top = rect.Y;

                var sideSize = Math.Sqrt(input.Settings.MaxPointsPerProcess);
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
                            .InitNewMap(input.CurrentMap)
                            .ReduceDestArea(Area2D.Create(x, y, cellWidth, rowHight), true)
                            .Build();

                        if (map.DestElementsCount > 0)
                        {
                            newMappings.Add(map);
                        }
                    }
                }

                // Combine small mappings
                var bins = BinsPacker<Area2DMap>.Pack(newMappings, m => m.DestElementsCount, input.Settings.MaxPointsPerProcess);
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
                        var newInput = input.MakeCopy();
                        newInput.SubstituteMap(map);
                        result.Add(newInput);
                    }
                }
            }
            else
            {
                result.Add(input);
            }

            Console.WriteLine($"NnfSplit:\t{input.GetInfo()}");
            return result.ToArray();
        }
    }
}