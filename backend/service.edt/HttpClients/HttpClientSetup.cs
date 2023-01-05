namespace edt.service.HttpClients;

using System.Net.Http.Headers;
using edt.service.HttpClients.Services.EdtCore;
using EdtService.Extensions;
using IdentityModel.Client;
using Serilog;

public static class HttpClientSetup
{
    public static IServiceCollection AddHttpClients(this IServiceCollection services, EdtServiceConfiguration config)
    {
        services.AddHttpClient<IAccessTokenClient, AccessTokenClient>();

        //services.AddHttpClientWithBaseAddress<IAddressAutocompleteClient, AddressAutocompleteClient>(config.AddressAutocompleteClient.Url);

        Log.Logger.Information("Using EDT enpoint {0}", config.EdtClient.Url);

        services.AddHttpClientWithBaseAddress<IEdtClient, EdtClient>(config.EdtClient.Url)
            .ConfigureHttpClient(c => c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.EdtClient.ApiKey));

        return services;
    }

    public static IHttpClientBuilder AddHttpClientWithBaseAddress<TClient, TImplementation>(this IServiceCollection services, string baseAddress)
        where TClient : class
        where TImplementation : class, TClient
        => services.AddHttpClient<TClient, TImplementation>(client => client.BaseAddress = new Uri(baseAddress.EnsureTrailingSlash()));

    public static IHttpClientBuilder WithBearerToken<T>(this IHttpClientBuilder builder, T credentials) where T : ClientCredentialsTokenRequest
    {
        builder.Services.AddSingleton(credentials)
            .AddTransient<BearerTokenHandler<T>>();

        builder.AddHttpMessageHandler<BearerTokenHandler<T>>();

        return builder;
    }
}
