using AuthServer.Domain.Entities;
using AuthServer.Domain.Models.Auth;
using AuthServer.Domain.Models.OTP;
using AuthServer.Domain.Models.Result;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<PagedResult<lstUser>> InfoUser(int PageIndex, int PageSize, string Search, Guid? idPhongBan, bool? IsActive);
        Task<Permissions> GetPermission(Guid idUser);
        Task<bool> UnActiveUser(Guid idUser);
        Task<bool> deleteUser(Guid idUser);
        Task<Permissions> addPermission(Permissions Model);
        Task<Permissions> GetByIdUser(Guid IdUser);
        Task<bool> Update(Permissions Model);
        Task<OTPModel> GetOTP(string Email, string OTP);
        Task<int> addTechnician(string AccountName);

        Task<int> addSalon(string AccountName);
        Task<IEnumerable<string>> getAuthorityGroup(Guid id);
        Task<string> GetUserId(Guid Id);
    }
}
