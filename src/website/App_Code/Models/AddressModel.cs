using Merchello.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for AddressModel
/// </summary>
namespace Site.Models
{
    public class AddressModel
    {

        public Guid CustomerKey { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string CountryCode { get; set; }

        public string Email { get; set; }

        public bool IsCommercial { get; set; }

        public string Locality { get; set; }

        public string Name { get; set; }

        public string Organization { get; set; }

        public string Phone { get; set; }

        public string PostalCode { get; set; }

        public string Region { get; set; }
    }

    public static class AddressModelExtensions
    {
        public static IAddress ToAddress(this AddressModel address)
        {
            return new Address()
            {
                Address1 = address.Address1,
                Address2 = address.Address2,
                CountryCode = address.CountryCode,
                Email = address.Email,
                IsCommercial = address.IsCommercial,
                Locality = address.Locality,
                Name = address.Name,
                Organization = address.Organization,
                Phone = address.Phone,
                PostalCode = address.PostalCode,
                Region = address.Region
            };
        }
    }
}