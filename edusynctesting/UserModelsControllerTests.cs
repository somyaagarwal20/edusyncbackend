using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using EduSyncwebapi.Controllers;
using EduSyncwebapi.Data;
using EduSyncwebapi.Models;
using EduSyncwebapi.Dtos;
using MockQueryable.Moq;

namespace edusynctesting
{
    [TestFixture]
    public class UserModelsControllerTests
    {
        private Mock<AppDbContext> _mockContext;
        private UserModelsController _controller;
        private Mock<DbSet<UserModel>> _mockDbSet;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<AppDbContext>();
            _mockDbSet = new Mock<DbSet<UserModel>>();
            _controller = new UserModelsController(_mockContext.Object);
        }

        [Test]
        public async Task GetUserModel_WithValidId_ReturnsUser()
        {
            var userId = Guid.NewGuid();
            var user = new UserModel { UserId = userId, Name = "Test User", Email = "test@test.com", Role = "Student" };
            var users = new List<UserModel> { user };
            var mockDbSet = users.AsQueryable().BuildMockDbSet();
            mockDbSet.Setup(d => d.FindAsync(userId)).ReturnsAsync(user);
            _mockContext.Setup(c => c.UserModels).Returns(mockDbSet.Object);

            var result = await _controller.GetUserModel(userId);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            if (result.Result is OkObjectResult okResult && okResult.Value is UserDto returnedUser)
            {
                Assert.That(returnedUser!.UserId, Is.EqualTo(userId));
            }
            else
            {
                Assert.Fail("Result is not OkObjectResult with UserDto");
            }
        }

        [Test]
        public async Task GetUserModel_WithInvalidId_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            var users = new List<UserModel>();
            var mockDbSet = users.AsQueryable().BuildMockDbSet();
            mockDbSet.Setup(d => d.FindAsync(userId)).ReturnsAsync((UserModel)null);
            _mockContext.Setup(c => c.UserModels).Returns(mockDbSet.Object);

            var result = await _controller.GetUserModel(userId);

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task PostUserModel_WithValidData_ReturnsCreatedUser()
        {
            var userDto = new UserDto
            {
                Name = "New User",
                Email = "new@test.com",
                Role = "Student",
                Password = "password123"
            };

            _mockContext.Setup(c => c.UserModels.Add(It.IsAny<UserModel>()));
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var result = await _controller.PostUserModel(userDto);

            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            if (result.Result is CreatedAtActionResult createdResult && createdResult.Value is UserDto returnedUser)
            {
                Assert.That(returnedUser!.Name, Is.EqualTo(userDto.Name));
                Assert.That(returnedUser!.Email, Is.EqualTo(userDto.Email));
            }
            else
            {
                Assert.Fail("Result is not CreatedAtActionResult with UserDto");
            }
        }

        [Test]
        public async Task Login_WithValidCredentials_ReturnsUserData()
        {
            var email = "test@test.com";
            var password = "password123";
            var user = new UserModel
            {
                UserId = Guid.NewGuid(),
                Email = email,
                PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password))
            };

            var users = new List<UserModel> { user };
            var mockDbSet = users.AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.UserModels).Returns(mockDbSet.Object);

            var loginRequest = new LoginRequest { Email = email, Password = password };

            var result = await _controller.Login(loginRequest);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            if (result.Result is OkObjectResult okResult && okResult.Value is LoginResponse loginResponse)
            {
                Assert.That(loginResponse.Email, Is.EqualTo(email));
            }
            else
            {
                Assert.Fail("Result is not OkObjectResult with LoginResponse");
            }
        }

        [Test]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            var email = "test@test.com";
            var password = "wrongpassword";
            var user = new UserModel
            {
                UserId = Guid.NewGuid(),
                Email = email,
                PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("correctpassword"))
            };

            var users = new List<UserModel> { user };
            var mockDbSet = users.AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.UserModels).Returns(mockDbSet.Object);

            var loginRequest = new LoginRequest { Email = email, Password = password };

            var result = await _controller.Login(loginRequest);

            Assert.That(result.Result, Is.InstanceOf<UnauthorizedObjectResult>());
        }
    }
} 