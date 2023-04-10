using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Domain.Services.Address
{
    public interface IAddressService
    {
        Task<ServiceResponse> ListCity();

        Task<ServiceResponse> DistrictbyCityId(int id);

        Task<ServiceResponse> WardbyDistrictId(int id);

        Task<ServiceResponse> StreetbyDistrictId(int id);
    }
}
