using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace THNETII.Utils.GhMultiSync.Models.Yaml
{
    public class SourceRepositorySpec : RepositoryReference
    {
        [YamlMember(Alias = "copyFiles")]
        public List<PathGroupSpec> CopyFiles { get; set; }

        [YamlMember(Alias = "emptyFiles")]
        public List<PathGroupSpec> EmptyFiles { get; set; }
    }
}
