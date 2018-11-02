using Google.Apis.Auth;
using Netpips.Identity.Model;

namespace Netpips.Identity.Service
{
    public interface IAuthService
    {
        bool ValidateGoogleIdToken(string idToken, out GoogleJsonWebSignature.Payload payload, out AuthError err);
        string GenerateAccessToken(User user);
    }
}