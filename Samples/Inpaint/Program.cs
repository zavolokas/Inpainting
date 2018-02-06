using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inpaint
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: open an image and an image with a marked area to inpaint

            // TODO: build pyramids by downscaling the image and the markup
            // TODO: we should also apply a smoothing filter to the scaled images 
            // to reduce high spatial ferquency introduced by scaling.

            // TODO: go thru all the pyramid levels starting from the top one
            {
                // TODO: build a mapping that will define a source 
                // area (to get patches from) and related target area for which we need 
                // to calculate its pixels values.

                // TODO: if there is a NNF built on the prev level
                // scale it up 

                // TODO: start inpaint iterations
                {
                    // TODO: skip building NNF for the first iteration in the level
                    // unless it is top level (for the top one we don't have built 
                    // NNF yet)

                    // TODO: in order to find best mathes for the inpainted area,
                    // we build NNF for this image as a dest and a source 
                    // but excluding the inpainted area from the source area
                    // (our mapping already takes care of it)

                    // TODO: after NNF is built we calculate the values of the
                    // pixels in the inpainted area

                    // TODO: we also calculate the percent of pixels change during the iteration

                    // TODO: if the change is smaller then a treshold, we quit
                }
            }

            // TODO: convert image to a bitmap and save it
        }
    }
}
