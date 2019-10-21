using System.Text;
using YamlDotNet.Serialization;

namespace THNETII.Utils.GhMultiSync.Models
{
    public class RepositoryReference
    {
        [YamlMember(Alias = "repoOwner")]
        public string RepositoryOwner { get; set; }

        [YamlMember(Alias = "repoName")]
        public string RepositoryName { get; set; }

        [YamlMember(Alias = "treeRef")]
        public string TreeReference { get; set; }
    }
}
