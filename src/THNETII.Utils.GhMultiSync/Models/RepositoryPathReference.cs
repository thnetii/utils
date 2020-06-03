using System;

namespace THNETII.Utils.GhMultiSync.Models
{
    public class RepositoryPathReference : RepositoryReference
    {
        public string Path { get; set; }

        public override int GetHashCode() => HashCode.Combine(
            RepositoryOwner,
            RepositoryName,
            TreeReference,
            Path
            );

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case RepositoryPathReference other:
                    return RepositoryReferenceComparer.Instance.Equals(this, other)
                        && string.Equals(Path, other.Path, StringComparison.Ordinal);

                case null:
                default:
                    return false;
            }
        }
    }
}
