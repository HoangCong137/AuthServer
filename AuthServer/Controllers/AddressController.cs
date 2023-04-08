using AuthServer.Infrastructure.Service.Address;
using AuthServer.Infrastructure.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.Controllers
{
    [Route("api/addr")]
    [ApiController]
    public class AddressController : Controller
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet("getAllCity")]
        public async Task<ServiceResponse> getAllCity() => await _addressService.ListCity();

        [HttpGet("getDistrict")]
        public async Task<ServiceResponse> getDistrict(int Id) => await _addressService.DistrictbyCityId(Id);

        [HttpGet("getWard")]
        public async Task<ServiceResponse> getWard(int Id) => await _addressService.WardbyDistrictId(Id);

        [HttpGet("getStreet")]
        public async Task<ServiceResponse> getStreet(int Id) => await _addressService.StreetbyDistrictId(Id);

    }
}
