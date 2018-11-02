using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Netpips.Identity.Authorization;
using Netpips.Identity.Controller;
using Netpips.Identity.Model;
using Netpips.Identity.Service;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Identity.Controller
{
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<IUserRepository> repository;
        private Mock<ILogger<UserController>> logger;
        private Mock<IUserAdministrationService> service;

        [SetUp]
        public void Setup()
        {
            repository = new Mock<IUserRepository>();
            logger = new Mock<ILogger<UserController>>();
            service = new Mock<IUserAdministrationService>();
        }


        [Test]
        public void GetUsers()
        {
            var adminUser = TestHelper.Admin;
            var administrableUsers = new List<User>
            {
                TestHelper.ItemOwner,
                TestHelper.NotAnItemOwner
            };

            var mocker = new AutoMocker();
            mocker.Use<IUserRepository>(x => x.GetUsers(adminUser.Id) == administrableUsers);
            var ctrl = mocker.CreateInstance<UserController>();
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = adminUser.MapToClaimPrincipal()
                }
            };

            var res = ctrl.GetUsers();
            mocker.GetMock<IUserRepository>().Verify(x => x.GetUsers(adminUser.Id), Times.Once);
            Assert.AreEqual(administrableUsers.Count, ((List<User>)res.Value).Count);
        }

        [Test]

        public void CreateUser_CaseNotAllowed()
        {
            var adminUser = TestHelper.Admin;
            var userToCreate = new User();
            var mocker = new AutoMocker();
            mocker.Use<IUserAdministrationService>(x => x.CanUpdate(It.IsAny<Role>(), It.IsAny<Role>()) == false);
            var ctrl = mocker.CreateInstance<UserController>();
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = adminUser.MapToClaimPrincipal()
                }
            };
            var res = ctrl.CreateUser(userToCreate);
            Assert.AreEqual(403, res.StatusCode);
        }

        [Test]
        public void CreateUser_CaseAddressInUser()
        {
            var adminUser = TestHelper.Admin;
            var userToCreate = new User();
            var mocker = new AutoMocker();
            mocker.Use<IUserAdministrationService>(x => x.CanUpdate(It.IsAny<Role>(), It.IsAny<Role>()));
            mocker.Use<IUserRepository>(x => x.CreateUser(userToCreate) == false);
            var ctrl = mocker.CreateInstance<UserController>();
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = adminUser.MapToClaimPrincipal() }
            };
            var res = ctrl.CreateUser(userToCreate);
            Assert.AreEqual(400, res.StatusCode);
        }

        [Test]
        public void CreateUser_CaseOk()
        {
            var adminUser = TestHelper.Admin;
            var userToCreate = new User();
            var mocker = new AutoMocker();
            mocker.Use<IUserAdministrationService>(x => x.CanUpdate(It.IsAny<Role>(), It.IsAny<Role>()));
            mocker.Use<IUserRepository>(x => x.CreateUser(It.IsAny<User>()) == true);
            var ctrl = mocker.CreateInstance<UserController>();
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = adminUser.MapToClaimPrincipal() }
            };
            var res = ctrl.CreateUser(userToCreate);
            Assert.AreEqual(201, res.StatusCode);
        }

        [Test]
        public void UpdateUser_CaseNotFound()
        {
            var adminUser = TestHelper.Admin;
            var mocker = new AutoMocker();
            var userToUpdate = new User
            {
                Id = Guid.NewGuid()
            };
            mocker.Use<IUserRepository>(x => x.FindUser(It.IsAny<Guid>()) == null);
            var ctrl = mocker.CreateInstance<UserController>();
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = adminUser.MapToClaimPrincipal()
                }
            };
            var res = ctrl.UpdateUser(userToUpdate);
            Assert.AreEqual(404, res.StatusCode);
        }

        [Test]
        public void UpdateUser_CaseNotAllowed()
        {
            var adminUser = TestHelper.Admin;
            var userToUpdate = new User
            {
                Id = Guid.NewGuid()
            }; var mocker = new AutoMocker();
            mocker.Use<IUserRepository>(x => x.FindUser(It.IsAny<Guid>()) == userToUpdate);
            mocker.Use<IUserAdministrationService>(x => x.CanUpdate(It.IsAny<Role>(), It.IsAny<Role>()) == false);
            var ctrl = mocker.CreateInstance<UserController>();
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = adminUser.MapToClaimPrincipal()
                }
            };
            var res = ctrl.UpdateUser(userToUpdate);
            Assert.AreEqual(403, res.StatusCode);
        }

        [Test]
        public void UpdateUser_CaseOk()
        {
            var adminUser = TestHelper.Admin;
            var userToUpdate = new User();
            var mocker = new AutoMocker();
            mocker.Use<IUserRepository>(x => 
                x.FindUser(It.IsAny<Guid>()) == userToUpdate && 
                x.UpdateUser(It.IsAny<User>()));
            mocker.Use<IUserAdministrationService>(x => x.CanUpdate(It.IsAny<Role>(), It.IsAny<Role>(), It.IsAny<Role>()));
            var ctrl = mocker.CreateInstance<UserController>();
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = adminUser.MapToClaimPrincipal() }
            };
            var res = ctrl.UpdateUser(userToUpdate);
            Assert.AreEqual(200, res.StatusCode);
        }

        [Test]
        public void UpdateUser_CaseAddressInUse()
        {
            var adminUser = TestHelper.Admin;
            var userToUpdate = new User();
            var mocker = new AutoMocker();
            mocker.Use<IUserRepository>(x =>
                x.FindUser(It.IsAny<Guid>()) == userToUpdate &&
                !x.UpdateUser(It.IsAny<User>()));
            mocker.Use<IUserAdministrationService>(x => x.CanUpdate(It.IsAny<Role>(), It.IsAny<Role>(), It.IsAny<Role>()));
            var ctrl = mocker.CreateInstance<UserController>();
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = adminUser.MapToClaimPrincipal() }
            };
            var res = ctrl.UpdateUser(userToUpdate);
            Assert.AreEqual(400, res.StatusCode);
        }

        [Test]
        public void DeleteUser_CaseNotFound()
        {
            var adminUser = TestHelper.Admin;
            var mocker = new AutoMocker();
            var userToDelete = new User
            {
                Id = Guid.NewGuid()
            };
            mocker.Use<IUserRepository>(x => x.FindUser(It.IsAny<Guid>()) == null);
            var ctrl = mocker.CreateInstance<UserController>();
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = adminUser.MapToClaimPrincipal()
                }
            };
            var res = ctrl.DeleteUser(userToDelete.Id);
            Assert.AreEqual(404, res.StatusCode);
        }


        [Test]
        public void DeleteUser_CaseNotAllowed()
        {
            var adminUser = TestHelper.Admin;
            var userToDelete = new User
            {
                Id = Guid.NewGuid()
            }; var mocker = new AutoMocker();
            mocker.Use<IUserRepository>(x => x.FindUser(It.IsAny<Guid>()) == userToDelete);
            mocker.Use<IUserAdministrationService>(x => x.CanUpdate(It.IsAny<Role>(), It.IsAny<Role>()) == false);
            var ctrl = mocker.CreateInstance<UserController>();
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = adminUser.MapToClaimPrincipal()
                }
            };
            var res = ctrl.DeleteUser(userToDelete.Id);
            Assert.AreEqual(403, res.StatusCode);
        }

        [Test]
        public void DeleteUser_CaseOk()
        {
            var adminUser = TestHelper.Admin;
            var userToDelete = new User
            {
                Id = Guid.NewGuid()
            }; var mocker = new AutoMocker();
            mocker.Use<IUserRepository>(x => x.FindUser(It.IsAny<Guid>()) == userToDelete);
            mocker.Use<IUserAdministrationService>(x => x.CanUpdate(It.IsAny<Role>(), It.IsAny<Role>()));
            var ctrl = mocker.CreateInstance<UserController>();
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = adminUser.MapToClaimPrincipal()
                }
            };
            var res = ctrl.DeleteUser(userToDelete.Id);
            Assert.AreEqual(200, res.StatusCode);
        }
    }
}