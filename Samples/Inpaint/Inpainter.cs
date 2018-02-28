using System;
using System.Collections.Generic;
using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.ImageProcessing.PatchMatch;
using Zavolokas.Structures;

namespace Inpaint
{
    public class Inpainter
    {
        public event EventHandler<InpaintIterationFinishedEventArgs> IterationFinished;

        private InpaintingResult _inpaintingResult;

        public ZsImage Inpaint(ZsImage imageArgb, ZsImage markupArgb)
        {
            // TODO: move settings to a separate entity
            // TODO: should be calculated based on image and markup size
            const byte levelsAmount = 5;
            const byte patchSize = 11;
            const double changedPixelsPercentTreshold = 0.005;

            const double InitK = 3.0;
            const double MinK = 3.0;
            const double dk = 0.001;

            var calculator = ImagePatchDistance.Cie2000;
            var backgroundColor = new [] { 0.0, 0.0, 0.0 };

            var K = InitK;

            // TODO: extract a part of the image that can be scaled down 
            // required amount of times (levels)

            // Build pyramids by downscaling the image and the markup.
            // We also apply a smoothing filter to the scaled images 
            // to reduce high spatial ferquency introduced by scaling
            // (the filter is not applied to the inoainted area to avoid
            // inpainted object propagation out of its boundaries)
            var images = new Stack<ZsImage>();
            var mappings = new Stack<Area2DMap>();
            var markups = new Stack<Area2D>();

            var mapBuilder = new Area2DMapBuilder();
            var nnfBuilder = new PatchMatchNnfBuilder();

            // TODO: extract method
            // Build pyramids
            for (byte levelIndex = 0; levelIndex < levelsAmount; levelIndex++)
            {
                // convert image to Lab color space and store it
                var imageCopy = imageArgb.Clone()
                    .FromArgbToRgb(backgroundColor)
                    .FromRgbToLab();
                images.Push(imageCopy);

                var inpaintArea = markupArgb.FromArgbToArea2D();
                var imageArea = Area2D.Create(0, 0, imageArgb.Width, imageArgb.Height);
                var nnfSourceArea = imageArea.Substract(inpaintArea);
                Area2D nnfTargetArea;

                // Obtain target area for the NNF building based on the level
                if (levelIndex == levelsAmount - 1)
                {
                    // For the top level, the target area should be the whole image area
                    nnfTargetArea = imageArea;
                }
                else
                {
                    // For the rest levels, the target area should be
                    // slightly bigger then the inpaint area.
                    nnfTargetArea = inpaintArea
                        .Dilation(patchSize)
                        .Intersect(imageArea);
                }

                // Create a mapping for the level.
                var mapping = mapBuilder.InitNewMap(nnfTargetArea, nnfSourceArea)
                    .Build();

                mappings.Push(mapping);
                markups.Push(inpaintArea);

                if (levelIndex < levelsAmount - 1)
                {
                    // downscale for the next level
                    // NOTE: we shouldn't blur out the inpainted area so it is not getting bigger!!
                    imageArgb.PyramidDownArgb(nnfSourceArea);
                    markupArgb.PyramidDownArgb(false);
                }
            }

            // go thru all the pyramid levels starting from the top one
            Nnf nnf = null;
            ZsImage image = null;

            var nnfSettings = new PatchMatchSettings { PatchSize = patchSize };
            
            for (byte levelIndex = 0; levelIndex < levelsAmount; levelIndex++)
            {
                image = images.Pop();
                var mapping = mappings.Pop();
                var inpaintArea = markups.Pop();

                var imageArea = Area2D.Create(0, 0, image.Width, image.Height);

                // if there is a NNF built on the prev level
                // scale it up
                nnf = nnf == null
                    ? new Nnf(image.Width, image.Height, image.Width, image.Height, nnfSettings.PatchSize)
                    : nnf.CloneAndScale2XWithUpdate(image, image, nnfSettings, mapping, calculator);

                // start inpaint iterations
                K = InitK;
                int inpaintIteration = 0;
                while (inpaintIteration < 10)
                {
                    // Obtain pixels area.
                    // Pixels area defines which pixels are allowed to be used
                    // for the patches distance calculation. We must avoid pixels
                    // that we want to inpaint. That is why before the area is not
                    // inpainted - we should exclude this area.
                    var pixelsArea = imageArea;
                    if (levelIndex == 0 && inpaintIteration == 0)
                    {
                        pixelsArea = imageArea.Substract(inpaintArea);
                    }

                    // skip building NNF for the first iteration in the level
                    // unless it is top level (for the top one we haven't built NNF yet)
                    if (levelIndex == 0 || inpaintIteration > 0)
                    {
                        // in order to find best matches for the inpainted area,
                        // we build NNF for this image as a dest and a source 
                        // but excluding the inpainted area from the source area
                        // (our mapping already takes care of it)
                        nnfBuilder.RunRandomNnfInitIteration(nnf, image, image, nnfSettings, calculator, mapping, pixelsArea);
                        nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Forward, nnfSettings, calculator, mapping, pixelsArea);
                        nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Backward, nnfSettings, calculator, mapping, pixelsArea);
                        nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Forward, nnfSettings, calculator, mapping, pixelsArea);
                        nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Backward, nnfSettings, calculator, mapping, pixelsArea);
                        nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Forward, nnfSettings, calculator, mapping, pixelsArea);
                    }

                    var nnfNormalized = nnf.Clone();
                    nnfNormalized.Normalize();

                    // after we have the NNF - we calculate the values of the pixels in the inpainted area
                    var inpaintResult = Inpaint(image, inpaintArea, nnfNormalized, nnfSettings.PatchSize, ColorResolver.MeanShift, K);
                    //if (K > MinK)
                    //{
                    //    K -= dk;
                    //}

                    if (IterationFinished != null)
                    {
                        var eventArgs = new InpaintIterationFinishedEventArgs
                        {
                            InpaintedLabImage = image.Clone(),
                            InpaintResult = inpaintResult,
                            LevelIndex = levelIndex,
                            InpaintIteration = inpaintIteration
                        };
                        IterationFinished(this, eventArgs);
                    }

                    inpaintIteration++;
                    // if the change is smaller then a treshold, we quit
                    if (inpaintResult.ChangedPixelsPercent < changedPixelsPercentTreshold) break;
                }
            }

            // TODO: paste result in the original bitmap where it was extracted from

            return image
                .FromLabToRgb()
                .FromRgbToArgb(Area2D.Create(0, 0, image.Width, image.Height));
        }

        private InpaintingResult Inpaint(ZsImage image, Area2D removeArea, Nnf nnf, int patchSize, ColorResolver colorResolver, double k)
        {
            // Since the nnf was normalized, the sigma now is normalized as well and it
            // has not any more some specific value.
            //const double naturalLogBase = System.Math.E;
            //const double minusSigmaCube2 = -0.91125;//-2 * sigma * sigma; sigma = 0.675 = 75percentil
            //double NaturalLogBase2 = System.Math.Pow(System.Math.E, 1.0 / minusSigmaCube2);
            const double naturalLogBasePowMinusSigmaCube2 = 0.33373978049163078;

            const double pixelChangeTreshold = 0.00003;
            const double maxSquareDistanceInLab = 32668.1151;
            // Gamma is used for calculation of alpha in markup. The confidence.
            const double gamma = 1.3;
            // The value of confidence in non marked areas.
            const double confidentValue = 1.50;

            // Get points' indexes that that needs to be inpainted.
            var pointIndexes = new int[removeArea.ElementsCount];
            removeArea.FillMappedPointsIndexes(pointIndexes, image.Width);

            var patchOffset = (patchSize - 1) / 2;

            var imageWidth = image.Width;
            var nnfWidth = nnf.DstWidth;
            var nnfHeight = nnf.DstHeight;
            var srcWidth = nnf.SourceWidth;
            var srcHeight = nnf.SourceHeight;

            var nnfdata = nnf.GetNnfItems();
            var cmpts = image.NumberOfComponents;

            // Weighted color is color's components values + weight of the color.
            var weightedColors = new double[patchSize * patchSize * (cmpts + 1)];
            var confidence = removeArea.CalculatePointsConfidence(confidentValue, gamma);
            var resolvedColor = new double[cmpts];

            var totalDifference = 0.0;
            var changedPixelsAmount = 0;
            var changedPixelsDifference = 0.0;

            for (int ii = 0; ii < pointIndexes.Length; ii++)
            {
                var pointIndex = pointIndexes[ii];

                int pty = pointIndex / imageWidth;
                int ptx = pointIndex % imageWidth;

                int colorsCount = 0;

                //go thru the patch
                for (int y = pty - patchOffset, j = 0; j < patchSize; j++, y++)
                {
                    for (int x = ptx - patchOffset, i = 0; i < patchSize; i++, x++)
                    {
                        if (0 <= x && x < nnfWidth && 0 <= y && y < nnfHeight)
                        {
                            int destPatchPointIndex = y * nnfWidth + x;
                            int srcPatchPointIndex = (int)nnfdata[destPatchPointIndex * 2];

                            int srcPatchPointX = srcPatchPointIndex % nnfWidth;
                            int srcPatchPointY = srcPatchPointIndex / nnfWidth;

                            int srcPixelX = srcPatchPointX - i + patchOffset;
                            int srcPixelY = srcPatchPointY - j + patchOffset;

                            if (0 <= srcPixelX && srcPixelX < srcWidth && 0 <= srcPixelY && srcPixelY < srcHeight)
                            {
                                int srcPixelIndex = srcPixelY * imageWidth + srcPixelX;

                                for (int ci = 0; ci < cmpts; ci++)
                                {
                                    weightedColors[colorsCount * (cmpts + 1) + ci] = image.PixelsData[srcPixelIndex * cmpts + ci];
                                }
                                weightedColors[colorsCount * (cmpts + 1) + cmpts] =
                                    System.Math.Pow(naturalLogBasePowMinusSigmaCube2, nnfdata[destPatchPointIndex * 2 + 1]) *
                                    confidence[ii];

                                colorsCount++;
                            }
                        }
                    }
                }
                // calculate color
                colorResolver.Resolve(weightedColors, 0, colorsCount, cmpts, k, resolvedColor, 0);

                // how different is resolvedColor from the old color?
                var labL = resolvedColor[0] - image.PixelsData[pointIndex * cmpts + 0];
                var labA = resolvedColor[1] - image.PixelsData[pointIndex * cmpts + 1];
                var labB = resolvedColor[2] - image.PixelsData[pointIndex * cmpts + 2];
                var pixelsDiff = (labL * labL + labA * labA + labB * labB) / maxSquareDistanceInLab;
                totalDifference += pixelsDiff;

                if (pixelsDiff > pixelChangeTreshold)
                {
                    changedPixelsAmount++;
                    changedPixelsDifference += pixelsDiff;
                }

                for (var i = 0; i < cmpts; i++)
                {
                    image.PixelsData[pointIndex * cmpts + i] = resolvedColor[i];
                }
            }

            totalDifference /= pointIndexes.Length;
            _inpaintingResult.TotalDifference = totalDifference / pointIndexes.Length;
            _inpaintingResult.PixelsChangedAmount = changedPixelsAmount;
            _inpaintingResult.PixelsToInpaintAmount = pointIndexes.Length;
            _inpaintingResult.ChangedPixelsDifference = changedPixelsDifference;

            return _inpaintingResult;
        }
    }
}