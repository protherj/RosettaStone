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
    /// <summary>
    /// SurfaceController responsible for checkout workflow
    /// </summary>
    [PluginController("RosettaStone")]
    public class CheckoutController : MerchelloSurfaceContoller
    {
        // Normally you would not want to hard code these Id's in a controller
        // as it is too easy for a back office user to make a mistake and muck things up

        private const int ShipRateQuoteId = 1075; // Shipment Rate Quotes
        private const int PaymentInfoId = 1076; // Selecting payment information
        private readonly int _confirmationId = 1077; // confirmation

        public CheckoutController()
             : this(MerchelloContext.Current)
         {}

        public CheckoutController(IMerchelloContext merchelloContext) : base(merchelloContext)
         {}

        /// <summary>
        /// Renders the Address Form for shipping and billing address entry
        /// </summary>
        /// <param name="addressType">The <see cref="AddressType"/> - Shipping and Billing are handled</param>
        /// <returns>Partial view</returns>
        /// <remarks>
        /// 
        /// If you are new to MVC, this is "sort of like" the code behind a ASP.NET forms UserControl, where the
        /// /Partials/AddressForm.cshtml would be the .ascx albeit the data posted back is handle a bit different.  We are just
        /// making a reusable "control"
        /// 
        /// STEPS 2 and 3 - on render
        /// </remarks>
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
        /// <remarks>
        /// 
        /// STEP 2 - POSTed data
        /// </remarks>
        [HttpPost]
        public ActionResult SaveAddress(AddressModel model)
        {
            // Error checking defined in AddressModel
            if (!ModelState.IsValid) return CurrentUmbracoPage();

            if(model.AddressType == AddressType.Custom) throw new InvalidOperationException("We are not handling Custom Address Types in this example");

            // Rosetta extension method "ToAddress()"
            var address = model.ToAddress();

            // Basket has an extension method that handles the "BasketSalesPrepartion" instantiation

            // SalesPrepartionBase is responsible for persisting information needed to facilitate a sale (in this case an order)
            // while we are collecting enough information to create an invoice.
           
            // Addresses are saved to either an AnonymousCustomer (version 1.1.x) or potentially an established
            // customer (assuming they have logged in).  Established, or persisted customers are slated to be introduced
            // in Merchello Version 1.3.0.  No code change will be required here as the "conversion" will be handled in the
            // "CustomerContext"

            if (model.AddressType == AddressType.Shipping)
            {
                Basket.SalePreparation().SaveShipToAddress(address);
            }
            else
            {
                // This workflow never uses this as we combined this call with the select payment information 
                Basket.SalePreparation().SaveBillToAddress(address);
            }

            return RedirectToUmbracoPage(ShipRateQuoteId);
        }

        /// <summary>
        /// Saves the customer's approved shipment rate quote to the sales preparation
        /// </summary>
        /// <param name="shipMethodKey">The <see cref="IShipMethod"/> Key (Guid) for the method of shipping selected</param>
        /// <remarks>
        /// 
        /// NOTE: This methodology will change when we expose the multiple shipments as we will need to retain the context
        /// of which shipment each quote is approved.  In this case, there can be only one so we will just package the basket again.
        /// 
        /// STEP 3 - POSTed data
        /// </remarks>
        [HttpPost]
        public ActionResult SaveApprovedShipmentRateQuote(Guid shipMethodKey)
        {
            if (!ModelState.IsValid) return CurrentUmbracoPage();

            var shippingAddress = Basket.SalePreparation().GetShipToAddress();
            if(shippingAddress == null) return RedirectToUmbracoPage(ShipRateQuoteId);

            // Get the shipment again
            var shipment = Basket.PackageBasket(shippingAddress).FirstOrDefault();

            // get the quote using the "approved shipping method"
            var quote = shipment.ShipmentRateQuoteByShipMethod(shipMethodKey);

            // save the quote
            Basket.SalePreparation().SaveShipmentRateQuote(quote);

            return RedirectToUmbracoPage(PaymentInfoId); // Proceed to step 3
        }


        /// <summary>
        /// Utility: Returns a collection of countries Merchello is "allowed" to ship to
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
        /// Utility: Returns a collection of all countries Merchello
        /// </summary>
        /// <remarks>
        /// 
        /// We need a list of "All Countries" to populate the "Billing Address Form"
        /// 
        /// We want to lazy load this so that we do not have to query for these on every call to the controller ... only when we need them
        /// </remarks>
        private Lazy<IEnumerable<ICountry>> AllCountries
        {
            get
            {
                return new Lazy<IEnumerable<ICountry>>(() => Services.StoreSettingService.GetAllCountries().OrderBy(x => x.Name));
            }
        }

        /// <summary>
        /// Utility Helper: Builds a list of states/provinces for a country if applicable.
        /// </summary>
        /// <param name="countries">A collection of <see cref="ICountry"/></param>
        /// <returns>
        /// 
        /// The province list is editable in the merchello.config file.  /App_Plugins/Merchello/Config/merchello.config
        /// 
        /// </returns>
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