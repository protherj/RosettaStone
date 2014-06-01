using System;
using System.Collections.Generic;
using System.Linq;
using Merchello.Core;
using Merchello.Core.Models;
using Models;
using Umbraco.Web.Mvc;
using Merchello.Web;
using System.Web.Mvc;

namespace Controllers
{

    [PluginController("RosettaStone")]
    public class CheckoutController : MerchelloSurfaceContoller
    {
        
        public CheckoutController()
             : this(MerchelloContext.Current)
         {}

        public CheckoutController(IMerchelloContext merchelloContext) : base(merchelloContext)
         {}

        [ChildActionOnly]
        public ActionResult RenderAddressForm(AddressType addressType)
        {
            ViewBag.AddressType = addressType;

            // COUNTRIES - We only want to populate the address drop down list with applicable countries.  Merchello uses the ISO Code for the country so we must be specific
            // -------------------------------------------------------------------------------------------------------------

            // For SHIPPING - We have to be able to actually ship to a country so we need to filter the list.  Configuration
            // of which countries we can ship to is do through the Merchello back office in the shipping section.

            // For BILLING - We'll let someone pay from pretty much anyway - so we need a list of all countries

            var countries = AddressType.Shipping == addressType ? AllowableShipCounties.Value : AllCountries.Value;

            var countriesArray = countries as ICountry[] ?? countries.ToArray(); // because ReSharper was having fun

            ViewBag.Countries = countriesArray.Select(x => new SelectListItem()
                {
                    Value = x.CountryCode,
                    Text = x.Name
                });

            // REGIONS - Regions are important for both Shipping and Taxation - Again Merchello uses the ISO Code here so we need to be really specific
            // ---------------------------------------------------------------------------------------------------------------

            // This is going to be a little messy as we really should separate out the regions per country code
            // ... easy to do with an async call on country code change ... but for this example, we are going to group them all
            // together since they are only US and CA at this point (and we're somewhat used to it)

            var regions = BuildProvinceCollection(countriesArray);

            ViewBag.Regions = regions.Select(x => new SelectListItem()
                {
                    Value = x.ProvinceCode,
                    Text = x.Name
                });

            return PartialView("AddressForm");
        }

        /// <summary>
        /// Saves either a Shipping or Billing Address
        /// </summary>
        /// <param name="model">The <see cref="AddressModel"/> to be persisted</param>
        [HttpPost]
        public ActionResult SaveAddress(AddressModel model)
        {
            if(model.AddressType == AddressType.Custom) throw new InvalidOperationException("We are not handling Custom Address Types in this example");

            if (!ModelState.IsValid) return CurrentUmbracoPage();

            // Rosetta extension method "ToAddress()"
            var address = model.ToAddress();

            // Basket has an extension method that handles the "BasketSalesPrepartion" instantiation
            // SalesPrepartionBase is responsible for persisting information needed to facilitate an order
            // while we are collecting enough information to create an invoice and eventually an order.
           

            // Addresses are saved to either an AnonymousCustomer (version 1.1.x) or potentially an established
            // customer (assuming they have logged in).  Established, or persisted customers are slated to be introduced
            // in Merchello Version 1.3.0.  No code change will be required here as the "conversion" will be handled in the
            // "CustomerContext"

            int proceed;
            // Hacky for this example but we are hard coding the "proceed" (Umbraco Content Id) for the next steps

            // 1075 Shipment Rate Quotes
            // 1077 confirmation

            if (model.AddressType == AddressType.Shipping)
            {
                Basket.SalePreparation().SaveShipToAddress(address);
                proceed = 1075;
            }
            else
            {
                Basket.SalePreparation().SaveBillToAddress(address);
                proceed = 1077;
            }

            return RedirectToUmbracoPage(proceed);

        }


        public ActionResult SaveApprovedShipmentRateQuote(Guid shipMethodKey)
        {
            return RedirectToUmbracoPage(1076);
        }

        /// <summary>
        /// Returns a collection of countries Merchello is "allowed" to ship to
        /// </summary>
        /// <remarks>
        /// We want to lazy load this so that we do not have to query for these on every call to the controller ... only when we need them
        /// </remarks>
        private Lazy<IEnumerable<ICountry>> AllowableShipCounties
        {
            get
            {
                return new Lazy<IEnumerable<ICountry>>(() => Shipping.GetAllowedShipmentDestinationCountries().OrderBy(x => x.Name));
            }
        }

        /// <summary>
        /// Returns a collection of all countries Merchello
        /// </summary>
        /// <remarks>
        /// We want to lazy load this so that we do not have to query for these on every call to the controller ... only when we need them
        /// </remarks>
        private Lazy<IEnumerable<ICountry>> AllCountries
        {
            get
            {
                return new Lazy<IEnumerable<ICountry>>(() => Services.StoreSettingService.GetAllCountries().OrderBy(x => x.Name));
            }
        }

        private static IEnumerable<ProvinceModel> BuildProvinceCollection(IEnumerable<ICountry> countries)
        {
            var models = new List<ProvinceModel>();
            foreach (var country in countries)
            {
                models.AddRange(country.Provinces.Select(p => new ProvinceModel() { ProvinceCode = p.Code, Name = p.Name }));
            }
            return models;
        }
    }
}