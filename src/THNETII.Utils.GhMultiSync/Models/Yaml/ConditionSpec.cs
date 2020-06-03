using Octokit;

namespace THNETII.Utils.GhMultiSync.Models.Yaml
{
    public abstract class ConditionSpec
    {
        public abstract bool Evaluate(string sourcePath,
            RepositoryReference sourceRepo, RepositoryContent sourceContent,
            string targetPath, RepositoryReference targetRepo,
            RepositoryContent targetContent);
    }
}
