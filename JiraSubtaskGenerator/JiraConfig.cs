namespace JiraSubtaskGenerator
{
    public class JiraConfig
    {
        public JiraConfig(string jiraUrl, string jiraEmail, string jiraToken)
        {
            this.JiraUrl = ValidateJiraUrl(jiraUrl);
            this.JiraEmail = ValidateEmail(jiraEmail);
            this.JiraToken = ValidateToken(jiraToken);
        }

        private static string ValidateJiraUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Jira URL cannot be empty", nameof(url));

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                throw new ArgumentException("Jira URL is not a valid absolute URL", nameof(url));

            if (uri.Scheme != Uri.UriSchemeHttps)
                throw new ArgumentException("Jira URL must use https", nameof(url));

            if (!uri.Host.EndsWith(".atlassian.net", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Jira URL must be an Atlassian Cloud URL (*.atlassian.net)", nameof(url));

            return url;
        }

        private static string ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                    throw new ArgumentException("Email is not valid", nameof(email));
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Email is not valid", nameof(email), ex);
            }

            return email;
        }

        private static string ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty", nameof(token));

            if (token.Length < 10)
                throw new ArgumentException("Token looks too short to be a valid Jira API token", nameof(token));

            return token;
        }

        public string JiraUrl { get; private set; } = string.Empty;
        public string JiraEmail { get; private set; } = string.Empty;
        public string JiraToken { get; private set; } = string.Empty;
    }
}