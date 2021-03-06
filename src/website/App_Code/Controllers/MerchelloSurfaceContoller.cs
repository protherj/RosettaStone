﻿using System;
using Merchello.Core;
using Merchello.Web;
using Merchello.Web.Workflow;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Merchello.Core.Services;
using Merchello.Core.Gateways.Shipping;

namespace Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Something like this will likely be included in the Merchello Core soon
    /// </remarks>
    public abstract class MerchelloSurfaceContoller : SurfaceController
    {
        private readonly IBasket _basket;
        private readonly IMerchelloContext _merchelloContext;
        
        protected MerchelloSurfaceContoller(IMerchelloContext merchelloContext)
        {
            if (merchelloContext == null)
            {
                var ex = new ArgumentNullException("merchelloContext");
                LogHelper.Error<MerchelloSurfaceContoller>("The MerchelloContext was null upon instantiating the CartController.", ex);
                throw ex;
            }

            _merchelloContext = merchelloContext;

            var customerContext = new CustomerContext(UmbracoContext);
            var currentCustomer = customerContext.CurrentCustomer;

            _basket = currentCustomer.Basket();
        }

        /// <summary>
        /// Gets the Basket for the CurrentCustomer
        /// </summary>
        protected IBasket Basket
        {
            get { return _basket; }
        }

        /// <summary>
        /// We are going to hide the Umbraco Service Context here so controller that sub class this controller are "Merchello Surface Controllers"
        /// </summary>
        protected new IServiceContext Services
        {
            get { return _merchelloContext.Services; }
        }

        /// <summary>
        /// Exposes the Shipping Context
        /// </summary>
        protected IShippingContext Shipping
        {
            get { return _merchelloContext.Gateways.Shipping; }
        }

    }
}