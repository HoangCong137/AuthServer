using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Domain.Models.Auth
{
    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
