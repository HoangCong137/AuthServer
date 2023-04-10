using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Domain.Models.Auth
{
    public class AuthenticateResponse : AuthResponse
    {
        public long TruongHocId { get; set; } = 0;
        public UserInfo UserInformation { get; set; }
        public IEnumerable<string> Role { get; set; }
        //public List<PermissionDetailResponse> ListPermissions { get; set; }
    }
}
