using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Netpips.Identity.Model;
using Netpips.Identity.Service;

namespace Netpips.Identity.Controller
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class AuthController : Microsoft.AspNetCore.Mvc.Controller
    {
        private static readonly List<AuthErrorResponse> AuthErrors =
            new List<AuthErrorResponse>
            {
                new AuthErrorResponse {Error = AuthError.InvalidToken, HttpCode = 400, Message = "Invalid Google Id token"},
                new AuthErrorResponse {Error = AuthError.EmailNotVerified, HttpCode = 400, Message = "Email not verified"},
                new AuthErrorResponse {Error = AuthError.TokenExpired, HttpCode = 400, Message = "Token expired"},
                new AuthErrorResponse {Error = AuthError.UnregisteredUser, HttpCode = 400, Message = "Unregistered user"},
                new AuthErrorResponse {Error = AuthError.WrongAudience, HttpCode = 400, Message = "Wrong audience"},
                new AuthErrorResponse {Error = AuthError.WrongIssuer, HttpCode = 400, Message = "Wrong issuer"},
            };

        private readonly IAuthService service;
        private readonly IUserRepository repository;
        private readonly ILogger<AuthController> logger;
        public AuthController(IUserRepository repository, IAuthService service, ILogger<AuthController> logger)
        {
            this.repository = repository;
            this.service = service;
            this.logger = logger;
        }

        [HttpPost("login", Name = "login")]
        [ProducesResponseType(403)]
        [ProducesResponseType(200)]
        [AllowAnonymous]
        public ObjectResult Login([FromServices] IAuthService authService, [FromBody] string idToken)
        {
            if (!authService.ValidateGoogleIdToken(idToken, out var payload, out var err))
            {
                var errReponse = AuthErrors.Single(x => x.Error == err);
                return StatusCode(errReponse.HttpCode, errReponse);
            }

            var user = repository.FindUser(payload.Email);
            if (user == null)
            {
                logger.LogWarning(payload.Email + " : unregistered user");
                return StatusCode(403, new AuthErrorResponse { Error = AuthError.UnregisteredUser, Message = "Unregistered user" });
            }
            user.UpdateInfos(payload);
            repository.UpdateUser(user);

            return StatusCode(200, authService.GenerateAccessToken(user));
        }
    }
}