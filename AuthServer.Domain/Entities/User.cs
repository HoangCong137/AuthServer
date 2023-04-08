using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AuthServer.Domain.Entities
{
    public class User : IdentityUser<Guid>, IEntity<Guid>
    {
        [MaxLength(50)]
        public string FullName { get; set; }

        public string Source { get; set; }
        public string FacebookId { get; set; }
        public string AppleId { get; set; }
        public string GoogleId { get; set; }
        public long? TruongHocId { get; set; }
        public int? IdSalon { get; set; }
        public int? IdTechnician { get; set; }
        public string Avatar { get; set; }
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public int? StreetId { get; set; }
        public int? WardId { get; set; }
        public string Address { get; set; }
        public DateTime? Birthday { get; set; }
        public Guid? IdNhanVien { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? LoginTime { get; set; }
        public DateTime? Created_at { get; set; } = DateTime.Now;
    }

    public class ThongTinCaNhan
    {
        public string AnhUrl { get; set; }
        public string Ten { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public long Dob { get; set; }
        public int PhuongXaId { get; set; }
        public string TenPhuongXa { get; set; }
        public string TenQuanHuyen { get; set; }
        public string TenTinh { get; set; }
        public string DiaChi { get; set; }

    }
    public class MyRole
    {
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public Guid Id { get; set; }
    }

    public class lstClaim
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public Guid? IdParent { get; set; }
        public List<lstClaimDetail> lstClaimDetail { get; set; }
    }

    public class lstClaimDetail
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
    }
    public class GetUser
    {
        public string Username { get; set; }

        public static implicit operator GetUser(User v)
        {
            throw new NotImplementedException();
        }
    }
}
