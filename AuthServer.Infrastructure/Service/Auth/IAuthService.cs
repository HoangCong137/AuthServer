using AuthServer.Domain.Entities;
using AuthServer.Infrastructure.Model.Auth;
using AuthServer.Infrastructure.ServiceModel;
using AuthServer.Infrastructure.ViewModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Infrastructure.Service.Auth
{
    public interface IAuthService
    {
        Task<ServiceResponse> CreateUserAsyncEmail(RegisterViewModelEmail User);
        Task<ServiceResponse> CreateUserAsyncMobile(RegisterViewModelMobile User);
        Task<ServiceResponse> AuthenticateEmailAsync(LoginViewEmailModel request);
        Task<ServiceResponse> AuthenticateMobileAsync(LoginViewMobileModel request);
        Task<ServiceResponse> UpdatePassword(UpdatePasswordViewModel model);
        Task<ServiceResponse> AuthenticateFacebookEmailAsync(LoginViewFacebookEmailModel request);
        Task<ServiceResponse> AuthenticateGoogleEmailAsync(LoginViewGoogleEmailModel request);
        Task<ServiceResponse> AuthenticateAppleEmailAsync(LoginViewAppleEmailModel request);
        Task<ServiceResponse> UpdateForgetPassword(UpdateForgetPasswordViewModel model);
        Task<ServiceResponse> UpdateAnh(AnhModel model, string userId);
        Task<ServiceResponse> CapNhatThongTinCaNhan(ThongTinCaNhanModel model, string userId);
        Task<ServiceResponse> GetThongTinCaNhan(string userId);
        Task<ServiceResponse> UpdatePassword(UpdatePasswordModel model, string userId);
        Task<ServiceResponse> RegisterForNhaTruong(RegisterForNhaTruongViewModelEmail model);
        Task<ServiceResponse> UpdateFireBaseDeviceToken(FireBaseDeviceTokenModel model, string userId);
        Task<ServiceResponse> GetAllRole();
        Task<ServiceResponse> checkmobile(string Mobile);
        Task<ServiceResponse> updateAvater(updateUserModel userModel, string UserName);
        Task<ServiceResponse> getUser(string UserName);
        Task<ServiceResponse> getAllUser(string search);
        Task<ServiceResponse> updateMultiAvatar(IFormFile avatar, string UserName);
        Task<ServiceResponse> Authenticate(LoginViewModel request);
        Task<ServiceResponse> CreateUserAsync(RegisterViewModel User);
        Task<ServiceResponse> updateRole(string UserName);
        Task<ServiceResponse> GetClaim(Guid IdRole);
        Task<ServiceResponse> infoUser(int PageIndex, int PageSize, string Search, Guid? idPhongBan, bool? IsActive);
        Task<ServiceResponse> UnActiveUser(Guid idUser);
        Task<ServiceResponse> deleteUser(Guid idUser);
        Task<ServiceResponse> addPermission(Permissions Model);
        Task<ServiceResponse> GetByIdUser(Guid IdUser);
        Task<ServiceResponse> UpdatePer(Permissions Model);
        Task<ServiceResponse> getAllUserbyId(Guid Id);

        //Task<ServiceResponse> CreateRegisterId(UserRegister User, string fullname, string phonenumber, string email, DateTime birthday);
        Task<ServiceResponse> CreateNewAccount(UserRegister User);
        Task<ServiceResponse> GetUserNew(Guid Id);
        Task<ServiceResponse> UpdatePasswordNew(UpdatePasswordRequest model);
    }
}
