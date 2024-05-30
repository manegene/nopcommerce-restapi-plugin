using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.RestAPI.Services
{
    public interface IApiCustomService
    {
        //protected Task PlaceOrderAsync(Order order);
        Task<bool> CheckModelIsEmptyAsync<T>(T model);
        Task<object> GetOrderByCustomerId(int userid);

        Task<IPagedList<Product>> GetAllProducts(int pageno, int pagesize);

        Task<string> GenerateToken(string username);
    }
}
