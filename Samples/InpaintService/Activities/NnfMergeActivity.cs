using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Zavolokas.ImageProcessing.PatchMatch;
using Zavolokas.Structures;

namespace InpaintService.Activities
{
    public static class NnfMergeActivity
    {
        public const string Name = "MergeNnfs";

        [FunctionName(Name)]
        public static Task MergeNnfs([ActivityTrigger] (string[] nnfs, string[] mappings, string nnf, string container, string mapping) input)
        {
            return Task.Run(() =>
            {
                var storage = StorageFactory.Create();
                storage.OpenContainer(input.container);
                var nnfState = storage.Read<NnfState>(input.nnfs[0]);
                var destNnf = new Nnf(nnfState);

                var mappingState = storage.Read<Area2DMapState>(input.mappings[0]);
                var destMapping = new Area2DMap(mappingState);

                var mapBuilder = new Area2DMapBuilder();

                for (int nnfIndex = 1; nnfIndex < input.nnfs.Length; nnfIndex++)
                {
                    nnfState = storage.Read<NnfState>(input.nnfs[nnfIndex]);
                    var srcNnf = new Nnf(nnfState);

                    mappingState = storage.Read<Area2DMapState>(input.mappings[nnfIndex]);
                    var srcMapping = new Area2DMap(mappingState);

                    destNnf.Merge(srcNnf, destMapping, srcMapping);

                    destMapping = mapBuilder
                        .InitNewMap(destMapping)
                        .AddMapping(srcMapping)
                        .Build();

                }

                var nnfData = JsonConvert.SerializeObject(destNnf.GetState());
                storage.SaveJson(nnfData, input.nnf);
                foreach (var nnf in input.nnfs)
                {
                    storage.SaveJson(nnfData, nnf);
                }
            });
        }
    }
}