using System;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting
{
    public class PyramidBuilder
    {
        public void SetImage(ZsImage image)
        {

        }

        public void SetRemoveMarkup(ZsImage markup)
        {

        }

        public void AddDonorMarkup(ZsImage donor)
        {

        }

        /// <summary>
        /// Builds an image pyramid with the specified levels amount.
        /// </summary>
        /// <param name="levelsAmount">The levels amount.</param>
        /// <returns></returns>
        public IPyramid Build(byte levelsAmount)
        {
            throw new NotImplementedException();
        }
    }
}