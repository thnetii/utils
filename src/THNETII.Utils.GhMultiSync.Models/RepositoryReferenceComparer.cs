using System;
using System.Collections.Generic;

namespace THNETII.Utils.GhMultiSync.Models
{
    public class RepositoryReferenceComparer : IEqualityComparer<RepositoryReference>
    {
        public static RepositoryReferenceComparer Instance { get; } =
            new RepositoryReferenceComparer();

        private RepositoryReferenceComparer() { }

        public bool Equals(RepositoryReference x, RepositoryReference y)
        {
            return ReferenceEquals(x, y) ||
                (
                    string.Equals(x?.RepositoryOwner, y?.RepositoryOwner, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x?.RepositoryName, y?.RepositoryName, StringComparison.OrdinalIgnoreCase)
                );
        }

        public int GetHashCode(RepositoryReference obj)
            => obj?.RepositoryName?.GetHashCode() ?? 0;
    }
}
