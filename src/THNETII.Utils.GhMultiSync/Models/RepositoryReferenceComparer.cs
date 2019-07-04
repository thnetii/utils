using System;
using System.Collections.Generic;

namespace THNETII.Utils.GhMultiSync.Models
{
    public class RepositoryReferenceComparer : IEqualityComparer<RepositoryReferenceSpec>
    {
        public static RepositoryReferenceComparer Instance { get; } =
            new RepositoryReferenceComparer();

        private RepositoryReferenceComparer() { }

        public bool Equals(RepositoryReferenceSpec x, RepositoryReferenceSpec y)
        {
            return ReferenceEquals(x, y) ||
                (
                string.Equals(x?.RepositoryOwner, y?.RepositoryOwner, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x?.RepositoryName, y?.RepositoryName, StringComparison.OrdinalIgnoreCase)
                );
        }

        public int GetHashCode(RepositoryReferenceSpec obj)
            => obj?.RepositoryName?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0;
    }
}
