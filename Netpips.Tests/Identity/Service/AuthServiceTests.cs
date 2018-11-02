using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Netpips.Core;
using Netpips.Core.Settings;
using Netpips.Identity.Service;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Identity.Service
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<IOptions<AuthSettings>> options;
        private Mock<ILogger<AuthService>> logger;
        private Mock<IGoogleAuthService> googleAuthService;

        [SetUp]
        public void Setup()
        {
            this.logger = new Mock<ILogger<AuthService>>();
            this.options = new Mock<IOptions<AuthSettings>>();
            this.googleAuthService = new Mock<IGoogleAuthService>();
        }

        [Test]
        public void ValidateGoogleIdTokenTest_CaseInvalidToken()
        {
            // invalid token
            this.options.SetupGet(x => x.Value).Returns(TestHelper.CreateAuthSettings());
            this.googleAuthService.Setup(x => x.ValidateAsync(It.IsAny<string>())).Throws<Exception>();
            var service = new AuthService(this.options.Object, this.logger.Object, this.googleAuthService.Object);
            Assert.AreEqual(false, service.ValidateGoogleIdToken("abcd", out _, out var err), "google id Token should be invalid");
            Assert.AreEqual(AuthError.InvalidToken, err);

        }

        [Test]
        public void ValidateGoogleIdTokenTest_CaseEmailNotVerified()
        {
            // email not verified
            var expectedPayload = new GoogleJsonWebSignature.Payload { EmailVerified = false };
            this.options.SetupGet(x => x.Value).Returns(TestHelper.CreateAuthSettings());
            this.googleAuthService.Setup(x => x.ValidateAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(expectedPayload));
            var service = new AuthService(this.options.Object, this.logger.Object, this.googleAuthService.Object);
            Assert.IsFalse(service.ValidateGoogleIdToken(It.IsAny<string>(), out var payload, out var err));
            Assert.IsFalse(payload.EmailVerified);
            Assert.AreEqual(AuthError.EmailNotVerified, err);
        }


        [Test]
        public void ValidateGoogleIdTokenTest_CaseWrongAudience()
        {
            // wrong audience

            this.options.SetupGet(x => x.Value).Returns(TestHelper.CreateAuthSettings());

            var expectedPayload = new GoogleJsonWebSignature.Payload { Audience = "abcd", EmailVerified = true };
            this.googleAuthService.Setup(x => x.ValidateAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(expectedPayload));

            var service = new AuthService(this.options.Object, this.logger.Object, this.googleAuthService.Object);
            Assert.IsFalse(service.ValidateGoogleIdToken(It.IsAny<string>(), out var payload, out var err));
            Assert.AreEqual(AuthError.WrongAudience, err);
        }

        [Test]
        public void ValidateGoogleIdTokenTest_CaseTokenExpired()
        {
            // wrong audience
            this.options.SetupGet(x => x.Value).Returns(TestHelper.CreateAuthSettings());

            var expectedPayload = new GoogleJsonWebSignature.Payload { ExpirationTimeSeconds = ExtensionMethods.ConvertToUnixTimestamp(DateTime.Now.Subtract(TimeSpan.FromHours(3))), EmailVerified = true, Audience = this.options.Object.Value.GoogleClientId };

            this.googleAuthService.Setup(x => x.ValidateAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(expectedPayload));


            var service = new AuthService(this.options.Object, this.logger.Object, this.googleAuthService.Object);
            Assert.IsFalse(service.ValidateGoogleIdToken(It.IsAny<string>(), out var payload, out var err));
            Assert.AreEqual(AuthError.TokenExpired, err);
        }

        [Test]
        public void ValidateGoogleIdTokenTest_CaseWrongIssuer()
        {
            // wrong audience
            this.options.SetupGet(x => x.Value).Returns(TestHelper.CreateAuthSettings());

            var expectedPayload = new GoogleJsonWebSignature.Payload { Issuer = "abcd", EmailVerified = true, ExpirationTimeSeconds = ExtensionMethods.ConvertToUnixTimestamp(DateTime.Now.AddHours(1)), Audience = this.options.Object.Value.GoogleClientId };

            this.googleAuthService.Setup(x => x.ValidateAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(expectedPayload));

            var service = new AuthService(this.options.Object, this.logger.Object, this.googleAuthService.Object);
            Assert.IsFalse(service.ValidateGoogleIdToken(It.IsAny<string>(), out var payload, out var err));
            Assert.AreEqual(AuthError.WrongIssuer, err);
        }

        [Test]
        public void ValidateGoogleIdTokenTest_CaseValid()
        {
            // wrong audience
            this.options.SetupGet(x => x.Value).Returns(TestHelper.CreateAuthSettings());

            var expectedPayload = new GoogleJsonWebSignature.Payload { Issuer = AuthService.Issuers.First(), EmailVerified = true, ExpirationTimeSeconds = ExtensionMethods.ConvertToUnixTimestamp(DateTime.Now.AddHours(1)), Audience = this.options.Object.Value.GoogleClientId };
            this.googleAuthService.Setup(x => x.ValidateAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(expectedPayload));
            var service = new AuthService(this.options.Object, this.logger.Object, this.googleAuthService.Object);
            Assert.IsTrue(service.ValidateGoogleIdToken(It.IsAny<string>(), out var payload, out var err));
        }

        [Test]
        public void GenerateAccessTokenTest()
        {
            this.options.SetupGet(x => x.Value).Returns(TestHelper.CreateAuthSettings());
            var service = new AuthService(this.options.Object, this.logger.Object, this.googleAuthService.Object);
            var token = service.GenerateAccessToken(TestHelper.Admin);
            Assert.NotNull(token);
        }
    }
}
