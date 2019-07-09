using System.Threading;

namespace THNETII.Utils.GhMultiSync.Models
{
    public class RepositoryContentChangeTracker
    {
        public SemaphoreSlim Lock { get; } = new SemaphoreSlim(1);
        public RepositoryReference Reference { get; set; }
    }
}
