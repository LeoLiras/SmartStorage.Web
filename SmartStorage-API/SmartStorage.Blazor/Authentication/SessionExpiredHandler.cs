using System.Net;

namespace SmartStorage.Blazor.Authentication
{
    public class SessionExpiredHandler : DelegatingHandler
    {
        private readonly SessionExpiration session;

        public SessionExpiredHandler(SessionExpiration session)
        {
            this.session = session;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized
                && request.Headers.Authorization is not null
                && !request.RequestUri.AbsolutePath.EndsWith("/signin", StringComparison.OrdinalIgnoreCase))
                _ = session.NotifyExpired();

            return response;
        }
    }
}
