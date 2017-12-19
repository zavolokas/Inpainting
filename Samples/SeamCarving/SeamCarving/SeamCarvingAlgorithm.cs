using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace SeamCarving
{
    public sealed class SeamCarvingAlgorithm
    {
        public Image[] InpaintImage(Image image, Image removeMarkupImage, Image protectMarkupImage, SeamCarvingSettings settings)
        {
            int width = image.Width;
            int height = image.Height;

            int[][] removeMarkup = ImageUtils.ConvertToBinaryIntJaggedArray(removeMarkupImage);
            int[] removeMarkup1 = removeMarkup.ToOneDemensional();
            ArrayHelper.Multiply(removeMarkup1, 100);
            removeMarkup.CopyFrom(removeMarkup1);

            int[][] protectMarkup;

            if (protectMarkupImage != null)
            {
                protectMarkup = ImageUtils.ConvertToBinaryIntJaggedArray(protectMarkupImage);
                int[] protectMarkup1 = protectMarkup.ToOneDemensional();
                ArrayHelper.Multiply(protectMarkup1, 30000);
                protectMarkup.CopyFrom(protectMarkup1);
            }
            else
            {
                protectMarkup = ArrayHelper.CreateJagged<int>(width, height);
            }

            var remover = new SmartRemove(width, height);

            var searchArea = new RectArea(0, 0, image.Width, image.Height);
            var areas = ImageUtils.GetAllObjects(removeMarkup1, searchArea, image.Width);

            // TODO: replace with ZsImage
            int[][] imagePixels = ConvertRgbToInt(image.Pixels);

            //-----------"INEVBERE"----------------------

            var factory = new RemoveUnitFactory(remover, new SobolEdgeDetector(), new ImageFilters());

            IRemoveTypeResolver removeTypeResolver = new SimpleRemoveTypeResolver();
            var removeTarget = removeTypeResolver.DecideOn(areas, width, height);
            var removeUnit = factory.CreateRemoveUnit(removeTarget, imagePixels.ToOneDemensional(), removeMarkup.ToOneDemensional(), protectMarkup.ToOneDemensional(), width, height);

            //update the width and height, because they might be changed depending on orientation
            width = removeUnit.Width;

            //REMOVEPART
            int removeItarationsCount = Remove(width, remover, removeUnit);

            var removeUnits = new List<RemoveUnit> { removeUnit };
            //it is not an optimization it is to avoid infinite loop, since the remove units
            // list is grow inside the loop

            if (settings.ProvideRestored || settings.ProvideRestoredWithMask)
            {
                var removeUnitsToRestore = new List<RemoveUnit>();

                //"RESTORE"

                //calculate the procentage of the rest part of the image
                var restRate = removeUnit.Width / (double)width;
                //there are no appropriate restored variants for the mostly removed image
                if (restRate > 0.15)
                {
                    if (settings.ProvideRestored)
                    {
                        //each remove unit creates two another variants of photo and adds them to a list
                        var noPreserveRemoveUnits = factory.Clone(removeUnit, false, false, width);
                        removeUnits.Add(noPreserveRemoveUnits);
                        removeUnitsToRestore.Add(noPreserveRemoveUnits);
                    }

                    //it is make no sence to try restore a photo taking into
                    //account a preserve map if there is no such a map!
                    if (!removeUnit.IsPreserveMapEmpty && settings.ProvideRestoredWithMask)
                    {
                        var preserveRemoveUnit = factory.Clone(removeUnit, false, true, width);
                        removeUnits.Add(preserveRemoveUnit);
                        removeUnitsToRestore.Add(preserveRemoveUnit);
                    }
                }

                //we want to have unrestored variant of photo and in order to get it
                //we indicate that all restore iterations have been performed
                removeUnit.restoreIteration = removeUnit.removeItarationsCount;

                if (!settings.ProvideNotRestored)
                {
                    removeUnits.RemoveAt(0);
                }

                Restore(image, remover, removeUnits, removeItarationsCount, removeUnitsToRestore);
            }

            var results = new Image[removeUnits.Count];
            for (var index = 0; index < removeUnits.Count; index++)
            {
                var component1 = removeUnits[index];
                if (component1.Width <= 0)
                    continue;
                var resultPhoto = component1.GetPhoto(image.Width);
                var result2d = ArrayHelper.CreateJagged<int>(component1.Width, component1.Height);
                result2d.CopyFrom(resultPhoto);
                var rgbResultPhoto = ConvertIntToRgb(result2d);
                results[index] = new RgbImage(rgbResultPhoto);
            }

            return results;
        }

        private static void SaveEnergyMap(int[] energy, int width,int i)
        {
            var height = energy.Length / width;
            var rgbEnergy = ConvertEnergyToRgb(energy);
            var result2d = ArrayHelper.CreateJagged<int>(width, height);
            result2d.CopyFrom(rgbEnergy);
            var rgbResultPhoto = ConvertIntToRgb(result2d);
            var rgbImage = new RgbImage(rgbResultPhoto);
            var gdiImage = new GdiImage(rgbImage);
            gdiImage.Bitmap.Save($"..\\..\\{i}.bmp", ImageFormat.Bmp);
        }

        private static int[] ConvertEnergyToRgb(int[] energy)
        {
            var max = (double)energy.Max();
            var min = (double)energy.Min();

            //var offs = min < 0 ? Math.Abs(min) : -min;
            var size = max - min;

            var result = new int[energy.Length];
            energy.CopyTo(result, 0);

            return result.Select(e =>
            {
                var normE = (int)(((e - min) / size)*255);
                return ((0xFF) << 24) | ((normE & 0xFF) << 16) | ((normE & 0xFF) << 8) | (normE & 0xFF);
            }).ToArray();
        }

        private static int Remove(int width, SmartRemove remover, RemoveUnit removeUnit)
        {
            int removeItarationsCount = 0;

            while (!removeUnit.IsStuck && removeItarationsCount < width)
            {
                // Removing is still not stuck so we can proceed with deleting
                SaveEnergyMap(removeUnit.EnergyMap, width, removeItarationsCount);
                //update the minCol which is a property of a removeUnit object
                remover.RecalculateCurrentMinCol(removeUnit.EnergyMap, removeUnit.Width, width, removeUnit.Height, removeUnit.minCol);
                remover.RemoveCol(removeUnit, removeUnit.All, removeUnit.Width, width, removeUnit.Height, removeUnit.minCol);
                remover.CorrectEnergy(removeUnit.EnergyMap, removeUnit.RemoveMap, removeUnit.PreserveMap, removeUnit.Edges, removeUnit.Width, width, removeUnit.minCol);

                System.Diagnostics.Debug.Assert(remover.IsValidVEnergyMap(removeUnit.EnergyMap, removeUnit.RemoveMap, removeUnit.PreserveMap, removeUnit.Edges, removeUnit.Width, width, removeUnit.Height), "The energy map is not correct!");

                removeItarationsCount++;
            }

            return removeItarationsCount;
        }

        private static void Restore(Image image, SmartRemove remover, List<RemoveUnit> removeUnits, int removeItarationsCount, List<RemoveUnit> unitsToRestore)
        {
            //removeItarationsCount has a value which is the MAX amount of iterations
            for (var index = 0; index < removeItarationsCount; index++)
            {
                //foreach removeUnit perform this if its own iteratin counter less then current iteration
                for (var unitIndex = 0; unitIndex < unitsToRestore.Count; unitIndex++)
                {
                    var removeUnit = unitsToRestore[unitIndex];

                    remover.FindMinCol(removeUnit.EnergyMap, removeUnit.Width, image.Width, removeUnit.Height, removeUnit.minCol);
                    remover.StreightenCol(removeUnit.Edges, removeUnit.minCol, image.Width, image.Height);
                    remover.CorrectEnergy(removeUnit.EnergyMap, removeUnit.RemoveMap, removeUnit.PreserveMap, removeUnit.Edges, removeUnit.Width, image.Width, removeUnit.minCol);
                    remover.CloneCol(removeUnit, removeUnit.ToRestore, removeUnit.Width, image.Width, removeUnit.Height, removeUnit.minCol);
                }

                unitsToRestore.Clear();
                unitsToRestore.AddRange(
                    removeUnits.Where(unit => unit.restoreIteration < unit.removeItarationsCount));
            }
        }

        private static RgbColor[][] ConvertIntToRgb(int[][] pixels)
        {
            var result = new RgbColor[pixels.Length][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new RgbColor[pixels[i].Length];
                for (int j = 0; j < result[i].Length; j++)
                {
                    var color = pixels[i][j];

                    int r = (color & 0x00ff0000) >> 16;
                    int g = (color & 0x0000ff00) >> 8;
                    int b = (color & 0x000000ff);

                    result[i][j] = new RgbColor((byte)r, (byte)g, (byte)b);
                }
            }
            return result;
        }

        private static int[][] ConvertRgbToInt(RgbColor[][] pixels)
        {
            var result = new int[pixels.Length][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new int[pixels[i].Length];
                for (int j = 0; j < result[i].Length; j++)
                {
                    var color = pixels[i][j];
                    result[i][j] = ((0xFF) << 24) | ((color.R & 0xFF) << 16) | ((color.G & 0xFF) << 8) | (color.B & 0xFF);
                }
            }

            return result;
        }
    }
}
