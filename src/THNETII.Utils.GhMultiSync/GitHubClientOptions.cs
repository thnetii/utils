using System;

namespace THNETII.Utils.GhMultiSync
{
    public class GitHubClientOptions
    {
        public const string ConfigurationSectionName = "GitHub";
        private string? accessToken;

        public string? AccessToken
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(accessToken))
                    return accessToken;
                var variable = AccessTokenVariable;
                if (!string.IsNullOrWhiteSpace(variable))
                    return Environment.GetEnvironmentVariable(variable);
                return null;
            }
            set => accessToken = value;
        }
        public string? AccessTokenVariable { get; set; }
        public string? AuthenticationType { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
    }
}
