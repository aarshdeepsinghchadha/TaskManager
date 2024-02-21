using Application.Common;
using Application.Interface;
using Application.TaskManager;
using Domain;
using Infrastructure.Services;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerBackendUnitTestCases
{
    [TestFixture]
    public class TaskManagerServiceTests
    {
        private ITaskManagerService _taskManagerService;
        private Mock<UserManager<AppUser>> _userManagerMock;
        private Mock<SignInManager<AppUser>> _signInManagerMock;
        private Mock<IResponseGeneratorService> _responseGeneratorServiceMock;
        private Mock<ITokenService> _tokenServiceMock;
        private Mock<IEmailSenderService> _emailSenderServiceMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<ILog> _logMock;
        private Mock<DataContext> _dataContext;
        [SetUp]
        public void SetUp()
         {
            _logMock = new Mock<ILog>();
            _configurationMock = new Mock<IConfiguration>();
            _emailSenderServiceMock = new Mock<IEmailSenderService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _responseGeneratorServiceMock = new Mock<IResponseGeneratorService>();
            _userManagerMock = new Mock<UserManager<AppUser>>(
                 Mock.Of<IUserStore<AppUser>>(),
                   null,
                   null,
                   null,
                   null,
                   null,
                   null,
                   null,
                   null);

            _signInManagerMock = new Mock<SignInManager<AppUser>>(
            _userManagerMock.Object,
                 Mock.Of<IHttpContextAccessor>(),
                 Mock.Of<IUserClaimsPrincipalFactory<AppUser>>(),
                 null,
                 null,
                 null,
                 null);

         
            _dataContext = new Mock<DataContext>();
            _dataContext.Setup(x => x.Users).Returns(Mock.Of<DbSet<AppUser>>());

            _taskManagerService = new TaskManagerService
                (
                _userManagerMock.Object, _signInManagerMock.Object,
                _tokenServiceMock.Object, _responseGeneratorServiceMock.Object,
                _emailSenderServiceMock.Object, _dataContext.Object, _configurationMock.Object
                );

         
        }


        [Test]

        public async Task LoginUserAsync_ValidCredentials_ReturnsSuccessResponse()
        {
            //Arrange 
            var loginDto = new LoginDto
            {
                Username = "ValidUsername",
                Password = "ValidPassword"
                
            };

            var user = new AppUser { UserName = "ValidUsername" };

            _userManagerMock.Setup(x => x.FindByNameAsync(loginDto.Username)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Username)).ReturnsAsync(user);


            _signInManagerMock.Setup(m => m.CheckPasswordSignInAsync(user, loginDto.Password, false)).ReturnsAsync(SignInResult.Success);
            _tokenServiceMock.Setup(m => m.GenerateTokenAsync(user.UserName, loginDto.Password)).ReturnsAsync("FakeToken");

            var setRefreshTokenResponse = new ReturnResponse<RefreshToken>
            {
                Status = true,
                StatusCode = StatusCodes.Status200OK,
                Message = It.IsAny<string>(),   
                Data = It.IsAny<RefreshToken>()
            };

            _tokenServiceMock.Setup(x => x.SetRefreshToken(user, It.IsAny<string>())).ReturnsAsync(setRefreshTokenResponse);

            var expectedResponse = new ReturnResponse<object>
            {
                Status = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Login successful.",
                Data = It.IsAny<object>()

            };

            _responseGeneratorServiceMock
                             .Setup(x => x.GenerateResponseAsync(true, StatusCodes.Status200OK, "Login successful.",It.IsAny<object>()))
                             .ReturnsAsync(expectedResponse);

            //Act 

            var result = await _taskManagerService.LoginUserAsync(loginDto);

            //Assert
            Assert.That(result.Status, Is.EqualTo(expectedResponse.Status));
            Assert.That(result.StatusCode, Is.EqualTo(expectedResponse.StatusCode));

            // Verify that CheckPasswordSignInAsync is called with the correct arguments
            _signInManagerMock.Verify(
                m => m.CheckPasswordSignInAsync(user, loginDto.Password, false),
                Times.Once);

            // Verify that GenerateTokenAsync is called with the correct arguments
            _tokenServiceMock.Verify(
                m => m.GenerateTokenAsync(user.UserName, loginDto.Password),
                Times.Once);

            // Verify that SetRefreshToken is called with the correct arguments
            _tokenServiceMock.Verify(
                x => x.SetRefreshToken(user, It.IsAny<string>()),
                Times.Once);

            // Verify that GenerateResponseAsync is called with the correct arguments
            _responseGeneratorServiceMock.Verify(
                m => m.GenerateResponseAsync(true, StatusCodes.Status200OK, It.IsAny<string>(), It.IsAny<object>()),
                Times.Once);
        }
        [Test]
        public async Task LoginUserAsync_InValidUsername_ReturnsUnauthorized()
        {
            var loginDto = new LoginDto
            {
                Username = "InValidUsername",
                Password = "ValidPassword"
            };

            var user = new AppUser { UserName = "InValidUsername" };

            _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((AppUser)null);

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AppUser)null);


            var expectedResponse = new ReturnResponse
            {
                Status = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid username or email."

            };

            _responseGeneratorServiceMock.Setup(x => x.GenerateResponseAsync(
                        false, StatusCodes.Status401Unauthorized, "Invalid username or email.")).ReturnsAsync(expectedResponse);
            //Act
            var result = await _taskManagerService.LoginUserAsync(loginDto);

            //Assert
            Assert.That(result.Status, Is.EqualTo(expectedResponse.Status));
            Assert.That(result.StatusCode, Is.EqualTo(expectedResponse.StatusCode));


            // Verify that GenerateTokenAsync is called with the correct arguments
            _tokenServiceMock.Verify(
                m => m.GenerateTokenAsync(user.UserName, loginDto.Password),
                Times.Never);

            // Verify that SetRefreshToken is called with the correct arguments
            _tokenServiceMock.Verify(
                x => x.SetRefreshToken(user, It.IsAny<string>()),
                Times.Never);

            // Verify that GenerateResponseAsync is called with the correct arguments
            _responseGeneratorServiceMock.Verify(
                m => m.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Invalid username or email."),
                Times.Once);
        }
        [Test]
        public async Task LoginUserAsync_InValidPassword_ReturnsUnauthorized()
        {
            var loginDto = new LoginDto
            {
                Username = "ValidUsername",
                Password = "InValidPassword"
            };

            var user = new AppUser { UserName = "ValidUsername" };

            _userManagerMock.Setup(x => x.FindByNameAsync(loginDto.Username)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Username)).ReturnsAsync(user);

            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, false)).ReturnsAsync(SignInResult.Failed);

            var expectedResponse = new ReturnResponse
            {
                Status = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid password."

            };

            _responseGeneratorServiceMock.Setup(x => x.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Invalid password."))
                .ReturnsAsync(expectedResponse);

            var result = await _taskManagerService.LoginUserAsync(loginDto);

            Assert.That(result, Is.EqualTo(expectedResponse));

        }

        [Test]
        public async Task RegisterUserAsync_PasswordMismatch_ReturnsBadRequest()
        {
            var registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "JohnDoe2",
                Password = "Password",
                ConfirmPassword = "MissMatchedPassword",
                Email = "john.doe@example.com",
                PhoneNumber = "1234567890"

            };

            var expectedRsponse = new ReturnResponse
            {
                Status = false,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Password and confirm password do not match."
            };
            _responseGeneratorServiceMock.Setup(x => x.GenerateResponseAsync(false, StatusCodes.Status400BadRequest, "Password and confirm password do not match."))
                .ReturnsAsync(expectedRsponse);


            var result = await _taskManagerService.RegisterUserAsync(registerDto);

            Assert.That(result,Is.EqualTo(expectedRsponse));

            _responseGeneratorServiceMock.Verify(x => x.GenerateResponseAsync(false, StatusCodes.Status400BadRequest, "Password and confirm password do not match.")
            , Times.Once);

            _responseGeneratorServiceMock.Verify(x => x.GenerateResponseAsync(
                        true, StatusCodes.Status200OK, $"Email sent successfully , Please check your Email to verify ") , Times.Never);

        }

        [Test]
        public async Task RegisterUserAsync_ExistingUser_ReturnsBadRequest()
        {
            var registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "JohnDoe2",
                Password = "Password",
                ConfirmPassword = "Password",
                Email = "john.doe@example.com",

                PhoneNumber = "1234567890"
            };

            var user = new AppUser { FirstName = "John", LastName = "Doe", UserName = "JohnDoe2", Email = "john.doe@example.com",PhoneNumber = "1234567890" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email)).ReturnsAsync(user);

            var expectedResponse = new ReturnResponse
            {
                Status = false,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = $"User {user.UserName} with the same email {user.Email} already exists."
            };

            _responseGeneratorServiceMock.Setup(x => x.GenerateResponseAsync(
                        false, StatusCodes.Status400BadRequest, $"User {user.UserName} with the same email {user.Email} already exists."))
                .ReturnsAsync(expectedResponse);

            var result = await _taskManagerService.RegisterUserAsync(registerDto);

            Assert.That(result, Is.EqualTo(expectedResponse));

            _responseGeneratorServiceMock.Verify(x => x.GenerateResponseAsync(
                        true, StatusCodes.Status200OK, $"Email sent successfully , Please check your Email to verify "),Times.Never);

        }

        [Test]
        public async Task RegisterUserAsync_FailedRegistration_ReturnsInternalServerError()
        {
            var registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "JohnDoe2",
                Password = "Password",
                ConfirmPassword = "Password",
                Email = "john.doe@example.com",
                PhoneNumber = "1234567890"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email)).ReturnsAsync((AppUser)null);
            var user = new AppUser { FirstName = "John", LastName = "Doe", UserName = "JohnDoe2", Email = "john.doe@example.com", PhoneNumber = "1234567890" };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                   .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User creation failed." }));

            var expectedResponse = new ReturnResponse
            {
                Status = false ,
                StatusCode = StatusCodes.Status500InternalServerError ,
                Message = It.IsAny<string>()
            };

             _responseGeneratorServiceMock.Setup(x => x.GenerateResponseAsync(false, StatusCodes.Status500InternalServerError, It.IsAny<string>()))
                      .ReturnsAsync(expectedResponse);

            var result = await _taskManagerService.RegisterUserAsync(registerDto);

            Assert.That(result, Is.EqualTo(expectedResponse));

            //Assert.That(result.Status, Is.EqualTo(expectedResponse.Status));
            //Assert.That(result.StatusCode, Is.EqualTo(expectedResponse.StatusCode));

            _responseGeneratorServiceMock.Verify(x => x.GenerateResponseAsync(
                        true, StatusCodes.Status200OK, $"Email sent successfully , Please check your Email to verify "), Times.Never);
        }
    }
}
