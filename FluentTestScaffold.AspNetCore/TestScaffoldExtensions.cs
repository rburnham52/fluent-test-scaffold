using FluentTestScaffold.Core;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FluentTestScaffold.AspNetCore;

public static class TestScaffoldExtensions
{
    public static TestScaffold WithWebApplicationFactory<TWebApplicationFactory, TEntry>(
        this TestScaffold testScaffold,
        TWebApplicationFactory webApplicationFactory)
        where TWebApplicationFactory : WebApplicationFactory<TEntry>
        where TEntry : class
    {
        testScaffold.TestScaffoldContext.Set(webApplicationFactory);

        return testScaffold.WithServiceProvider(webApplicationFactory.Services);
    }

    public static HttpClient GetWebApplicationHttpClient<TWebApplicationFactory, TEntry>(this TestScaffold testScaffold)
        where TWebApplicationFactory : WebApplicationFactory<TEntry>
        where TEntry : class
    {
        if (!testScaffold.TestScaffoldContext.TryGetValue<TWebApplicationFactory>(out var webApplicationFactory))
        {
            throw new InvalidOperationException(
                "A call to testScaffold.WithWebApplicationFactory<TFactory, TEntryPoint>(factory) is required" +
                " to initialise the a web application factory before a HttpClient can be created");
        }

        var key = CreateHttpClientKey<TWebApplicationFactory, TEntry>();

        // Maintain the same HttpClient for each WebApplicationFactory, this is we can maintain any cookies or sessions
        // create between http requests
        if (!testScaffold.TestScaffoldContext.TryGetValue<HttpClient>(key, out var httpClient))
        {
            httpClient = webApplicationFactory!.CreateClient(
                new WebApplicationFactoryClientOptions()
                {
                    HandleCookies = true,
                    AllowAutoRedirect = true
                });

            testScaffold.TestScaffoldContext.Set(httpClient, key);
        }

        return httpClient!;
    }

    private static string CreateHttpClientKey<TWebApplicationFactory, TEntry>()
        where TWebApplicationFactory : WebApplicationFactory<TEntry>
        where TEntry : class
    {
        return $"HttpClient__{typeof(TWebApplicationFactory).FullName}";
    }
}