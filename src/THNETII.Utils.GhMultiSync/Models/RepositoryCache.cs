using System;
using System.Collections.Generic;
using Octokit;

namespace THNETII.Utils.GhMultiSync.Models
{
    public class RepositoryContentEntry
    {
        public RepositoryContent Leaf { get; set; }
        public IReadOnlyList<RepositoryContent> Contents { get; set; }
    }
}
