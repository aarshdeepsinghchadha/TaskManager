using Application.Interface;
using Application.TaskManager;
using Domain;
using Infrastructure.Services;
using log4net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Persistance;




namespace TaskManagerWebAPITests
{
    public class TaskManagerServiceTests
    {
        private ITaskManagerService  _taskManagerService;
        private  Mock<UserManager<AppUser>> _userManagerMock;
        private  Mock<SignInManager<AppUser>> _signInManagerMock;
        private  Mock<IResponseGeneratorService> _responseGeneratorServiceMock;
        private  Mock<ITokenService> _tokenServiceMock;
        private  Mock<IEmailSenderService> _emailSenderServiceMock;
        private  Mock<IConfiguration> _configurationMock;
        private  Mock<ILog> _logMock;
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
   /* IPasswordHasher<TUser> passwordHasher */null,
   /* IEnumerable<IUserValidator<TUser>> userValidators */null,
   /* IEnumerable<IPasswordValidator<TUser>> passwordValidators */null,
   /* ILookupNormalizer keyNormalizer */null,
   /* IdentityErrorDescriber errors */null,
   /* IServiceProvider services */null,
   /* ILogger<UserManager<TUser>> logger */null);

                _signInManagerMock = new Mock<SignInManager<AppUser>>(
                _userManagerMock.Object,
                /* IHttpContextAccessor contextAccessor */Mock.Of<IHttpContextAccessor>(),
                /* IUserClaimsPrincipalFactory<TUser> claimsFactory */Mock.Of<IUserClaimsPrincipalFactory<AppUser>>(),
                /* IOptions<IdentityOptions> optionsAccessor */null,
                /* ILogger<SignInManager<TUser>> logger */null,
                /* IAuthenticationSchemeProvider schemes */null,
                /* IUserConfirmation<TUser> confirmation */null);

            _dataContext = new Mock<DataContext>();
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

            _signInManagerMock.Setup(m => m.CheckPasswordSignInAsync(user, loginDto.Password, false)).ReturnsAsync(SignInResult.Success);
            _tokenServiceMock.Setup(m => m.GenerateTokenAsync(user.UserName, loginDto.Password)).ReturnsAsync("FakeToken");



            //Act 

            var result = await _taskManagerService.LoginUserAsync(loginDto);

            //Assert

            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);

            _responseGeneratorServiceMock.Verify(
                m => m.GenerateResponseAsync(true, StatusCodes.Status200OK, It.IsAny<string>(), It.IsAny<object>()),
                Times.Once);
        }


    }

}
