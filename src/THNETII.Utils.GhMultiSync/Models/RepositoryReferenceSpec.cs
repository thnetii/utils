using YamlDotNet.Serialization;

namespace THNETII.Utils.GhMultiSync.Models
{
    public class RepositoryReferenceSpec
    {
        [YamlMember(Alias = "repoOwner")]
        public string RepositoryOwner { get; set; }

        [YamlMember(Alias = "repoName")]
        public string RepositoryName { get; set; }
    }
}
