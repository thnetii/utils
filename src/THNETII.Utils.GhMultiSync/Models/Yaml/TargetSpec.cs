using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace THNETII.Utils.GhMultiSync.Models.Yaml
{
    public class TargetSpec : RepositoryReference
    {
        [YamlMember(Alias = "groups")]
        public List<string> Groups { get; set; }
    }
}
