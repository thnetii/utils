using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace THNETII.Utils.GhMultiSync.Models.Yaml
{
    public class SourceGroupSpec
    {
        [YamlMember(Alias = "inherits")]
        public List<string> Inherits { get; set; }
        [YamlMember(Alias = "repositories")]
        public List<SourceRepositorySpec> Repositories { get; set; }
    }
}
