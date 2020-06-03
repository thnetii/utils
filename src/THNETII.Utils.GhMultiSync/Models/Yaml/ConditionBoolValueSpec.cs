using Octokit;

namespace THNETII.Utils.GhMultiSync.Models.Yaml
{
    public class ConditionBoolValueSpec : ConditionSpec
    {
        public bool Value { get; set; } = true;

        public override bool Evaluate(string sourcePath,
            RepositoryReference sourceRepo, RepositoryContent sourceContent,
            string targetPath, RepositoryReference targetRepo,
            RepositoryContent targetContent) => Value;
    }
}
