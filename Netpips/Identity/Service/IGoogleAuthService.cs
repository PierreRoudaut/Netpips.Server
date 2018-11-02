using System.Threading.Tasks;
using Google.Apis.Auth;

namespace Netpips.Identity.Service
{
    public interface IGoogleAuthService
    {
        Task<GoogleJsonWebSignature.Payload> ValidateAsync(string idToken);
    }

    public class GoogleAuthService: IGoogleAuthService
    {
        public Task<GoogleJsonWebSignature.Payload> ValidateAsync(string idToken)
        {
            return GoogleJsonWebSignature.ValidateAsync(idToken);
        }
    }
}