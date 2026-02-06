using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;
using System;
using System.IO;
using System.Linq;

namespace WebApplication1.Tests;

public class AuthFlowTests
{
    private sealed class AuthFactory : WebApplicationFactory<WebApplication1.Startup>
    {
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"tca_auth_{Guid.NewGuid():N}.db");

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, cfg) =>
            {
                cfg.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnectionSqlite"] = $"Data Source={_dbPath}",
                    ["AuthOptions:Issuer"] = "TelegramChatAnalyzer.Auth",
                    ["AuthOptions:Audience"] = "telegram-chat-analyzer",
                    ["AuthOptions:Secret"] = "CHANGE_ME_LONG_RANDOM_SECRET_FOR_DEV_ONLY",
                    ["AuthOptions:Lifetime"] = "900",
                    ["AuthOptions:RefreshLifetimeDays"] = "30",
                    ["AuthOptions:ServiceLifetime"] = "900",
                    ["ServiceClients:Clients:TelegramBot:ClientId"] = "telegram-bot",
                    ["ServiceClients:Clients:TelegramBot:ClientSecret"] = "CHANGE_ME_DEV_BOT_SECRET",
                    ["ServiceClients:Clients:TelegramBot:Scopes:0"] = "tca.api",
                });
            });

            return base.CreateHost(builder);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            try { File.Delete(_dbPath); } catch { /* ignore */ }
        }
    }

    [Fact]
    public async Task Register_Login_Me_Refresh_Logout_Works()
    {
        await using var factory = new AuthFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var email = $"tca_{Guid.NewGuid():N}@example.com";
        var password = "Passw0rdA!";

        // register
        var reg = await client.PostAsJsonAsync("/api/auth/register", new { email, password });
        if (reg.StatusCode != HttpStatusCode.OK)
        {
            var body = await reg.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Register failed: {(int)reg.StatusCode} {reg.StatusCode}. Body: {body}");
        }

        // login (sets refresh cookie)
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var refreshCookie = ExtractRefreshCookie(login);
        var loginJson = await login.Content.ReadFromJsonAsync<JsonElement>();
        var access1 = loginJson.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(access1));

        // me requires bearer
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access1);
        var me = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);

        // refresh rotates and returns new access (uses cookie)
        client.DefaultRequestHeaders.Authorization = null;
        var refreshReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh")
        {
            Content = JsonContent.Create(new { })
        };
        refreshReq.Headers.Add("Cookie", refreshCookie);
        var refresh = await client.SendAsync(refreshReq);
        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        refreshCookie = ExtractRefreshCookie(refresh);
        var refreshJson = await refresh.Content.ReadFromJsonAsync<JsonElement>();
        var access2 = refreshJson.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(access2));
        Assert.NotEqual(access1, access2);

        // logout revokes refresh
        var logoutReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout")
        {
            Content = JsonContent.Create(new { })
        };
        logoutReq.Headers.Add("Cookie", refreshCookie);
        var logout = await client.SendAsync(logoutReq);
        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);

        // refresh should now fail
        var refreshReq2 = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh")
        {
            Content = JsonContent.Create(new { })
        };
        refreshReq2.Headers.Add("Cookie", refreshCookie);
        var refresh2 = await client.SendAsync(refreshReq2);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh2.StatusCode);
    }

    [Fact]
    public async Task ClientCredentials_Returns_Service_Token()
    {
        await using var factory = new AuthFactory();
        using var client = factory.CreateClient();

        var res = await client.PostAsJsonAsync("/api/auth/token", new
        {
            clientId = "telegram-bot",
            clientSecret = "CHANGE_ME_DEV_BOT_SECRET"
        });

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var json = await res.Content.ReadFromJsonAsync<JsonElement>();
        var token = json.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    private static string ExtractRefreshCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values))
            throw new Xunit.Sdk.XunitException("Missing Set-Cookie header.");

        var cookie = values.FirstOrDefault(v => v.StartsWith("tca.refresh=", StringComparison.OrdinalIgnoreCase));
        if (cookie == null)
            throw new Xunit.Sdk.XunitException("Missing tca.refresh cookie.");

        // Return only the name=value pair for the Cookie header.
        return cookie.Split(';', 2)[0];
    }
}
