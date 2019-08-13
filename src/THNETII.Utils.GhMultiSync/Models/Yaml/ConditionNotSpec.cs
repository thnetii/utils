using Octokit;

namespace THNETII.Utils.GhMultiSync.Models.Yaml
{
    public class ConditionNotSpec : ConditionSpec
    {
        public ConditionSpec Condition { get; set; }

        public override bool Evaluate(string sourcePath,
            RepositoryReference sourceRepo, RepositoryContent sourceContent,
            string targetPath, RepositoryReference targetRepo,
            RepositoryContent targetContent)
        {
            var cond = Condition?.Evaluate(
                sourcePath, sourceRepo, sourceContent,
                targetPath, targetRepo, targetContent
                ) ?? true;
            return !cond;
        }
    }
}
