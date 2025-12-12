namespace AFHPNewsFunctions.Services
{
    public interface IEmailSender
    {
        /// <param name="email">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="htmlMessage">The body of the email which may contain HTML tags. Do not double encode this.</param>
        /// <returns></returns>
        Task<string?> SendEmailAsync(string email, string subject, string htmlMessage);
        Task<string?> SendEmailWithDelayAsync(string email, string subject, string htmlMessage, int minDelayMS = 1000, int maxDelayMS = 3000);
    }
}
