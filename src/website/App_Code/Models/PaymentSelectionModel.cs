using System;

namespace Models
{
    /// <summary>
    /// Model to hold payment selection information acquired from the payment selection view
    /// </summary>
    public class PaymentSelectionModel
    {

        public Guid PaymentMethodKey { get; set; }

        public AddressModel Address { get; set; }


    }
}