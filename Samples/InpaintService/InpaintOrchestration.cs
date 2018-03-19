using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace InpaintService
{
    public static class InpaintOrchestration
    {
        [FunctionName("InpaintOrchestration")]
        public static async Task<long> Orchest(
            [OrchestrationTrigger]
            DurableOrchestrationContext backupContext)
        {
            var req = backupContext.GetInput<InpaintRequest>();

            return 100;
        }
    }
}
