using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace AuthServer.Domain.ViewModels
{
    public class LoginViewEmailModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string Role { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }

    public class LoginViewMobileModel
    {
        [Required]
        [Phone]
        public string Mobile { get; set; }
        [Required]
        public string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        public int? Expired { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        public int? Expired { get; set; }
    }

    public class LoginViewFacebookEmailModel
    {
        [Required]
        public string FacebookId { get; set; }
        [Required]
        public string Email { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }

    public class LoginViewGoogleEmailModel
    {
        [Required]
        public string GoogleId { get; set; }
        [Required]
        public string Email { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }

    public class LoginViewAppleEmailModel
    {
        [Required]
        public string AppleId { get; set; }
        [Required]
        public string Email { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
