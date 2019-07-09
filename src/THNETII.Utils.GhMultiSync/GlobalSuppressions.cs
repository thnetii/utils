
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Globalization", "CA1303: Do not pass literals as localized parameters")]
[assembly: SuppressMessage("Usage", "CA2227: Collection properties should be read only",
    Scope = "NamespaceAndChildren",
    Target = nameof(THNETII) + "."
        + nameof(THNETII.Utils) + "."
        + nameof(THNETII.Utils.GhMultiSync) + "."
        + nameof(THNETII.Utils.GhMultiSync.Models) + "."
        + nameof(THNETII.Utils.GhMultiSync.Models.Yaml)
    )]


