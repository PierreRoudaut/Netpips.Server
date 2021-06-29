using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Netpips.Core;
using Netpips.Core.Settings;
using Netpips.Identity.Model;

namespace Netpips.Identity.Service
{
    public enum AuthError
    {
        InvalidToken,
        WrongAudience,
        WrongIssuer,
        UnregisteredUser,
        EmailNotVerified,
        TokenExpired
    }

    public class AuthErrorResponse
    {
        public string Message { get; set; }
        public AuthError Error { get; set; }
        public int HttpCode { get; set; }
    }

    public static class AppClaims
    {
        public const string Picture = "picture";
    }

    public class AuthService : IAuthService
    {
        private readonly AuthSettings settings;

        private readonly ILogger<AuthService> logger;
        public static readonly string[] Issuers = { "accounts.google.com", "https://accounts.google.com" };

        private readonly IGoogleAuthService googleAuthService;

        public AuthService(IOptions<AuthSettings> options, ILogger<AuthService> logger, IGoogleAuthService googleAuthService)
        {
            settings = options.Value;
            this.logger = logger;
            this.googleAuthService = googleAuthService;
        }

        public bool ValidateGoogleIdToken(string idToken, out GoogleJsonWebSignature.Payload payload, out AuthError error)
        {
            error = AuthError.InvalidToken;
            AuthError? err = null;
            payload = null;
            try
            {
                payload = googleAuthService.ValidateAsync(idToken).Result;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to validate GoogleToken");
                logger.LogError(e.Message);
                err = AuthError.InvalidToken;
                return false;
            }
            if (!payload.EmailVerified)
            {
                err = AuthError.EmailNotVerified;
            }
            else if (payload.Audience.ToString() != settings.GoogleClientId)
            {
                err = AuthError.WrongAudience;
            }
            else if (payload.ExpirationTimeSeconds != null && ExtensionMethods.ConvertFromUnixTimestamp(payload.ExpirationTimeSeconds.Value) <= DateTime.Now)
            {
                err = AuthError.TokenExpired;
            }
            else if (!Issuers.Contains(payload.Issuer))
            {
                err = AuthError.WrongIssuer;
            }
            if (err.HasValue)
            {
                error = err.Value;
                logger.LogWarning("VerifyIdToken: KO ({0})", err);
                return false;
            }
            logger.LogInformation("VerifyIdToken: OK ({0})", payload.Email);
            return true;
        }

        public string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.GivenName, user.GivenName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.FamilyName),
                new Claim(AppClaims.Picture, user.Picture),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(settings.JwtExpireMinutes));

            var token = new JwtSecurityToken(
                issuer: settings.JwtIssuer,
                audience: settings.JwtIssuer,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
