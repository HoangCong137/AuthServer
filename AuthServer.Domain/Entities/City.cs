using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Domain.Entities
{
    public class Street
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string UnsignedLoweredName { get; set; }

        public int DistrictId { get; set; }

        public string Full_Address { get; set; }

        public bool is_deleted { get; set; }

        public string Created_by { get; set; }

        public DateTime? Created_at { get; set; }

        public DateTime? Update_date { get; set; }

        public string Update_by { get; set; }
    }

    public class Ward
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string UnsignedLoweredName { get; set; }

        public int DistrictId { get; set; }

        public string Full_Address { get; set; }

        public bool is_deleted { get; set; }

        public string Created_by { get; set; }

        public DateTime? Created_at { get; set; }

        public DateTime? Update_date { get; set; }

        public string Update_by { get; set; }
    }

    public class City
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string UnsignedLoweredName { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public string Full_Address { get; set; }

        public bool is_deleted { get; set; }

        public string Created_by { get; set; }

        public DateTime? Created_at { get; set; }

        public DateTime? Update_date { get; set; }

        public string Update_by { get; set; }
    }

    public class District
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string UnsignedLoweredName { get; set; }

        public int CityId { get; set; }

        public string Full_Address { get; set; }

        public bool is_deleted { get; set; }

        public string Created_by { get; set; }

        public DateTime? Created_at { get; set; }

        public DateTime? Update_date { get; set; }

        public string Update_by { get; set; }
    }
}
