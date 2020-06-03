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

        internal string ToLogString()
        {
            var (owner, name, treeRef) = (RepositoryOwner, RepositoryName, TreeReference);
            var builder = new StringBuilder();
            builder.Append(owner).Append('/').Append(name);
            if (!string.IsNullOrEmpty(treeRef))
                builder.Append('@').Append(treeRef);
            return builder.ToString();
        }
    }
}
