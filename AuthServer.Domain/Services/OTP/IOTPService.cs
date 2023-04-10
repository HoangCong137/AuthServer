using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Domain.Services.OTP
{
    public interface IOTPService
    {
        Task<ServiceResponse> SendOtp(String[] phones, String content, int type, String sender);
        Task<ServiceResponse> SendOtp(string email, string maLanguage);
        Task<ServiceResponse> SendOtpAfterCheckEmail(string email, string maLanguage);
        Task<ServiceResponse> SendOtpAfterCheckNotEmail(string email, string maLanguage);
        Task<ServiceResponse> checkOTP(string Email, string OTP);
    }
}
