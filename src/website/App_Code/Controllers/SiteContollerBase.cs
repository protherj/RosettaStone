using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Merchello.Core;
using Merchello.Core.Models;
using Merchello.Web;
using Merchello.Web.Models.ContentEditing;
using Merchello.Web.Workflow;
using Site.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Merchello.Core.Services;
using Merchello.Core.Gateways.Shipping;


namespace Site.Controllers
{
    public abstract class SiteContollerBase : SurfaceController
    {

        private IBasket _basket;

        private readonly IMerchelloContext _merchelloContext;
        
        protected SiteContollerBase(IMerchelloContext merchelloContext)
        {
            if (merchelloContext == null)
            {
                var ex = new ArgumentNullException("merchelloContext");
                LogHelper.Error<SiteContollerBase>("The MerchelloContext was null upon instantiating the CartController.", ex);
                throw ex;
            }

            _merchelloContext = merchelloContext;

            var customerContext = new CustomerContext(UmbracoContext);
            var currentCustomer = customerContext.CurrentCustomer;

            _basket = currentCustomer.Basket();
        }

        protected IBasket Basket
        {
            get { return _basket; }
        }

        protected IServiceContext Services
        {
            get { return _merchelloContext.Services; }
        }

        protected IShippingContext Shipping
        {
            get { return _merchelloContext.Gateways.Shipping; }
        }

    }
}