using System.Collections.Generic;
using THNETII.Utils.GhMultiSync.Models;

namespace THNETII.Utils.GhMultiSync.Contexts
{
    public class SourceGroupEvaluationContext
    {
        public List<SourceGroupEvaluationContext> Inherits { get; internal set; }
        public List<SourceRepositorySpec> Repositories { get; internal set; }
    }
}
