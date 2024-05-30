using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.RestAPI.Model
{
    public class ApiCustomerModel
    {
        public int CustomerId { get; set; }
        public string Password { get; set; }
        public Address Address { get; set; }
        public Customer Customer { get; set; }

    }
}
