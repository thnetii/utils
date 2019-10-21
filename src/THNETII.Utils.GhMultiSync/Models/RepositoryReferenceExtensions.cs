using System.Text;

namespace THNETII.Utils.GhMultiSync.Models
{
    internal static class RepositoryReferenceExtensions
    {
        internal static string ToLogString(this RepositoryReference r)
        {
            var (owner, name, treeRef) = (r.RepositoryOwner, r.RepositoryName, r.TreeReference);
            var builder = new StringBuilder();
            builder.Append(owner).Append('/').Append(name);
            if (!string.IsNullOrEmpty(treeRef))
                builder.Append('@').Append(treeRef);
            return builder.ToString();
        }
    }
}
