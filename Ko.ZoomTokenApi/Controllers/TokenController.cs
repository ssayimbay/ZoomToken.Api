using Ko.ZoomTokenApi.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Ko.ZoomTokenApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ZoomCredentialsSettings _zoomCredentialsSettings;
        public TokenController(IOptions<ZoomCredentialsSettings> zoomCredentialsSettings)
        {
            _zoomCredentialsSettings = zoomCredentialsSettings.Value;
        }

        [HttpGet]
        public IActionResult Generate([FromQuery] string topic, string password, string sessionKey, string userIdentity)
        {
            var header = new
            {
                alg = "HS256",
                typ = "JWT"
            };

            var headerPart = Base64UrlEncoder.Encode(JsonConvert.SerializeObject(header));

            var iat = Math.Round((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds, 0);
            var payload = new
            {
                app_key = _zoomCredentialsSettings.SdkKey,
                iat = iat,
                exp = iat + 60 * 60 * 2,
                tpc = topic,
                pwd = password,
                user_identity = userIdentity,
                session_key = sessionKey
            };

            var payloadPart = Base64UrlEncoder.Encode(JsonConvert.SerializeObject(payload));

            var secret = _zoomCredentialsSettings.SdkSecret;
            var sha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{headerPart}.{payloadPart}"));
            var hash = Base64UrlEncoder.Encode(hashBytes);

            var jwt = $"{headerPart}.{payloadPart}.{hash}";

            return Ok(jwt);
        }
    }
}
