using AuthServer.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Infrastructure.Model.Auth
{
    public class UserModel
    {
        public IFormFile Avatar { get; set; }
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public int? StreetId { get; set; }
        public int? WardId { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public string UserName { get; set; }
        public DateTime? LoginTime { get; set; }
        public Guid NhanVienId { get; set; }
        public NhanSu NhanVien { get; set; }
    }

    public class lstUser
    {
        public Guid? Id { get; set; }
        public string Avatar { get; set; }
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public int? StreetId { get; set; }
        public int? WardId { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public string UserName { get; set; }
        public DateTime? LoginTime { get; set; }
        public DateTime? Created_at { get; set; }
        public bool? IsActive { get; set; }
        public NhanSu NhanVien { get; set; }
    }

    public class NhanSu
    {

        public Guid Id { get; set; }

        public string MaNhanVien { get; set; }

        #region PhongBan
        public Guid PhongBanId { get; set; }
        public PhongBan PhongBan { get; set; }
        #endregion

        public string EmailCaNhan { get; set; }

        public string EmailCongTy { get; set; }

        public DateTime? DOB { get; set; }

        public bool GioiTinh { get; set; }

        public string Ten { get; set; }

        public bool HoatDong { get; set; }

        public string Avatar { get; set; }

        public DateTime? Created_at { get; set; }

        public bool is_deleted { get; set; }

        public string DiaChi { get; set; }

        public string SoDienThoai { get; set; }

    }

    public class PhongBan
    {
        public Guid Id { get; set; }

        public string Ten { get; set; }

        public string AnhUrl { get; set; }

        public bool IsActive { get; set; }

        public DateTime Created_at { get; set; }

        public string MaPhongBan { get; set; }
    }

    public class UserInfo
    {
        public Guid Id { get; set; }
        public Guid? IdNhanVien { get; set; }
        public int? IdSalon { get; set; }
        public int? IdTechnician { get; set; }
        public string UserName { get; set; }
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public int? StreetId { get; set; }
        public int? WardId { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public Permissions Permission { get; set; }
    }

    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<RoleClaim> RoleClaims { get; set; } = new List<RoleClaim>();
    }

    public class RoleClaim
    {
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
    }

    public class RoleClaimDetail
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
    }

    public class updateUserModel
    {
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public int? StreetId { get; set; }
        public int? WardId { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public string UserName { get; set; }
        public DateTime? LoginTime { get; set; }
        public Guid NhanVienId { get; set; }
        public bool IsActive { get; set; }
    }
}
