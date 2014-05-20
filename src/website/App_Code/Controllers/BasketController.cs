using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Merchello.Core;
using Merchello.Core.Models;
using Merchello.Web;
using Merchello.Web.Models.ContentEditing;
using Merchello.Web.Workflow;

using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;

using Site.Models;

namespace Site.Controllers
{
    [PluginController("Site")]
    public class BasketController : SurfaceController
    {
        private readonly IBasket _basket;

        private readonly IMerchelloContext _merchelloContext;

        // These would normally be passed in or looked up so that there is not a 
        // hard coded reference

        // TODO
        private const int BasketContentId = 1093;
        private const int PaymentContentId = 1071;

        public BasketController()
             : this(MerchelloContext.Current)
         {}

        public BasketController(IMerchelloContext merchelloContext)
        {
            if (merchelloContext == null)
            {
                var ex = new ArgumentNullException("merchelloContext");
                LogHelper.Error<BasketController>("The MerchelloContext was null upon instantiating the CartController.", ex);
                throw ex;
            }

            _merchelloContext = merchelloContext;

            var customerContext = new CustomerContext(UmbracoContext);
            var currentCustomer = customerContext.CurrentCustomer;

            _basket = currentCustomer.Basket();
        }


        /// <summary>
        /// Renders the basket on the page
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult RenderBasket()
        {
            // ToBasketViewModel is an extension that
            // translates the IBasket to a local view model which
            // can be submitted via a form.
            return PartialView("merchBasket", _basket.ToBasketViewModel());
        }

        /// <summary>
        /// Renders the Add To Cart form which is a partial view on the Product Detail view
        /// </summary>
        /// <param name="product">
        /// 
        /// ProductDisplay (anything with 'Display') are Merchello.Web objects that are usually simplified
        /// versions of Core objects - basic POCOs and are primarily used for Back Office API calls.  We've
        /// repurposed the ProductDisplay, ProductVariantDisplay to rendering products in content quickly.
        /// 
        /// </param>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult RenderAddToCart(ProductDisplay product)
        {
            var contentId = UmbracoContext.PageId != null ? UmbracoContext.PageId.Value : 0;

            var model = new AddItemModel()
                {
                    ProductKey = product.Key,
                    ContentId = contentId
                };

            return PartialView("merchAddToCart", model);
        }

        /// <summary>
        /// Responsible updating the quantities of items in the basket
        /// </summary>
        /// <param name="model"><see cref="BasketViewModel"/></param>
        /// <returns>Redirects to the current Umbraco page (the basket page)</returns>
        [HttpPost]
        public ActionResult UpdateBasket(BasketViewModel model)
        {
            if (ModelState.IsValid)
            {
                // The only thing that can be updated in this basket is the quantity
                foreach (var item in model.Items)
                {
                    if(_basket.Items.First(x => x.Key == item.Key).Quantity != item.Quantity) 
                        _basket.UpdateQuantity(item.Key, item.Quantity);
                }

                // * Tidbit - Everytime "Save()" is called on the Basket, a new VersionKey (Guid) is generated.
                // *** This is used to validate the CheckoutPreparationBase (BasketCheckoutPrepartion in this case),
                // *** asserting that any previously saved information (rate quotes, shipments ...) correspond to the Basket version.
                // *** If the versions do not match, the CheckoutPreparation 
                _basket.Save();
            }
            return RedirectToUmbracoPage(BasketContentId);
        }

        [HttpPost]
        public ActionResult AddToBasket(AddItemModel model)
        {
            // This is an example of using the ExtendedDataCollection to add some custom functionality.
            // In this case, we are creating a direct reference to the content (Product Detail Page) so
            // that we can provide a link, thumbnail and description in the cart per this design.  In other 
            // designs, there may not be thumbnails or descriptions and the link could be to a completely
            // different website.
            var extendedData = new ExtendedDataCollection();
            extendedData.SetValue("umbracoContentId", model.ContentId.ToString(CultureInfo.InvariantCulture));
            
            var product = _merchelloContext.Services.ProductService.GetByKey(model.ProductKey);

            // In the event the product has options we want to add the "variant" to the basket.
            // -- If a product that has variants is defined, the FIRST variant will be added to the cart. 
            // -- This was done so that we did not have to throw an error since the Master variant is no
            // -- longer valid for sale.
            if (model.OptionChoices != null && model.OptionChoices.Any())
            {
                
                var variant = _merchelloContext.Services.ProductVariantService.GetProductVariantWithAttributes(product, model.OptionChoices);
                
                // TODO : This is an error in the back office ... name should already include the variant info
                // Begin fix -------------------------------------------------------------------------------------------
                // We need to save the variant name (T-Shirt - blue, large) instead of (T-Shirt).  This is done in the
                // ProductVariantService CreateProductVariantWithKey ... but obviously we're not using that method
                // and must be doing it himself in the Back Office before he sends it to the server - likely just 
                // calling save.  http://issues.merchello.com/youtrack/issue/M-153
                var name = product.Name;
                foreach (var att in variant.Attributes)
                {
                    if (name == product.Name) name += " - ";
                    else
                        name += ", ";
                    name += " " + att.Name;
                }
                // end fix ----------------------------------------------------------------------------------------------
                
                _basket.AddItem(variant, name, 1, extendedData);
            }
            else
            {
         
                _basket.AddItem(product, product.Name, 1, extendedData);
            }
            _basket.Save();

            // redirect to the cart page - this is a quick and dirty and should be done differently in
            // practice
            return RedirectToUmbracoPage(BasketContentId);
        }


        /// <summary>
        /// Removes an item from thet basket
        /// </summary>        
        [HttpGet]
        public ActionResult RemoveItemFromBasket(Guid lineItemKey)
        {
            if (_basket.Items.FirstOrDefault(x => x.Key == lineItemKey) == null)
            {
                var exception =
                    new InvalidOperationException(
                        "Attempt to delete an item from a basket that does not match the CurrentUser");
                LogHelper.Error<BasketController>("RemoveItemFromBasket failed.", exception);

                throw exception;
            }

            // remove the item by it's pk.  
            _basket.RemoveItem(lineItemKey);
            _basket.Save();
            
            return RedirectToUmbracoPage(BasketContentId);
        }

    }
}