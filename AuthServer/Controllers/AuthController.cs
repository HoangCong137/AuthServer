using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using AuthServer.Domain.Entities;
using System;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders.Physical;
using AuthServer.Infrastructure.Service.Auth;
using AuthServer.Domain.Models.Auth;
using AuthServer.Domain.ViewModels;
using AuthServer.Domain.Services;
using AuthServer.Domain.Services.Auth;

namespace AuthServer.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IAuthService _authService;

        public AuthController(ILogger<HomeController> logger, UserManager<User> userManager, SignInManager<User> signInManager
                              , IAuthService AuthService)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _authService = AuthService;
        }

        [HttpPost("registerEmail")]
        public virtual async Task<ServiceResponse> Register(RegisterViewModelEmail model) => await _authService.CreateUserAsyncEmail(model);
        /// <summary>
        /// Dự án DDA
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("registerMobile")]
        public virtual async Task<ServiceResponse> RegisterMobile(RegisterViewModelMobile model) => await _authService.CreateUserAsyncMobile(model);

        [HttpPost("register")]
        public virtual async Task<ServiceResponse> register([FromQuery] RegisterViewModel model) => await _authService.CreateUserAsync(model);

        [HttpPost("registerNhaTruongEmail")]
        [Authorize]
        public virtual async Task<ServiceResponse> RegisterForNhaTruong(RegisterForNhaTruongViewModelEmail model) => await _authService.RegisterForNhaTruong(model);

        [HttpPost("loginEmail")]
        public async Task<ServiceResponse> Login(LoginViewEmailModel model) => await _authService.AuthenticateEmailAsync(model);
        /// <summary>
        /// Dự án DDA
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("loginMobile")]
        public async Task<ServiceResponse> LoginMobile(LoginViewMobileModel model) => await _authService.AuthenticateMobileAsync(new LoginViewMobileModel
        {
            Password = model.Password,
            Mobile = model.Mobile,
            Expired = model.Expired
        });

        [HttpPost("Login")]
        public async Task<ServiceResponse> Login(LoginViewModel model) => await _authService.Authenticate(new LoginViewModel
        {
            Password = model.Password,
            UserName = model.UserName,
            Expired = model.Expired
        });

        [HttpPost("updatePassword")]
        public virtual async Task<ServiceResponse> UpdateForgetPassword(UpdatePasswordViewModel model) => await _authService.UpdatePassword(model);

        [HttpPost("updatePasswordotp")]
        public virtual async Task<ServiceResponse> updatePasswordotp(UpdateForgetPasswordViewModel model) => await _authService.UpdateForgetPassword(model);

        [HttpPost("loginFacebook")]
        public async Task<ServiceResponse> LoginByFaceBook(LoginViewFacebookEmailModel model) => await _authService.AuthenticateFacebookEmailAsync(model);

        [HttpPost("loginGoogle")]
        public async Task<ServiceResponse> LoginByGoogle(LoginViewGoogleEmailModel model) => await _authService.AuthenticateGoogleEmailAsync(model);

        [HttpPost("loginApple")]
        public async Task<ServiceResponse> LoginByApple(LoginViewAppleEmailModel model) => await _authService.AuthenticateAppleEmailAsync(model);

        [HttpPost("updateAnh")]
        [Authorize]
        public async Task<ServiceResponse> UpdateAnh(AnhModel model) => await _authService.UpdateAnh(model, User.Claims.FirstOrDefault(c => c.Type.Equals("UserId")).Value);

        [HttpPost("updateThongTinCaNhan")]
        [Authorize]
        public async Task<ServiceResponse> CapNhatThongTinCaNhan(ThongTinCaNhanModel model) => await _authService.CapNhatThongTinCaNhan(model, User.Claims.FirstOrDefault(c => c.Type.Equals("UserId")).Value);

        [HttpPost("ThongTinCaNhan")]
        [Authorize]
        public async Task<ServiceResponse> GetThongTinCaNhan()
        {
            return await _authService.GetThongTinCaNhan(User.Claims.FirstOrDefault(c => c.Type.Equals("UserId")).Value);
        }
        [HttpPost("updateUserPassword")]
        [Authorize]
        public async Task<ServiceResponse> UpdatePassword(UpdatePasswordModel model)
        {
            return await _authService.UpdatePassword(model, User.Identity.Name);
        }

        [HttpPost("updateFireBaseDeviceToken")]
        [Authorize]
        public async Task<ServiceResponse> UpdateFireBaseDeviceToke(FireBaseDeviceTokenModel model)
        {
            return await _authService.UpdateFireBaseDeviceToken(model, User.Claims.FirstOrDefault(c => c.Type.Equals("UserId")).Value);
        }

        [HttpGet("allRoles")]
        public async Task<ServiceResponse> GetAllRole() => await _authService.GetAllRole();

        [HttpGet("checkmobile")]
        public async Task<ServiceResponse> checkmobile(string Mobile) => await _authService.checkmobile(Mobile);

        [HttpPost("UpdateUser")]
        [Authorize]
        public async Task<ServiceResponse> UpdateUser(updateUserModel userModel)
        {

            if (!string.IsNullOrEmpty(userModel.UserName))
            {
                return await _authService.updateAvater(userModel, userModel.UserName);
            }
            return await _authService.updateAvater(userModel, User.Identity.Name);
        }

        [HttpGet("getUser")]
        [Authorize]
        public async Task<ServiceResponse> getUser() => await _authService.getUser(User.Identity.Name);

        [HttpGet("getUserbyId")]
        public async Task<ServiceResponse> getUserbyId(Guid Id) => await _authService.getAllUserbyId(Id);

        [HttpGet("getalluser")]
        public async Task<ServiceResponse> getalluser(string search) => await _authService.getAllUser(search);

        [HttpPost("updateavatar")]
        [Authorize]
        public async Task<ServiceResponse> updateavatar(IFormFile avatar) => await _authService.updateMultiAvatar(avatar, User.Identity.Name);

        [HttpPost("updateavatarbyUserName")]
        [Authorize]
        public async Task<ServiceResponse> updateavatarbyUserName(IFormFile avatar, string UserName) => await _authService.updateMultiAvatar(avatar, UserName);

        [HttpPost("updateRole")]
        [Authorize]
        public async Task<ServiceResponse> updateRole(string UserName) => await _authService.updateRole(User.Identity.Name);

        [HttpGet("getclaimbyidrole")]
        public async Task<ServiceResponse> getLstClaim(Guid IdRole) => await _authService.GetClaim(IdRole);

        [HttpGet("getLstUser")]
        public async Task<ServiceResponse> getLstUser(int PageIndex, int PageSize, string Search, Guid? idPhongBan, bool? IsActive) => await _authService.infoUser(PageIndex, PageSize, Search, idPhongBan, IsActive);

        [HttpDelete("deleteuser")]
        public async Task<ServiceResponse> deleteUser(Guid UserId) => await _authService.deleteUser(UserId);

        [HttpPut("UnactiveUser")]
        public async Task<ServiceResponse> UnactiveUser(Guid UserId) => await _authService.UnActiveUser(UserId);

        [HttpPost("AddPermission")]
        public async Task<ServiceResponse> AddPermission(Permissions Model) => await _authService.addPermission(Model);

        [HttpGet("GetPermission")]
        public async Task<ServiceResponse> AddPermission(Guid IdUser) => await _authService.GetByIdUser(IdUser);

        [HttpPost("UpdatePermission")]
        public async Task<ServiceResponse> UpdatePermission([FromBody] Permissions Model) => await _authService.UpdatePer(Model);

        [HttpPost("RegisterNewId")]       
        public virtual async Task<ServiceResponse> RegisterId(UserRegister User)
        {
            return await _authService.CreateNewAccount(User);
        }
        [HttpGet("GetNewUserId")]
        public async Task<ServiceResponse> GetUserId(Guid Id)
        {
            return await _authService.GetUserNew(Id);
        }
        [HttpPut,Route("UpdateNewPassword")]
        public async Task<ServiceResponse> UpdateNewPassword(UpdatePasswordRequest model)
        {
            return await _authService.UpdatePasswordNew(model);
        }
    } 
}
