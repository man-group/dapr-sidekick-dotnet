using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

namespace AppConfigurationSample.Controllers;

[ApiController]
[Route("[controller]")]
public class SampleController : ControllerBase
{
    [HttpGet(Name = "GetSecret")]
    public async Task<IActionResult> Get([FromServices] DaprClient daprClient, [FromServices] IConfiguration configuration)
    {
        // Can read secrets by using the client or IConfiguration through DI as well
        var clientSecrets = await daprClient.GetSecretAsync("localsecretstore", "secret");
        var clientSecret = string.Join(",", clientSecrets.Select(d => d.Value));

        var configurationSecret = configuration.GetSection("secret").Value;

        return Ok(new
        {
            SecretFromClient = clientSecret,
            SecretFromConfiguration = configurationSecret
        });
    }
}
