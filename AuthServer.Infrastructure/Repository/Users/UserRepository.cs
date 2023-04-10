using AuthServer.Domain.Entities;
using AuthServer.Domain.Interfaces;
using AuthServer.Domain.Models.Auth;
using AuthServer.Domain.Models.OTP;
using AuthServer.Domain.Models.Result;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Infrastructure.Repository.Users
{
    public class UserRepository : IUserRepository
    {

        private readonly IConfiguration _configuration;

        public UserRepository(IConfiguration configuration)
        {
            _configuration = configuration;

        }

        public async Task<PagedResult<lstUser>> InfoUser(int PageIndex, int PageSize, string Search,Guid? idPhongBan, bool? IsActive)
        {
            var connectionString = this.GetConnection();

            using var con = new SqlConnection(connectionString);
            try
            {
                con.Open();

                var query = @"SELECT *
                                FROM [Users] u left join TuanChau.dbo.NhanSu n ON U.IdNhanVien = N.Id 
                                left join TuanChau.dbo.PhongBan p ON n.PhongBanId = p.Id WHERE u.IsDeleted = 0";

                var queryTotal = @"SELECT COUNT(*)
                                FROM [Users] u left join TuanChau.dbo.NhanSu n ON U.IdNhanVien = N.Id 
                                left join TuanChau.dbo.PhongBan p ON n.PhongBanId = p.Id WHERE u.IsDeleted = 0";

                if (!string.IsNullOrEmpty(Search))
                {
                    query += " AND (u.UserName like CONCAT('%',@keyword,'%') or n.Ten like CONCAT('%',@keyword,'%')" +
                        " or n.MaNhanVien like CONCAT('%',@keyword,'%') or n.SoDienThoai like CONCAT('%',@keyword,'%'))";

                    queryTotal += " AND (u.UserName like CONCAT('%',@keyword,'%') or n.Ten like CONCAT('%',@keyword,'%')" +
                        " or n.MaNhanVien like CONCAT('%',@keyword,'%') or n.SoDienThoai like CONCAT('%',@keyword,'%'))";
                }

                if (idPhongBan.HasValue)
                {
                    query += " AND n.PhongBanId = @idPhongBan";

                    queryTotal += " AND n.PhongBanId = @idPhongBan";
                }

                if (IsActive.HasValue)
                {
                    query += " AND u.IsActive = @IsActive";

                    queryTotal += " AND u.IsActive = @IsActive";
                }

                query += " ORDER BY u.Created_at DESC";

                var paging = @" OFFSET (@PageIndex -1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;";

                var multi = await con.QueryMultipleAsync(query + paging + queryTotal, new { PageIndex = PageIndex, PageSize = PageSize, keyword = Search, idPhongBan = idPhongBan, IsActive = IsActive });

                var result = new PagedResult<lstUser>();

                result.Items = multi.Read<lstUser, NhanSu, PhongBan, lstUser>((p, t, u) =>
                 {
                     p.NhanVien = t;
                     if (t != null)
                     {
                         t.PhongBan = u;
                     }
                     return p;
                 }).ToList();

                result.TotalCount = multi.ReadFirst<int>();

                result.PageIndex = PageIndex;

                result.PageSize = PageSize;

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                con.Close();
            }
        }

        public async Task<Permissions> GetPermission(Guid idUser)
        {
            var connectionString = this.GetConnection();
            using var con = new SqlConnection(connectionString);
            try
            {
                con.Open();

                var query = @"SELECT * FROM Permission Where IdUser = @IdUser";

                var result = await con.QueryAsync<Permissions>(query, new { idUser  = idUser });

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                con.Close();
            }
        }

        public async Task<bool> UnActiveUser(Guid idUser)
        {
            var connectionString = this.GetConnection();
            using var con = new SqlConnection(connectionString);
            try
            {
                con.Open();

                var query = @"Update Users set isActive = 0 Where Id = @IdUser";

                var result = await con.QueryAsync(query, new { idUser = idUser });

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                con.Close();
            }
        }

        public async Task<bool> deleteUser(Guid idUser)
        {
            var connectionString = this.GetConnection();
            using var con = new SqlConnection(connectionString);
            try
            {
                con.Open();

                var query = @"Update Users set isDeleted = 1 Where Id = @IdUser AND isActive = 0";

                var count = await con.QueryAsync<int>(query, new { idUser = idUser });

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                con.Close();
            }
        }

        public async Task<Permissions> addPermission(Permissions Model)
        {
            var connectionString = this.GetConnection();
            using var con = new SqlConnection(connectionString);
            try
            {
                con.Open();

                var query = @"INSERT INTO Permission (Id, IdUser, Permission, RoomLock, is_deleted) OUTPUT INSERTED.* Values ( @Id, @IdUser, @Permission, @RoomLock, 0)";
                //var query = @"INSERT INTO Permission (Id, IdUser, Permission, RoomLock, is_deleted) OUTPUT INSERTED.* Values ( @Id, @IdUser,"
                //            + "REPLACE(REPLACE(@Permission,'{\"value\":\"QuanLyBangGia\",\"Detail\":[]}', '{\"value\":\"QuanLyBangGia\",\"Detail\":[\"PH\",\"KS\",\"PC\",\"TQ\",\"CV\",\"TT\"]}'),'{\"value\":\"DuyetBangGia\",\"Detail\":[]}','{\"value\":\"DuyetBangGia\",\"Detail\":[\"PH\",\"KS\",\"PC\",\"TQ\",\"CV\",\"TT\"]}')"
                //            + ", @RoomLock, 0";

                var result = await con.QueryAsync<Permissions>(query, Model);

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                con.Close();
            }
        }

        public async Task<Permissions> GetByIdUser(Guid IdUser)
        {
            var connectionString = this.GetConnection();
            using var con = new SqlConnection(connectionString);
            try
            {
                con.Open();

                var query = @"SELECT * FROM Permission where IdUser = @IdUser";

                var result = await con.QueryAsync<Permissions>(query, new { IdUser = IdUser });

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                con.Close();
            }
        }

        public async Task<bool> Update(Permissions Model)
        {
            var connectionString = this.GetConnection();
            using var con = new SqlConnection(connectionString);
            try
            {
                con.Open();

                var query = @"Update Permission set Permission = @Permission, RoomLock = @RoomLock where id = @Id";
                //var query = @"Update Permission set Permission = "
                //            + "REPLACE(REPLACE(@Permission,'{\"value\":\"QuanLyBangGia\",\"Detail\":[]}', '{\"value\":\"QuanLyBangGia\",\"Detail\":[\"PH\",\"KS\",\"PC\",\"TQ\",\"CV\",\"TT\"]}'),'{\"value\":\"DuyetBangGia\",\"Detail\":[]}','{\"value\":\"DuyetBangGia\",\"Detail\":[\"PH\",\"KS\",\"PC\",\"TQ\",\"CV\",\"TT\"]}')"
                //            + ", RoomLock = @RoomLock where id = @Id";

                var result = await con.QueryAsync<Permissions>(query, Model);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                con.Close();
            }
        }

        public async Task<OTPModel> GetOTP(string Email, string OTP)
        {
            var connectionString = this.GetConnection();
            using var con = new SqlConnection(connectionString);
            try
            {
                con.Open();

                var query = @"Select * from OtpCodeLog where Email = @Email and Code = @OTP";

                var result = await con.QueryAsync<OTPModel>(query, new { Email = Email , OTP = OTP});

                return result.First();
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                con.Close();
            }
        }

        public async Task<int> addTechnician(string AccountName)
        {
            var connectionString = this.GetConnectionNail();

            using var con = new SqlConnection(connectionString);
            try
            {
                con.Open();

                var query = @"INSERT INTO Technician (Email, AccountName, IsActive, CreatedAt, IsDeleted) OUTPUT INSERTED.Id Values ( @Email, @AccountName, 1, @CreatedAt, 0)";

                var result = await con.QueryAsync<int>(query, new { AccountName = AccountName, Email = AccountName, CreatedAt = DateTime.Now }) ;

                return result.FirstOrDefault();
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

        public async Task<int> addSalon(string AccountName)
        {
            var connectionString = this.GetConnectionNail();

            using var con = new SqlConnection(connectionString);
            try
            {
                con.Open();

                var query = @"INSERT INTO Salon (Email, AccountName, IsActive, CreatedAt, IsDeleted) OUTPUT INSERTED.Id Values ( @Email, @AccountName, 1, @CreatedAt, 0)";

                var result = await con.QueryAsync<int>(query, new { AccountName = AccountName, Email = AccountName, CreatedAt = DateTime.Now });

                return result.FirstOrDefault();
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
        public async Task<IEnumerable<string>> getAuthorityGroup(Guid id)
        {
            var connectionString = this.GetConnection();

            using var con = new SqlConnection(connectionString);
            try
            {
                con.Open();
                var query = "proc_GetAuthorityGroupOfUser";
                var result = await con.QueryAsync<string>(query, new { Id = id }, commandType: System.Data.CommandType.StoredProcedure);

                return result;


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                con.Close();
            }
            
        }
        public string GetConnectionLuxDeal()
        {
            var connection = _configuration.GetSection("ConnectionStrings").GetSection("CFTERP").Value;
            return connection;

        }

        public string GetConnection()
        {
            var connection = _configuration.GetSection("ConnectionStrings").GetSection("default").Value;
            return connection;
        }

        public string GetConnectionNail()
        {
            var connection = _configuration.GetSection("ConnectionStrings").GetSection("nail").Value;
            return connection;
        }

        public async Task<string> GetUserId(Guid Id)
        {
            var con = new SqlConnection(this.GetConnection());
            try
            {
                con.Open();
                var query = "Proc_Erp_User_GetId";
                var respon = await con.QueryFirstOrDefaultAsync<string>(query, new { Id }, commandType: System.Data.CommandType.StoredProcedure);
                return respon;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception("Get Id Authen fails");
            }
            finally { con.Close(); }
        }
    }
}
