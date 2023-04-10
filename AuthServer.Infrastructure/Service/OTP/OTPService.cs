using AuthServer.Infrastructure.Common.Service;
using AuthServer.Infrastructure.Data.Identity;
using AuthServer.Domain.Entities;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Net;
using System.Threading.Tasks;
using Dapper;
using AuthServer.Infrastructure.Repository.Users;
using AuthServer.Infrastructure;
using AuthServer.Domain.Interfaces;
using AuthServer.Domain.Services.OTP;
using AuthServer.Domain.Services;

namespace AuthServer.Infrastructure.Service.OTP
{
    public class OTPService : SResponse, IOTPService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        const string rootURL = "https://api.speedsms.vn/index.php";
        public OTPService(IConfiguration configuration, IUserRepository userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
        }
        public async Task<ServiceResponse> SendOtp(string[] phones, string content, int type, string sender)
        {

            string url = rootURL + "/sms/send";

            if (phones.Length <= 0)
                return null;
            if (content.Equals(""))
                return null;

            if (type == 3 && sender.Equals(""))
                return null;

            NetworkCredential myCreds = new NetworkCredential("rVIwokGsDO1TKOtNEEchEt18g79r6ZOe", ":x");
            WebClient client = new WebClient();
            client.Credentials = myCreds;
            client.Headers[HttpRequestHeader.ContentType] = "application/json";

            string builder = "{\"to\":[";

            for (int i = 0; i < phones.Length; i++)
            {
                builder += "\"" + phones[i] + "\"";
                if (i < phones.Length - 1)
                {
                    builder += ",";
                }
            }
            builder += "], \"content\": \"" + Uri.EscapeDataString(content) + "\", \"type\":" + type + ", \"sender\": \"" + sender + "\"}";

            string json = builder.ToString();

            return Ok(client.UploadString(url, json));
        }

        public async Task<ServiceResponse> SendOtp(string email, string maLanguage)
        {
            var connectionString = GetConnection();
            using var con = new SqlConnection(connectionString);
            try
            {
                con.Open();
                var OtpCode = (DateTime.Now.Ticks % 1000000).ToString();
                MimeMessage message = new MimeMessage();

                MailboxAddress from = new MailboxAddress("Hòm thư Tuần Châu",
                "cftnotify@gmail.com");
                message.From.Add(from);

                MailboxAddress to = new MailboxAddress("Quý khách",
                email);
                message.To.Add(to);

                message.Subject = "Hệ thống Tuần Châu gửi mã xác thực";
                BodyBuilder bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $"<p>Mã xác thực của bạn là {OtpCode}</p>";
                message.Body = bodyBuilder.ToMessageBody();
                SmtpClient client = new SmtpClient();
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate("cftnotify@gmail.com", "yatdugzgdyxxkcfm");
                client.Send(message);
                client.Disconnect(true);
                client.Dispose();
                var query = $"INSERT INTO OtpCodeLog(Id,Code,Email,ThoiGianKhoiTao) VALUES(NEWID(), '{OtpCode}', '{email}', GETDATE())";

                var result = await con.ExecuteAsync(query);
                return Ok("Gửi mã xác thực thành công");

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                con.Close();
            }

        }

        public string GetConnection()
        {
            var connection = _configuration.GetSection("ConnectionStrings").GetSection("default").Value;
            return connection;
        }

        public async Task<ServiceResponse> SendOtpAfterCheckEmail(string email, string maLanguage)
        {
            var connectionString = GetConnection();
            using var con = new SqlConnection(connectionString);
            try
            {
                con.Open();
                var isEmailExisted = await con.ExecuteScalarAsync<bool>($"SELECT 1 FROM Common.dbo.Users WHERE Email = '{email}'");
                if (isEmailExisted)
                {
                    var OtpCode = (DateTime.Now.Ticks % 1000000).ToString();
                    MimeMessage message = new MimeMessage();

                    MailboxAddress from = new MailboxAddress("Hòm thư Tuần Châu",
                    "cftnotify@gmail.com");
                    message.From.Add(from);

                    MailboxAddress to = new MailboxAddress("Quý khách",
                    email);
                    message.To.Add(to);

                    message.Subject = "Hệ thống Tuần Châu gửi mã xác thực";
                    BodyBuilder bodyBuilder = new BodyBuilder();
                    bodyBuilder.HtmlBody = $"<p>Mã xác thực của bạn là {OtpCode}</p>";
                    message.Body = bodyBuilder.ToMessageBody();
                    SmtpClient client = new SmtpClient();
                    client.Connect("smtp.gmail.com", 465, true);
                    client.Authenticate("cftnotify@gmail.com", "yatdugzgdyxxkcfm");
                    client.Send(message);
                    client.Disconnect(true);
                    client.Dispose();
                    var query = $"INSERT INTO OtpCodeLog(Id,Code,Email,ThoiGianKhoiTao) VALUES(NEWID(), '{OtpCode}', '{email}', GETDATE())";

                    var result = await con.ExecuteAsync(query);
                    return Ok("Gửi mã xác thực thành công");

                }
                return Unauthorized("401", "Email không tồn tại");

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                con.Close();
            }
        }

        public async Task<ServiceResponse> SendOtpAfterCheckNotEmail(string email, string maLanguage)
        {
            var connectionString = GetConnection();
            using var con = new SqlConnection(connectionString);
            try
            {
                con.Open();
                var isEmailExisted = await con.ExecuteScalarAsync<bool>($"SELECT 1 FROM Common.dbo.Users WHERE Email = '{email}'");
                if (!isEmailExisted)
                {
                    var OtpCode = (DateTime.Now.Ticks % 1000000).ToString();
                    MimeMessage message = new MimeMessage();

                    MailboxAddress from = new MailboxAddress("Hòm thư winwin",
                    "cftnotify@gmail.com");
                    message.From.Add(from);

                    MailboxAddress to = new MailboxAddress("Quý khách",
                    email);
                    message.To.Add(to);

                    message.Subject = "Hệ thống winwin cho cộng đồng người nước ngoài gửi mã xác thực";
                    BodyBuilder bodyBuilder = new BodyBuilder();
                    bodyBuilder.HtmlBody = $"<p>Mã xác thực của bạn là {OtpCode}</p>";
                    message.Body = bodyBuilder.ToMessageBody();
                    SmtpClient client = new SmtpClient();
                    client.Connect("smtp.gmail.com", 465, true);
                    client.Authenticate("cftnotify@gmail.com", "yatdugzgdyxxkcfm");
                    client.Send(message);
                    client.Disconnect(true);
                    client.Dispose();
                    var query = $"INSERT INTO OtpCodeLog(Id,Code,Email,ThoiGianKhoiTao) VALUES(NEWID(), '{OtpCode}', '{email}', GETDATE())";

                    var result = await con.ExecuteAsync(query);
                    return Ok("Gửi mã xác thực thành công");

                }
                return Unauthorized("401", "Tài khoản đã tồn tại");

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                con.Close();
            }
        }

        public async Task<ServiceResponse> checkOTP(string Email, string OTP)
        {
            var checkotp = await _userRepository.GetOTP(Email, OTP);

            if (checkotp == null)
            {
                return NotFound("404", "OTP không tồn tại");
            }

            if (DateTime.Now - checkotp.ThoiGianKhoiTao.Value > new TimeSpan(0, 0, 90))
            {
                return NotFound("404", "Hết thời hạn dùng otp");
            }

            return Ok(true);
        }
    }
}
