using System.Linq;
using Merchello.Core.Models;
using Merchello.Web.Workflow;

namespace Site.Models
{
    public static class BasketViewModelExtensions
    {
         public static BasketViewModel ToBasketViewModel(this IBasket basket)
         {
             return new BasketViewModel()
                 {
                     TotalPrice = basket.TotalBasketPrice,
                     Items = basket.Items.Select(item => item.ToBasketViewLineItem()).OrderBy(x => x.Name).ToArray()
                 };

         }

        /// <summary>
        /// Utility extension to map a <see cref="ILineItem"/> to a BasketViewLine item
        /// </summary>
        /// <param name="lineItem">The <see cref="ILineItem"/> to be mapped</param>
        /// <returns><see cref="BasketViewLineItem"/></returns>
        private static BasketViewLineItem ToBasketViewLineItem(this ILineItem lineItem)
        {
            return new BasketViewLineItem()
                {
                    Key = lineItem.Key,
                    ContentId = lineItem.ExtendedData.ContainsKey("umbracoContentId") ? int.Parse(lineItem.ExtendedData["umbracoContentId"]) : 0,
                    Name = lineItem.Name,
                    Sku = lineItem.Sku,
                    Price = lineItem.Price,
                    Quantity = lineItem.Quantity
                };

        }
    }
}