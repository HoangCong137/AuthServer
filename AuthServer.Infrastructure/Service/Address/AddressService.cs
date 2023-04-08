using AuthServer.Infrastructure.Common.Service;
using AuthServer.Domain.Entities;
using AuthServer.Infrastructure.Repository;
using AuthServer.Infrastructure.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Infrastructure.Service.Address
{
    public class AddressService : SResponse , IAddressService
    {
        private readonly IAsyncRepository<City> _repositoryCity;

        private readonly IAsyncRepository<District> _repositoryDistrict;

        private readonly IAsyncRepository<Ward> _repositoryWard;

        private readonly IAsyncRepository<Street> _repositoryStreet;

        public AddressService(IAsyncRepository<City> repositoryCity, IAsyncRepository<District> repositoryDistrict
                                              , IAsyncRepository<Ward> repositoryWard, IAsyncRepository<Street> repositoryStreet)
        {
            _repositoryCity = repositoryCity;

            _repositoryDistrict = repositoryDistrict;

            _repositoryWard = repositoryWard;

            _repositoryStreet = repositoryStreet;
        }

        public async Task<ServiceResponse> ListCity()
        {
            return Ok(await _repositoryCity.ListAllAsync());
        }

        public async Task<ServiceResponse> DistrictbyCityId(int id)
        {
            return Ok(await _repositoryDistrict.WhereAsync(x => x.CityId == id));
        }

        public async Task<ServiceResponse> WardbyDistrictId(int id)
        {
            return Ok(await _repositoryWard.WhereAsync(x => x.DistrictId == id));
        }

        public async Task<ServiceResponse> StreetbyDistrictId(int id)
        {
            return Ok(await _repositoryStreet.WhereAsync(x => x.DistrictId == id));
        }
    }
}
