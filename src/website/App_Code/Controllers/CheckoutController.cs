using System;
using System.Collections.Generic;
using System.Linq;
using Merchello.Core;
using Merchello.Core.Models;
using System.Web;
using Umbraco.Web.Mvc;
using Umbraco.Core.Logging;
using Merchello.Web;
using Merchello.Web.Workflow;
using System.Web.Mvc;
using Site.Models;

namespace Site.Controllers
{

    [PluginController("Site")]
    public class CheckoutController : SiteContollerBase
    {
        
        public CheckoutController()
             : this(MerchelloContext.Current)
         {}

        public CheckoutController(IMerchelloContext merchelloContext) : base(merchelloContext)
         {}

        [ChildActionOnly]
        public ActionResult RenderShippingAddressForm()
        {
            return PartialView("CheckoutStep1Address");
        }

        [HttpPost]
        public ActionResult SaveShippingAddress(AddressModel model)
        {
            if (ModelState.IsValid)
            {

                //can be done in view:
                var shippingAddress = model.ToAddress();
                Basket.SalePreparation().SaveShipToAddress(shippingAddress);
//                Basket.SalePreparation().SaveBillToAddress(address);
                var shipment = Basket.PackageBasket(shippingAddress).FirstOrDefault(); //This is an enumeration of IShipRateQuote which will be used to populate drop-down to choose shipping method. Can be called from view.
                //check for null shipment before foreach
                foreach(var quote in shipment.ShipmentRateQuotes())
                {
                    var key = quote.ShipMethod.Key;
                    var name = quote.ShipMethod.Name;

                }
                
                //this happens on postback (it's the method they chose). It includes a rate.
                //var shipmentAgain = Basket.PackageBasket(shippingAddress).FirstOrDefault();
                //var approvedQuote = shipmentAgain.ShipmentRateQuoteByShipMethod(shipMethodKey); //Got param from drop-down...
                //Basket.SalePreparation().SaveShipmentRateQuote(approvedQuote); //saves their chosen shipment method.

                //NOTE: This assumes one shipment type.
                //Basket.SalePreparation().SaveShipmentRateQuote();

                //TO SHOW INVOICE:
                //var invoice = Basket.SalePreparation().PrepareInvoice(); //creates invoice but doesn't save to database. E.g. calc tax, shipping, etc.
                
                //PAYMENT
                //var paymentMethods = Basket.SalePreparation().GetPaymentGatewayMethods(); //IPaymentGatewayMethod model.
                //(use key from kitten site).


                if (Basket.SalePreparation().IsReadyToInvoice()) //technically is happening in .PrepareInvoice() automatically. but can be used as a check if you want.
                {
                    var invoice = Basket.SalePreparation().PrepareInvoice();

                    var attempt = invoice.AuthorizePayment(Guid.NewGuid()); //returns a payment result.
                    if (attempt.Payment.Success)
                    {
                        var payment = attempt.Payment.Result;
                        var invoiceAgain = attempt.Invoice;
                    }

                    //POST BACK PAYMENT 
                    //invoice.AuthorizePayment(1);//paymentMethodKey -- from dropdown, //overloads might be used for some providers, provider dependent.
                    //authorizepayment above will save to database
                }


                //Notification.Trigger("OrderConfirmation", attempt); //coming soon.

            }
            return RedirectToUmbracoPage(1062);
        }




    }
}