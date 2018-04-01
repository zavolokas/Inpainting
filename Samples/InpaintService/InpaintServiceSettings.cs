using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zavolokas.ImageProcessing.Inpainting;

namespace InpaintService
{
    public class InpaintServiceSettings: InpaintSettings
    {
        public int MaxPointsAmountPerFunction { get; set; } = 22_500;
    }
}
