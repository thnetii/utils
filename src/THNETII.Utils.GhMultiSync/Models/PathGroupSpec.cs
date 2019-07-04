using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace THNETII.Utils.GhMultiSync.Models
{
    public class PathGroupSpec
    {
        [YamlMember(Alias = "sourcePaths")]
        public List<string> SourcePaths { get; set; }

        [YamlMember(Alias = "destinationPath")]
        public string DestinationPath { get; set; }

        public ConditionSpec Condition { get; set; }
    }
}
