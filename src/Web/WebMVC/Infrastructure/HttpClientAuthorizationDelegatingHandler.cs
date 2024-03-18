﻿namespace WebMVC.Infrastructure;

public class HttpClientAuthorizationDelegatingHandler
    : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<HttpClientAuthorizationDelegatingHandler> _logger;

    public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor, ILogger<HttpClientAuthorizationDelegatingHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authorizationHeader = _httpContextAccessor.HttpContext
            .Request.Headers["Authorization"];

        if (!string.IsNullOrEmpty(authorizationHeader))
        {
            request.Headers.Add("Authorization", new List<string>() { authorizationHeader });
        }

        var token = await GetToken();
        
        _logger.LogInformation("Retrieved token: {token}", token);

        if (token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    async Task<string> GetToken()
    {
        const string ACCESS_TOKEN = "access_token";

        return await _httpContextAccessor.HttpContext
            .GetTokenAsync(ACCESS_TOKEN);
    }
}
