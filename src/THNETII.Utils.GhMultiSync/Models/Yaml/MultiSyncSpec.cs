using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace THNETII.Utils.GhMultiSync.Models.Yaml
{
    public class MultiSyncSpec
    {
        [YamlMember(Alias = "groups")]
        public Dictionary<string, SourceGroupSpec> Groups { get; set; }

        [YamlMember(Alias = "targets")]
        public List<TargetSpec> Targets { get; set; }
    }
}
