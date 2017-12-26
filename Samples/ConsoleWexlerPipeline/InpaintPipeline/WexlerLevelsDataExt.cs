using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Zavolokas.GdiExtensions;
using Zavolokas.Structures;

namespace ConsoleWexlerPipeline
{
    internal static class WexlerLevelsDataExt
    {
        public static string GetInfo(this WexlerLevelsData input)
        {
            return
                $"maps:{input.Maps.Count},\tpic:{input.Pictures.Count},\tRemoveAreas:{input.RemoveAreas.Count},\t:ConfidenceMaps:{input.ConfidenceMaps.Count}";
        }

        public static void SaveAll(this WexlerLevelsData input, int iteration)
        {
            input.SaveResult(iteration, true, true, true, true, true, true, true, true, true);
        }

        public static void SaveResult(this WexlerLevelsData input, int iteration,
            bool currentPicture = false,
            bool removeArea = false,
            bool nnf = false,
            bool normalizedNnf = false,
            bool inpaintedOriginal = false,
            bool confidenceMap = false,
            bool restored = false,
            bool pixelsArea = false,
            bool destArea = false)
        {
            if (currentPicture) input.SaveCurrentPicture(iteration);
            if (removeArea) input.SaveCurrentRemoveArea(iteration);
            if (nnf) input.SaveNnf(iteration);
            if (normalizedNnf) input.SaveNormalizedNnf(iteration);
            if (inpaintedOriginal) input.SaveInpaintedOriginal(iteration);
            if (confidenceMap) input.SaveConfidenceMap(iteration);
            if (restored) input.SaveRestoredFromNnf(iteration);
            if (pixelsArea) input.SaveCurrentPixelsArea(iteration);
            if (destArea) input.SaveCurrentDestArea(iteration);
        }

        public static void SaveCurrentDestArea(this WexlerLevelsData input, int iteration)
        {
            (input.CurrentMap as IAreasMapping).DestArea
                .ToBitmap(Color.Green, input.CurrentPicture.Width, input.CurrentPicture.Height)
                .CloneWithScaleTo(input.OriginalImageWidth, input.OriginalImageHeight, InterpolationMode.NearestNeighbor)
                .Save($@"..\..\l{iteration:D2}_destArea.png", ImageFormat.Png);
        }

        public static void SaveCurrentPixelsArea(this WexlerLevelsData input, int iteration)
        {
            input.CurrentPixelsArea
                .ToBitmap(Color.BlueViolet, input.CurrentPicture.Width, input.CurrentPicture.Height)
                .CloneWithScaleTo(input.OriginalImageWidth, input.OriginalImageHeight, InterpolationMode.NearestNeighbor)
                .Save($@"..\..\l{iteration:D2}_pixelsArea.png", ImageFormat.Png);
        }

        public static void SaveCurrentRemoveArea(this WexlerLevelsData input, int iteration)
        {
            input.CurrentRemoveArea
                .ToBitmap(Color.Red, input.CurrentPicture.Width, input.CurrentPicture.Height)
                .CloneWithScaleTo(input.OriginalImageWidth, input.OriginalImageHeight, InterpolationMode.NearestNeighbor)
                .Save($@"..\..\l{iteration:D2}_markup.png", ImageFormat.Png);
        }

        public static void SaveConfidenceMap(this WexlerLevelsData input, int iteration)
        {
            input.CurrentConfidenceMap
                .ToBitmap(input.CurrentRemoveArea, input.CurrentPicture.Width, input.CurrentPicture.Height)
                .CloneWithScaleTo(input.OriginalImageWidth, input.OriginalImageHeight, InterpolationMode.NearestNeighbor)
                .Save($@"..\..\l{iteration:D2}_conf.png", ImageFormat.Png);
        }

        public static void SaveNormalizedNnf(this WexlerLevelsData input, int iteration)
        {
            input.NormalizedNnf
                .ToRgbImage()
                .FromRgbToBitmap()
                .CloneWithScaleTo(input.OriginalImageWidth, input.OriginalImageHeight, InterpolationMode.NearestNeighbor)
                .Save($"..\\..\\l{iteration:D2}_nnf_norm.png", ImageFormat.Png);
        }

        public static void SaveRestoredFromNnf(this WexlerLevelsData input, int iteration)
        {
            input.Nnf
                .RestoreImage(input.CurrentPicture, 3, input.PatchMatchSettings.PatchSize)
                .FromLabToRgb()
                .FromRgbToBitmap()
                .CloneWithScaleTo(input.OriginalImageWidth, input.OriginalImageHeight, InterpolationMode.NearestNeighbor)
                .Save($"..\\..\\l{iteration:D2}_restored.png", ImageFormat.Png);
        }

        public static void SaveNnf(this WexlerLevelsData input, int iteration)
        {
            input.Nnf
                .ToRgbImage()
                .FromRgbToBitmap()
                .CloneWithScaleTo(input.OriginalImageWidth, input.OriginalImageHeight, InterpolationMode.NearestNeighbor)
                .Save($"..\\..\\l{iteration:D2}_nnf.png", ImageFormat.Png);
        }

        public static void SaveInpaintedOriginal(this WexlerLevelsData input, int iteration)
        {
            input.OriginalImage
                .CopyFromImage(
                        destArea: input.OriginalRemoveArea,
                        srcImage: input.CurrentPicture
                                        .ScaleTo(input.OriginalImage.Width, input.OriginalImage.Height),
                        srcArea: input.OriginalRemoveArea)
                .FromLabToRgb()
                .FromRgbToBitmap()
                .Save($@"..\..\l{iteration:D2}_image2.png", ImageFormat.Png);
        }

        public static void SaveCurrentPicture(this WexlerLevelsData input, int iteration)
        {
            input.CurrentPicture
                .Clone()
                .FromLabToRgb()
                .FromRgbToBitmap()
                .CloneWithScaleTo(input.OriginalImageWidth, input.OriginalImageHeight, InterpolationMode.NearestNeighbor)
                .Save($@"..\..\l{iteration:D2}_image1.png", ImageFormat.Png);
        }
    }
}