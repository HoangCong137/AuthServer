
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AuthServer.Infrastructure.Common.Service;
using AuthServer.Domain.Entities;
using AuthServer.Domain.Shared;
using AuthServer.Infrastructure.Model;
using AuthServer.Infrastructure.Model.Auth;
using AuthServer.Infrastructure.Model.Config;
using AuthServer.Infrastructure.Repository;
using AuthServer.Infrastructure.ServiceModel;
using AuthServer.Infrastructure.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Dapper;
using Microsoft.AspNetCore.Http;
using AuthServer.Infrastructure.Repository.Users;
using Microsoft.Data.SqlClient;
using AuthServer.Infrastructure.Service.Files;
using AuthServer.Infrastructure.Repository;

namespace AuthServer.Infrastructure.Service.Auth
{
    public class AuthService : SResponse, IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtOptions _options;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly IAsyncRepository<User> _repository;
        private readonly IAsyncRepository<IdentityUserRole<Guid>> _repositoryRole;
        private readonly IAsyncRepository<Domain.Entities.Role> _repositoryRoleName;
        private readonly IAsyncRepository<IdentityRoleClaim<Guid>> _repositoryRoleClaim;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;
        private readonly IUserRepository _userRepository;

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, IOptions<JwtOptions> options
                           , IAsyncRepository<User> repository, IConfiguration configuration, IFileService fileService,
                            IAsyncRepository<IdentityUserRole<Guid>> repositoryRole, IAsyncRepository<Domain.Entities.Role> repositoryRoleName,
                            IAsyncRepository<IdentityRoleClaim<Guid>> repositoryRoleClaim, IUserRepository userRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _options = options.Value;
            _jwtSecurityTokenHandler ??= new JwtSecurityTokenHandler();
            _repository = repository;
            _configuration = configuration;
            _fileService = fileService;
            _repositoryRole = repositoryRole;
            _repositoryRoleName = repositoryRoleName;
            _repositoryRoleClaim = repositoryRoleClaim;
            _userRepository = userRepository;
        }       

        public async Task<ServiceResponse> CreateUserAsyncEmail(RegisterViewModelEmail User)
        {

            var connectionString = this.GetConnection();

            using var con = new SqlConnection(connectionString);

            try
            {
                con.Open();
                var userRegister = new User()
                {
                    UserName = User.Email,
                    Email = User.Email,
                    Source = Settings.SOURCE
                };

                if (!string.IsNullOrEmpty(User.FullName))
                {
                    userRegister.FullName = User.FullName;
                }

                var userResult = await _userManager.FindByEmailAsync(User.Email);

                if (userResult != null)
                {
                    return BadRequest("", "Đã tồn tại tài khoản");
                }

                userRegister.Id = Guid.NewGuid();
                var xacThucId = await con.ExecuteScalarAsync<Guid>($"SELECT TOP 1 Id FROM OtpCodeLog WHERE Email = '{User.Email}' AND Code = '{User.OtpCode}' AND ThoiGianXacThuc IS NULL");
                if (!Guid.Empty.Equals(xacThucId))
                {
                    if (User.TypeAccount == typeAccount.Tech)
                    {
                        userRegister.IdTechnician = await _userRepository.addTechnician(User.Email);
                    }

                    if (User.TypeAccount == typeAccount.Salon)
                    {
                        userRegister.IdSalon = await _userRepository.addSalon(User.Email);
                    }

                    await _userManager.CreateAsync(userRegister, User.Password);
                    await _userManager.AddToRoleAsync(userRegister, Settings.ENDUSERROLE);
                    var InfoUser = _repository.ListAll().Where(x => x.Email == User.Email).FirstOrDefault();
                    await con.ExecuteAsync($"UPDATE OtpCodeLog SET ThoiGianXacThuc = GETDATE() WHERE Email = '{User.Email}' AND Code = '{User.OtpCode}'");
                    var result = new AuthenticateResponse
                    {
                        AccessToken = GenerateAccessToken(InfoUser),
                        RefreshToken = generateRefreshToken(InfoUser.UserName).Token,
                        //UserInformation = InfoUser,
                    };
                    return Ok(result);
                }
                return Unauthorized(StatusCodes.Status401Unauthorized.ToString(), "Mã xác thực không đúng,vui lòng thử lại");
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

        public async Task<ServiceResponse> checkmobile(string Mobile)
        {
            var checkMobile = await _repository.FistOrDefaultAsync(x => x.PhoneNumber == Mobile);

            if (checkMobile == null)
            {
                return Forbidden("", "Số điện thoại không tồn tại trong hệ thống");
            }
            else
            {
                Ok(true);
            }
            return Ok();
        }

        public async Task<ServiceResponse> CreateUserAsyncMobile(RegisterViewModelMobile User)
        {
            var checkMobile = await _repository.FistOrDefaultAsync(x => x.PhoneNumber == User.Mobile);

            if (checkMobile != null)
            {
                return Forbidden("", "Số điện thoại đã tồn tại trong hệ thống");
            }

            var userRegister = new User()
            {
                PhoneNumber = User.Mobile,
                UserName = User.Mobile,
                FullName = User.FullName,
                Email = User.Email,
                IsActive = true
            };

            //var roles = new List<string>();
            //roles.Add("EndUser");
            userRegister.Id = Guid.NewGuid();
            var resultCreated = await _userManager.CreateAsync(userRegister, User.Password);

            var InfoUser = _repository.ListAll().Where(x => x.PhoneNumber == User.Mobile).FirstOrDefault();

            if (resultCreated.Succeeded)
            {
                var result = new AuthenticateResponse
                {
                    AccessToken = GenerateAccessToken(InfoUser),
                    RefreshToken = generateRefreshToken(InfoUser.UserName).Token,
                    //UserInformation = InfoUser,
                };
                return Ok(result);
            }

            return Ok(resultCreated);
        }


        public async Task<ServiceResponse> CreateUserAsync(RegisterViewModel User)
        {
            var checkMobile = await _repository.FistOrDefaultAsync(x => x.UserName == User.UserName);

            if (checkMobile != null)
            {
                return Forbidden("", "Tài khoản đã tồn tại");
            }

            var userRegister = new User()
            {
                UserName = User.UserName,
                FullName = User.FullName,
                Email = User.Email,
                IdNhanVien = User.IdNhanVien,
                IdSalon = User.IdSalon,
                IdTechnician = User.IdTechnician,
                IsActive = true,
                IsDeleted = false,
                PhoneNumber = User.PhoneNumber
            };

            //var roles = new List<string>();
            //roles.Add("EndUser");
            userRegister.Id = Guid.NewGuid();
            var resultCreated = await _userManager.CreateAsync(userRegister, User.Password);

            var InfoUser = await _userManager.FindByNameAsync(userRegister.UserName);

            var roles = await _userRepository.GetPermission(InfoUser.Id);

            var UserInfo = new UserInfo()
            {
                Id = InfoUser.Id,
                UserName = InfoUser.UserName,
                FullName = InfoUser.FullName,
                Birthday = InfoUser.Birthday,
                Email = InfoUser.Email,
                PhoneNumber = User.PhoneNumber,
                Permission = roles
            };


            if (resultCreated.Succeeded)
            {
                var result = new AuthenticateResponse
                {
                    AccessToken = GenerateAccessTokenTC(UserInfo),
                    RefreshToken = generateRefreshToken(InfoUser.UserName).Token,
                    //UserInformation = InfoUser,
                };
                return Ok(result);
            }

            return Ok(resultCreated);
        }

        public async Task<ServiceResponse> AuthenticateEmailAsync(LoginViewEmailModel request)
        {
            var user = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);
            if (user.Succeeded)
            {
                var InfoUser = await _repository.FistOrDefaultAsync(x => x.Email == request.Email);
                var IsPhanQuyen = await _userManager.IsInRoleAsync(InfoUser, string.IsNullOrEmpty(request.Role) ? Settings.ENDUSERROLE : request.Role);
                if (!IsPhanQuyen)
                {
                    return Unauthorized(StatusCodes.Status401Unauthorized.ToString(), "Bạn chưa được phân quyền để sử dụng chức năng này");
                }
                var result = new AuthenticateResponse
                {
                    AccessToken = GenerateAccessToken(InfoUser),
                    RefreshToken = generateRefreshToken(InfoUser.UserName).Token,
                    TruongHocId = InfoUser.TruongHocId.GetValueOrDefault()                  
                    //UserInformation = InfoUser,
                };
                return Ok(result);
            }
            return Unauthorized(StatusCodes.Status401Unauthorized.ToString(), "Tài khoản hoặc mật khẩu không đúng");
        }

        public async Task<ServiceResponse> AuthenticateMobileAsync(LoginViewMobileModel request)
        {
            var user = await _signInManager.PasswordSignInAsync(request.Mobile, request.Password, false, false);

            _options.ExpiresInMinutes = request.Expired > 0 ? request.Expired.Value * 60 : _options.ExpiresInMinutes;

            if (user.Succeeded)
            {
                var InfoUser = await _repository.FistOrDefaultAsync(x => x.PhoneNumber == request.Mobile || x.UserName == request.Mobile);
                var result = new AuthenticateResponse
                {
                    AccessToken = GenerateAccessToken(InfoUser),
                    RefreshToken = generateRefreshToken(InfoUser.UserName).Token,
                    //UserInformation = InfoUser,
                };
                return Ok(result);
            }
            return Unauthorized();
        }


        public async Task<ServiceResponse> Authenticate(LoginViewModel request)
        {
            var checkuser = await _userManager.FindByNameAsync(request.UserName);

            if (!checkuser.IsActive)
            {
                return Unauthorized("","Tài khoản đã bị khoá");
            }

            var u = await _signInManager.PasswordSignInAsync(request.UserName, request.Password, false, false);

            _options.ExpiresInMinutes = request.Expired > 0 ? request.Expired.Value * 60 : _options.ExpiresInMinutes;

            if (u.Succeeded)
            {
                var User = await _userManager.FindByNameAsync(request.UserName);

                var roles = await _userRepository.GetPermission(User.Id);

                var UserInfo = new UserInfo()
                {
                    Id = User.Id,
                    IdNhanVien = User.IdNhanVien,
                    Avatar = User.Avatar,
                    UserName = User.UserName,
                    FullName = User.FullName,
                    Birthday = User.Birthday,
                    Email = User.Email,
                    IdSalon = User.IdSalon,
                    IdTechnician = User.IdTechnician,
                    Permission = roles
                };
                var roleList = await _userRepository.getAuthorityGroup(User.Id);

                    var result = new AuthenticateResponse
                    {
                        AccessToken = GenerateAccessTokenTC(UserInfo),
                        RefreshToken = generateRefreshToken(User.UserName).Token,
                        UserInformation = UserInfo,
                        Role = roleList,
                    };

                User.LoginTime = DateTime.Now;

                await _userManager.UpdateAsync(User);

                return Ok(result);
            }
            return Unauthorized();
        }

        public async Task<ServiceResponse> UpdatePassword(UpdatePasswordViewModel model)
        {
            var user = _repository.FistOrDefault(x => x.PhoneNumber == model.Mobile && model.Mobile != null || x.Email == model.Email && model.Email != null || x.UserName == model.UserName && model.UserName != null);

            if (user == null)
            {
                return BadRequest("", "Tài khoản không tồn tại");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            return Ok(result);
        }

        private string GenerateAccessTokenTC(UserInfo infoUser)
        {
            var claims = new List<Claim>() {
                new Claim(ClaimTypes.NameIdentifier, infoUser.FullName != null ? infoUser.FullName.ToString() : ""),
                new Claim(Claims.Permissions, infoUser.Permission != null ? infoUser.Permission.Permission : "" ),
                new Claim(ClaimTypes.Name, infoUser.UserName),
                new Claim(Claims.UserId, infoUser.Id.ToString()),
                new Claim(Claims.IdNhanVien, infoUser.IdNhanVien != null ? infoUser.IdNhanVien.Value.ToString() : ""),
                new Claim(Claims.RoomLock, infoUser.Permission != null ? infoUser.Permission.RoomLock != null ? infoUser.Permission.RoomLock.Value.ToString() : "" : ""),
                //new Claim(Claims.CanBoViewModel, Newtonsoft.Json.JsonConvert.SerializeObject(canBoInfo))
            };


            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret)), SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_options.ExpiresInMinutes),
                SigningCredentials = signingCredentials,
                Issuer = _options.issuer,
                Audience = _options.issuer,
            };

            return _jwtSecurityTokenHandler.WriteToken(_jwtSecurityTokenHandler.CreateToken(tokenDescriptor));

        }

        private string GenerateAccessToken(User infoUser)
        {
            var claims = new List<Claim>() {
                new Claim(Claims.UserId, infoUser.Id.ToString(), ClaimValueTypes.String),
                new Claim(ClaimTypes.NameIdentifier, infoUser.FullName != null ? infoUser.FullName.ToString() : null),
                new Claim(ClaimTypes.Name, infoUser.UserName),
                //new Claim(Claims.Permissions, Newtonsoft.Json.JsonConvert.SerializeObject(permission)),
                //new Claim(Claims.CanBoViewModel, Newtonsoft.Json.JsonConvert.SerializeObject(canBoInfo))
            };


            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret)), SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_options.ExpiresInMinutes),
                SigningCredentials = signingCredentials,
                Issuer = _options.issuer,
                Audience = _options.issuer,
            };

            return _jwtSecurityTokenHandler.WriteToken(_jwtSecurityTokenHandler.CreateToken(tokenDescriptor));

        }

        private RefreshToken generateRefreshToken(string ipAddress)
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };
            }
        }

        public async Task<ServiceResponse> AuthenticateFacebookEmailAsync(LoginViewFacebookEmailModel request)
        {
            User user;
            if (string.IsNullOrEmpty(request.FacebookId))
            {
                return Unauthorized(StatusCodes.Status401Unauthorized.ToString(), "facebook userid invalid");
            }
            user = await _repository.FistOrDefaultAsync(q => q.FacebookId.Equals(request.FacebookId));
            if (user != null)
            {
                return Ok(new AuthenticateResponse
                {
                    AccessToken = GenerateAccessToken(user),
                    RefreshToken = generateRefreshToken(user.UserName).Token,
                    //UserInformation = InfoUser,
                });
            }
            if (string.IsNullOrEmpty(request.Email))
            {
                return Unauthorized(StatusCodes.Status401Unauthorized.ToString(), "vui lòng bổ sung thêm email");
            }
            user = new User()
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                FullName = request.Email,
                Source = Settings.SOURCE,
                FacebookId = request.FacebookId
            };
            await _userManager.CreateAsync(user);
            await _userManager.AddToRoleAsync(user, Settings.ENDUSERROLE);
            var result = new AuthenticateResponse
            {
                AccessToken = GenerateAccessToken(user),
                RefreshToken = generateRefreshToken(user.UserName).Token,
                //UserInformation = InfoUser,
            };
            return Ok(result);
        }

        public async Task<ServiceResponse> AuthenticateGoogleEmailAsync(LoginViewGoogleEmailModel request)
        {
            User user;
            if (string.IsNullOrEmpty(request.GoogleId))
            {
                return Unauthorized(StatusCodes.Status401Unauthorized.ToString(), "Google userid invalid");
            }
            user = await _repository.FistOrDefaultAsync(q => q.FacebookId.Equals(request.GoogleId));
            if (user != null)
            {
                return Ok(new AuthenticateResponse
                {
                    AccessToken = GenerateAccessToken(user),
                    RefreshToken = generateRefreshToken(user.UserName).Token,
                    //UserInformation = InfoUser,
                });
            }
            if (string.IsNullOrEmpty(request.Email))
            {
                return Unauthorized(StatusCodes.Status401Unauthorized.ToString(), "vui lòng bổ sung thêm email");
            }
            user = new User()
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                FullName = request.Email,
                Source = Settings.SOURCE,
                GoogleId = request.GoogleId
            };
            await _userManager.CreateAsync(user);
            await _userManager.AddToRoleAsync(user, Settings.ENDUSERROLE);
            var result = new AuthenticateResponse
            {
                AccessToken = GenerateAccessToken(user),
                RefreshToken = generateRefreshToken(user.UserName).Token,
                //UserInformation = InfoUser,
            };
            return Ok(result);
        }

        public async Task<ServiceResponse> AuthenticateAppleEmailAsync(LoginViewAppleEmailModel request)
        {
            User user;
            if (string.IsNullOrEmpty(request.AppleId))
            {
                return Unauthorized(StatusCodes.Status401Unauthorized.ToString(), "Apple userid invalid");
            }
            user = await _repository.FistOrDefaultAsync(q => q.AppleId.Equals(request.AppleId));
            if (user != null)
            {
                return Ok(new AuthenticateResponse
                {
                    AccessToken = GenerateAccessToken(user),
                    RefreshToken = generateRefreshToken(user.UserName).Token,
                    //UserInformation = InfoUser,
                });
            }
            if (string.IsNullOrEmpty(request.Email))
            {
                return Unauthorized(StatusCodes.Status401Unauthorized.ToString(), "vui lòng bổ sung thêm email");
            }
            user = new User()
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                FullName = request.Email,
                Source = Settings.SOURCE,
                AppleId = request.AppleId
            };
            await _userManager.CreateAsync(user);
            await _userManager.AddToRoleAsync(user, Settings.ENDUSERROLE);
            var result = new AuthenticateResponse
            {
                AccessToken = GenerateAccessToken(user),
                RefreshToken = generateRefreshToken(user.UserName).Token,
                //UserInformation = InfoUser,
            };
            return Ok(result);
        }

        public string GetConnection()
        {
            var connection = _configuration.GetSection("ConnectionStrings").GetSection("default").Value;
            return connection;
        }

        public async Task<ServiceResponse> UpdateForgetPassword(UpdateForgetPasswordViewModel model)
        {
            var connectionString = this.GetConnection();
            using var con = new SqlConnection(connectionString);

            try
            {
                con.Open();
                var xacThucId = await con.ExecuteScalarAsync<Guid>($"SELECT TOP 1 Id FROM OtpCodeLog WHERE Email = '{model.Email}' AND Code = '{model.OtpCode}' AND ThoiGianXacThuc IS NULL");
                if (!Guid.Empty.Equals(xacThucId))
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (await _userManager.CheckPasswordAsync(user, model.Password))
                    {
                        return Unauthorized("404", "Mật khẩu đã tồn tại, vui lòng nhập lại Mật khẩu");
                    }
                    var passwordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);
                    user.PasswordHash = passwordHash;
                    await _userManager.UpdateAsync(user);
                    await con.ExecuteAsync($"UPDATE OtpCodeLog SET ThoiGianXacThuc = GETDATE() WHERE Email = '{model.Email}' AND Code = '{model.OtpCode}'");
                    //var result = new AuthenticateResponse
                    //{
                    //    AccessToken = GenerateAccessToken(user),
                    //    RefreshToken = generateRefreshToken(user.UserName).Token,
                    //    //UserInformation = InfoUser,
                    //};
                    return Ok(true);
                }
                return Unauthorized("404", "Mã xác thực không đúng vui lòng thử lại");

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

        public async Task<ServiceResponse> UpdateAnh(AnhModel model, string userId)
        {
            var connectionString = this.GetConnection();
            using var con = new SqlConnection(connectionString);

            try
            {
                con.Open();
                var result = await con.QueryFirstAsync<ThongTinCaNhan>($"UPDATE [Users] SET AnhId = {model.FileId} WHERE Id = '{userId}' ; EXEC WinwinKorea_Auth_Account_GetThongTinCaNhan '{userId}'");
                return Ok(result);
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

        public async Task<ServiceResponse> CapNhatThongTinCaNhan(ThongTinCaNhanModel model, string userId)
        {
            var connectionString = this.GetConnection();
            using var con = new SqlConnection(connectionString);

            try
            {
                con.Open();
                var query = $"UPDATE [Users] SET Ten = N'{model.Ten}',PhoneNumber = '{model.SoDienThoai}',PhuongXaId = {model.PhuongXaId},Address = N'{model.DiaChi}',Dob = [dbo].[fn_ConvertLongToDateTime]({model.Dob}) WHERE Id = '{userId}'; EXEC WinwinKorea_Auth_Account_GetThongTinCaNhan '{userId}'";
                var result = await con.QueryFirstAsync<ThongTinCaNhan>(query);
                return Ok(result);
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

        public async Task<ServiceResponse> GetThongTinCaNhan(string userId)
        {
            var connectionString = this.GetConnection();
            using var con = new SqlConnection(connectionString);

            try
            {
                con.Open();
                var query = $"WinwinKorea_Auth_Account_GetThongTinCaNhan '{userId}'";
                var result = await con.QueryFirstAsync<ThongTinCaNhan>(query);
                return Ok(result);
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

        public async Task<ServiceResponse> UpdatePassword(UpdatePasswordModel model, string UserName)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(UserName);

                if (user == null)
                {
                    return Unauthorized("401", "Không tìm thấy tài khoản");
                }
                var IsOk = await _userManager.CheckPasswordAsync(user, model.OldPassword);
                if (!IsOk)
                {
                    return Unauthorized("401", "Sai mật khẩu,vui lòng thử lại");
                }
                var passwordHash = _userManager.PasswordHasher.HashPassword(user, model.NewPassword);
                user.PasswordHash = passwordHash;
                await _userManager.UpdateAsync(user);
                var result = new AuthenticateResponse
                {
                    AccessToken = GenerateAccessToken(user),
                    RefreshToken = generateRefreshToken(user.UserName).Token,
                    TruongHocId = user.TruongHocId.GetValueOrDefault()
                    //UserInformation = InfoUser,
                };
                return Ok(result);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }

        public async Task<ServiceResponse> lockUser(string UserName)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(UserName);

                await _userManager.SetLockoutEnabledAsync(user, true);

                return Ok();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }

        public async Task<ServiceResponse> RegisterForNhaTruong(RegisterForNhaTruongViewModelEmail model)
        {
            var connectionString = this.GetConnection();
            using var con = new SqlConnection(connectionString);

            try
            {
                con.Open();
                var userRegister = new User()
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.Email,
                    Source = Settings.SOURCE
                };
                var userResult = await _userManager.FindByEmailAsync(model.Email);

                if (userResult != null)
                {
                    return Unauthorized("401", "Tài khoản đã tồn tại");
                }
                userRegister.Id = Guid.NewGuid();
                userRegister.TruongHocId = model.TruongHocId;
                await _userManager.CreateAsync(userRegister, model.Password);
                await _userManager.AddToRoleAsync(userRegister, Settings.NHATRUONGROLE);
                var InfoUser = _repository.ListAll().Where(x => x.Email == model.Email).FirstOrDefault();
                var result = new AuthenticateResponse
                {
                    AccessToken = GenerateAccessToken(InfoUser),
                    RefreshToken = generateRefreshToken(InfoUser.UserName).Token,
                    //UserInformation = InfoUser,
                };
                return Ok(result);
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

        public async Task<ServiceResponse> UpdateFireBaseDeviceToken(FireBaseDeviceTokenModel model, string userId)
        {
            var connectionString = this.GetConnection();
            using var con = new SqlConnection(connectionString);

            try
            {
                con.Open();
                var query = $"INSERT INTO [UserDeviceToken](UserId,DeviceToken,Role,NgayTao) VALUES ('{userId}', '{model.DeviceToken}', '{model.Role}', GETDATE()); SELECT 1;";
                var result = await con.ExecuteScalarAsync<bool>(query);
                return Ok(true);
            }
            catch (Exception ex)
            {
                return Unauthorized("404", "Cập nhật device token thất bại");
            }
            finally
            {
                con.Close();
            }
        }

        public async Task<ServiceResponse> GetAllRole()
        {
            var connectionString = this.GetConnection();
            using var con = new SqlConnection(connectionString);

            try
            {
                con.Open();
                var query = "SELECT Id, Name, NormalizedName FROM [Roles]";
                var result = await con.QueryAsync<MyRole>(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized("404", "Cập nhật device token thất bại");
            }
            finally
            {
                con.Close();
            }
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse> GetClaim(Guid IdRole)
        {
            var connectionString = this.GetConnection();
            using var con = new SqlConnection(connectionString);

            try
            {
                con.Open();
                var query = "SELECT * FROM Claims a Left Join Claims b ON a.Id = b.IdParent WHERE a.IdRole = @IdRole AND a.IdParent is null ";

                var lookup = new Dictionary<Guid, lstClaim>();

                var result = await con.QueryAsync<lstClaim, lstClaimDetail, lstClaim>(query, (s, a) =>
                {
                    lstClaim lstClaim;
                    if (!lookup.TryGetValue(s.Id, out lstClaim))
                    {
                        lookup.Add(s.Id, lstClaim = s);
                    }
                    if (lstClaim.lstClaimDetail == null)
                        lstClaim.lstClaimDetail = new List<lstClaimDetail>();
                    if (a != null)
                    {
                        lstClaim.lstClaimDetail.Add(a);
                    }

                    return lstClaim;
                },
                param: new { IdRole = IdRole }
                );

                return Ok(lookup.Values.ToList());
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                con.Close();
            }
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse> updateAvater(updateUserModel userModel, string UserName)
        {
            var user = await _userManager.FindByNameAsync(UserName);
            user.DistrictId = userModel.DistrictId;
            user.CityId = userModel.CityId;
            user.WardId = userModel.WardId;
            user.StreetId = userModel.StreetId;
            user.Address = userModel.Address;
            user.Email = userModel.Email;
            user.Birthday = userModel.Birthday;
            user.FullName = userModel.FullName;
            user.IdNhanVien = userModel.NhanVienId;
            user.PhoneNumber = userModel.Mobile;

            return Ok(await _userManager.UpdateAsync(user));
        }

        public async Task<ServiceResponse> updateMultiAvatar(IFormFile avatar, string UserName)
        {
            var user = await _userManager.FindByNameAsync(UserName);

            if (avatar != null && avatar.Length > 0)
            {
                user.Avatar = await _fileService.UploadImage(avatar);
            }
            else
            {
                user.Avatar = null;
            }

            return Ok(await _userManager.UpdateAsync(user));
        }

        public async Task<ServiceResponse> getUser(string UserName)
        {
            var user = await _userManager.FindByNameAsync(UserName);

            return Ok(new
            {
                username = UserName,
                id = user.Id,
                avatar = user.Avatar,
                ten = user.FullName,
                email = user.Email,
                Phone = user.PhoneNumber,
                birthday = user.Birthday,
                CityId = user.CityId,
                WardId = user.WardId,
                StreetId = user.StreetId,
                Address = user.Address,
                District = user.DistrictId
            });
        }

        public async Task<ServiceResponse> getAllUser(string search)
        {
            var lstuser = new List<User>();

            if (!string.IsNullOrEmpty(search))
            {
                lstuser = await _repository.WhereAsync(x => (x.FullName.Contains(search) || x.UserName.Contains(search)) && x.Source != "WinwinKorea");
            }
            else
            {
                lstuser = await _repository.WhereAsync(x => x.Source != "WinwinKorea");
            }

            var result = new List<object>();

            foreach (var item in lstuser)
            {
                result.Add(new
                {
                    username = item.UserName,
                    id = item.Id,
                    avatar = item.Avatar,
                    ten = item.FullName,
                    email = item.Email,
                    Phone = item.PhoneNumber,
                    birthday = item.Birthday,
                    CityId = item.CityId,
                    WardId = item.WardId,
                    StreetId = item.StreetId,
                    Address = item.Address,
                    District = item.DistrictId
                });
            }

            return Ok(result);
        }

        public async Task<ServiceResponse> getAllUserbyId(Guid Id)
        {
            var lstuser = new User();

            lstuser = await _repository.FistOrDefaultAsync(x => x.Id == Id);

            var result = new object();
            
                result = new
                {
                    isActive = lstuser.IsActive,
                    username = lstuser.UserName,
                    id = lstuser.Id,
                    avatar = lstuser.Avatar,
                    ten = lstuser.FullName,
                    email = lstuser.Email,
                    Phone = lstuser.PhoneNumber,
                    birthday = lstuser.Birthday,
                    CityId = lstuser.CityId,
                    WardId = lstuser.WardId,
                    StreetId = lstuser.StreetId,
                    Address = lstuser.Address,
                    District = lstuser.DistrictId
                };
            return Ok(result);
        }


        public async Task<ServiceResponse> updateRole(string UserName)
        {
            var user = await _userManager.FindByNameAsync(UserName);

            var claims = new List<Claim>
                {
                  new Claim(ClaimTypes.Name, UserName),
                  new Claim(ClaimTypes.Role, "User"),
                  new Claim("Role", "Admin"),
               };

            await _userManager.AddClaimsAsync(user, claims);

            return Ok();
        }

        public async Task<ServiceResponse> infoUser(int PageIndex, int PageSize, string Search, Guid? idPhongBan, bool? IsActive) => Ok(await _userRepository.InfoUser(PageIndex, PageSize, Search, idPhongBan, IsActive));

        public async Task<ServiceResponse> UnActiveUser(Guid UserId) => Ok(await _userRepository.UnActiveUser(UserId));

        public async Task<ServiceResponse> deleteUser(Guid UserId) => Ok(await _userRepository.deleteUser(UserId));

        public async Task<ServiceResponse> addPermission(Permissions Model) => Ok(await _userRepository.addPermission(Model));

        public async Task<ServiceResponse> GetByIdUser(Guid IdUser) => Ok(await _userRepository.GetByIdUser(IdUser));

        public async Task<ServiceResponse> UpdatePer(Permissions Model) => Ok(await _userRepository.Update(Model));

        public async Task<ServiceResponse> CreateNewAccount(UserRegister User)
        {
            var checkMobile = await _repository.FistOrDefaultAsync(x => x.UserName == User.UserName);

            if (checkMobile != null)
            {
                return Forbidden("", "Tài khoản đã tồn tại");
            }

            var userRegister = new User()
            {
                UserName = User.UserName,
                FullName = User.UserName,
                IsActive = true,
                IsDeleted = false
            };

            //var roles = new List<string>();
            //roles.Add("EndUser");
            userRegister.Id = Guid.NewGuid();
            var resultCreated = await _userManager.CreateAsync(userRegister, User.Password);

            var InfoUser = await _userManager.FindByNameAsync(userRegister.UserName);

            var roles = await _userRepository.GetPermission(InfoUser.Id);

            var UserInfo = new UserInfo()
            {
                Id = InfoUser.Id,
                UserName = InfoUser.UserName,
                FullName = InfoUser.FullName,
                Birthday =  InfoUser.Birthday,
                Email = InfoUser.Email,
                Permission = roles
            };

            if (resultCreated.Succeeded)
            {
                return Ok(UserInfo.Id);
            }

            //return Ok(UserInfo);
            return BadRequest();
        }

        public async Task<ServiceResponse> GetUserNew(Guid Id)
        {
             var lstuser = await _userRepository.GetUserId(Id);
            if (lstuser == null) { return BadRequest("400","Không tìm thấy user authenca"); }
            return Ok(lstuser);
        }
        public async Task<ServiceResponse> UpdatePasswordNew(UpdatePasswordRequest model)
        {
            var user = _repository.FistOrDefault(x=>x.UserName == model.UserName);

            if (user == null)
            {
                return BadRequest("", "Tài khoản không tồn tại");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            return Ok(result);
        }
    }
}
