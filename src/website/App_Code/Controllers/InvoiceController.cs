using System;
using System.Web.Mvc;
using Merchello.Core;
using Merchello.Core.Models;
using Merchello.Web;
using Umbraco.Web.Mvc;

namespace Controllers
{
    /// <summary>
    ///SurfaceController responsible for shipment related functions
    /// </summary>
    [PluginController("RosettaStone")]
    public class InvoiceController : MerchelloSurfaceContoller
    {
        public InvoiceController()
            : this(MerchelloContext.Current)
        { }

        public InvoiceController(IMerchelloContext merchelloContext) 
            : base(merchelloContext)
        { }


        [ChildActionOnly]
        public ActionResult RenderPreparedInvoice()
        {
            if (Basket.SalePreparation().IsReadyToInvoice())
            {
                var invoice = Basket.SalePreparation().PrepareInvoice(); // Preparing the invoice will also add any taxes required
                return RenderInvoice(invoice);
            }

            // this should be handled differently
            throw new InvalidOperationException("Not ready to invoice");
        }

        /// <summary>
        /// Renders the InvoiceSummary Partial View
        /// </summary>
        /// <param name="invoice">The <see cref="IInvoice"/> to be displayed</param>
        private ActionResult RenderInvoice(IInvoice invoice)
        {
            return PartialView("InvoiceSummary", invoice);
        }
    }
}