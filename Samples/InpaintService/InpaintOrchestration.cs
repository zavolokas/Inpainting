using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.Structures;

namespace InpaintService
{
    public static class InpaintOrchestration
    {
        [FunctionName("InpaintOrchestration")]
        public static async Task<long> Orchest(
            [OrchestrationTrigger]
            DurableOrchestrationContext backupContext)
        {
            var _levelDetector = new PyramidLevelsDetector();
            var _pyramidBuilder = new PyramidBuilder();
            var settings = new InpaintSettings();

            ZsImage imageArgb;
            ZsImage removeMaskArgb;

            //var levelsAmount = _levelDetector.CalculateLevelsAmount(imageArgb, removeMaskArgb, settings.PatchSize);

            //var _nnfBuilder = new PatchMatchNnfBuilder();

            //var req = backupContext.GetInput<InpaintRequest>();


            //var levelsAmount = _levelDetector.CalculateLevelsAmount(imageArgb, markupArgb, settings.PatchSize);

            return 100;
        }
    }
}
