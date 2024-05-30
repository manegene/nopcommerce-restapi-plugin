using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.RestAPI.Model
{
    public class ApiOrderModel
    {
        public string Email { get; set; }
        public int BillingAddressId { get; set; }
        public int ShippingAddressId { get; set; }
        public int PickupAddressId { get; set; }
        public string PaymentMethodSysName { get; set; }
        public string CurrencyCode { get; set; }
        public string AttributesDescription { get; set; }
        public string AttributesXML { get; set; }
        public int DiscountId { get; set; } = 0;
    }
}
