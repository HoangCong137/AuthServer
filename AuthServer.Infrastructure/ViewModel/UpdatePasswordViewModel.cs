using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AuthServer.Infrastructure.ViewModel
{
    public class UpdatePasswordViewModel
    {
        public string Email { get; set; }

        public string Mobile { get; set; }

        public string UserName { get; set; }

        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Password and confirmation password not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class UpdateNewPassWord
    {
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Password and confirmation password not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class UpdateForgetPasswordViewModel
    {
        public string Email { get; set; }
        public string OtpCode { get; set; }
        public string Password { get; set; }
    }
    public class UpdatePasswordModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Password and confirmation password not match.")]
        public string ConfirmPassword { get; set; }
    }
    public class FireBaseDeviceTokenModel
    {
        public string DeviceToken { get; set; }
        public string Role { get; set; }
    }
    public class UpdatePasswordRequest
    {
        public string UserName { get; set; }
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Password and confirmation password not match.")]
        public string ConfirmPassword { get; set; }
    }
}
