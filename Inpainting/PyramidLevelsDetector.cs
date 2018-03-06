using System;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting
{
    public class PyramidLevelsDetector
    {
        /// <summary>
        /// Calculates the levels amount.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="removeMarkup">The remove markup.</param>
        /// <param name="patchSize">Size of the patch.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// image
        /// or
        /// removeMarkup
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when patchSize is less than 2.</exception>
        /// <exception cref="AreaRemovedException">Thrown when area to remove covers whole image.</exception>
        public byte CalculateLevelsAmount(ZsImage image, ZsImage removeMarkup, byte patchSize)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (removeMarkup == null)
                throw new ArgumentNullException(nameof(removeMarkup));

            if (patchSize < 2)
                throw new ArgumentOutOfRangeException(nameof(patchSize));

            var imageArea = Area2D.Create(0, 0, image.Width, image.Height);
            var removeArea = removeMarkup
                .FromArgbToArea2D()
                .Intersect(imageArea);

            if (imageArea.Substract(removeArea).IsEmpty)
                throw new AreaRemovedException();

            if (removeArea.IsEmpty)
                return 1;

            const double patchProcentOfMarkup = 0.15;

            byte levels = 1;
            var patchArea = patchSize * patchSize;

            var markupPointsAmount = removeArea.ElementsCount;

            //in the smart version of the method we take into account the density 
            //of the marking, so we wouldnt get a lots of levels when need to remove 
            //many small things

            //var searchArea = new RectArea(0, 0, markup.Width, markup.Height);
            ////get all the pieces of the marking
            //var areas = Utils.ImageUtils.GetAllObjects(markup.Data, searchArea);

            //if (areas.Count > 1)
            //{
            //    //find the biggest area and use its pixels amount
            //    markupPointsAmount = areas.Max(x => x.PixAmount);
            //}


            double curPpm = patchArea / (double)markupPointsAmount;

            while (curPpm < patchProcentOfMarkup)
            {
                levels++;
                markupPointsAmount /= 4;
                curPpm = patchArea / (double)markupPointsAmount;
            }

            return levels;
        }
    }
}
