using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Infrastructure.ViewModel
{
    public class ThongTinCaNhanModel
    {
        public string Ten { get; set; }
        public string SoDienThoai { get; set; }
        public int PhuongXaId { get; set; }
        public long Dob { get; set; }
        public string DiaChi { get; set; }
    }
    public class AnhModel
    {
        public int FileId { get; set; }
    }
}
