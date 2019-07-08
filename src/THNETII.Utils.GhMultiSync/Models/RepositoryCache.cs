using System;
using System.Collections.Generic;
using Octokit;

namespace THNETII.Utils.GhMultiSync.Models
{
    public class RepositoryCache
    {
        public Dictionary<string, RepositoryCacheEntry> Entries { get; set; } =
            new Dictionary<string, RepositoryCacheEntry>(StringComparer.OrdinalIgnoreCase);
    }

    public class RepositoryCacheEntry
    {
        public RepositoryContent Leaf { get; set; }
        public IReadOnlyList<RepositoryContent> Contents { get; set; }
    }
}
