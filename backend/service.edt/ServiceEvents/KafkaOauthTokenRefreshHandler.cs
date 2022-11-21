namespace edt.service.ServiceEvents;
using Confluent.Kafka;
using IdentityModel.Client;

public class KafkaOauthTokenRefreshHandler
{
    /// <summary>
    /// create a reusable method for get accesstoken and refreshhandler for kafka clients in production
    /// </summary>
    /// <param name="producer"></param>
    /// <param name="config"></param>
    private async void OauthTokenRefreshCallback(IClient client, string config)
    {
        try
        {
            var accessTokenClient = new HttpClient();
            var accessToken = await accessTokenClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = "https://sso-dev-5b7aa5-dev.apps.silver.devops.gov.bc.ca/auth/realms/DEMSPOC/protocol/openid-connect/token",
                ClientId = "",
                ClientSecret = "",
            });
            client.OAuthBearerSetToken(accessToken.AccessToken, accessToken.ExpiresIn, null);
        }
        catch (Exception ex)
        {
            client.OAuthBearerSetTokenFailure(ex.ToString());
        }
    }
}
