using System.Collections.Generic;
using System.Linq;
using Octokit;

namespace THNETII.Utils.GhMultiSync.Models.Yaml
{
    public class ConditionAndSpec : ConditionSpec
    {
        public List<ConditionSpec> Conditions { get; set; }

        public override bool Evaluate(string sourcePath,
            RepositoryReference sourceRepo, RepositoryContent sourceContent,
            string targetPath, RepositoryReference targetRepo,
            RepositoryContent targetContent)
        {
            foreach (var condSpec in Conditions ?? Enumerable.Empty<ConditionSpec>())
            {
                bool condItem = condSpec?.Evaluate(
                    sourcePath, sourceRepo, sourceContent,
                    targetPath, targetRepo, targetContent
                    ) ?? true;
                if (!condItem)
                    return false;
            }
            return true;
        }
    }
}
