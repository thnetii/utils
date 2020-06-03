using Octokit;

namespace THNETII.Utils.GhMultiSync.Models.Yaml
{
    public class ConditionDestinationExistsSpec : ConditionSpec
    {
        public override bool Evaluate(string sourcePath,
            RepositoryReference sourceRepo, RepositoryContent sourceContent,
            string targetPath, RepositoryReference targetRepo,
            RepositoryContent targetContent)
        {
            return !(targetContent is null);
        }
    }
}
