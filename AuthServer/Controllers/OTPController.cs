using AuthServer.Domain.Services;
using AuthServer.Domain.Services.OTP;
using AuthServer.Domain.ViewModels;
using AuthServer.Infrastructure.Service.OTP;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.Controllers
{
    [Route("api/otp")]
    [ApiController]
    public class OTPController : Controller
    {
        private readonly IOTPService _otpService;

        public OTPController(IOTPService AuthService)
        {
            _otpService = AuthService;
        }

        [HttpPost("send-otp")]
        public async Task<ServiceResponse> post([FromBody] OtpCodeViewModel model) => await _otpService.SendOtp(model.Email,"vi");
        [HttpPost("send-otp-after-check-email-existed")]
        public async Task<ServiceResponse> postAfterCheckEmail([FromBody] OtpCodeViewModel model) => await _otpService.SendOtpAfterCheckEmail(model.Email, "vi");
        [HttpPost("send-otp-after-check-email-not-existed")]
        public async Task<ServiceResponse> postAfterCheckNotEmail([FromBody] OtpCodeViewModel model) => await _otpService.SendOtpAfterCheckNotEmail(model.Email, "vi");

        [HttpGet("checkotp")]
        public async Task<ServiceResponse> checkOTP(string Email, string OTP) => await _otpService.checkOTP(Email, OTP);
    }
}
