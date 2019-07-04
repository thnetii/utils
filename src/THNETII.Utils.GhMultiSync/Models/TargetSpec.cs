using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace THNETII.Utils.GhMultiSync.Models
{
    public class TargetSpec : RepositoryReferenceSpec
    {
        [YamlMember(Alias = "groups")]
        public List<string> Groups { get; set; }
    }
}
