using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Domain.Models.OTP
{
    public class OTPModel
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public DateTime? ThoiGianXacThuc { get; set; }

        public DateTime? ThoiGianKhoiTao { get; set; }
    }
}
