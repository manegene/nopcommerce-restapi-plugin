using LinqToDB.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.IdentityModel.Tokens;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using NUglify.Helpers;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.RestAPI.Services
{
    public class ApiCustomService:IApiCustomService
    {
        #region  fields
        
        private readonly IRepository<Product> _productsRepository;
        private readonly IRepository<Order> _ordersRepository;
        private readonly IRepository<OrderItem> _orderItemsRepository;
        private const string Secret = "VGhpcyBpcyBhIGJhc2U2NCBlbmNvZGVkIHN0cmluZw==";

        #endregion
        #region ctor
        public ApiCustomService(IRepository<Product> productRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository) 
        {
            _productsRepository= productRepository;
            _ordersRepository= orderRepository;
            _orderItemsRepository= orderItemRepository;

        }
        #endregion

        #region methods

       public virtual async Task<object> GetOrderByCustomerId(int userid)
        {
            //incase no userid is passed, fail
            if (userid < 0)
            {
                return null;
            }
            // a user id is already passed and validated as valid customer
            var query = (from a in _productsRepository.Table
                         join b in _orderItemsRepository.Table on a.Id equals b.ProductId
                         join c in _ordersRepository.Table on b.OrderId equals c.Id
                         where c.CustomerId == userid
                         orderby c.CreatedOnUtc
                         select new
                         {
                             id = a.Id,
                             name = a.Name,
                             price = a.Price,
                             orderid = b.OrderId,
                             quantity = b.Quantity,
                             orderdate = c.CreatedOnUtc

                         });
            var order = await query.ToListAsync();

            return order;
            
        }
       public virtual async Task<bool> CheckModelIsEmptyAsync<T>(T model)
        {
            await Task.Delay(0);
            bool isEmpty=false;
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(model);


                // Check for null values
                if (string.IsNullOrEmpty(Convert.ToString(value)))
                {
                    // If the property is nullable or a string, continue checking other properties
                    if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                    {
                        isEmpty = false;
                        continue;
                    }



                    else
                    {
                        isEmpty = true;
                        break;

                    }

                }

                // Check for string fields and handle empty strings
                if (property.PropertyType == typeof(string) && !string.IsNullOrWhiteSpace((string)value))
                {
                    isEmpty = false;
                    continue;

                }


                // Check for integer fields and handle non-zero values
                if (property.PropertyType == typeof(int))
                    if ((int)value > 0)
                    {
                        isEmpty = false;
                        continue;

                    }
                    else if ((int)value <= 0)
                    {
                        isEmpty = true;
                        break;
                    }



                // Check for default values for other types
                if (value.Equals(property.PropertyType.GetDefaultValue()))
                {
                    isEmpty = false;
                    continue;

                }

                // If value is  null,  an empty string, and not a default value, field is  empty
            }

            // All properties are either null, empty strings, or have default values
            return isEmpty;
        }

        public virtual async Task<IPagedList<Product>> GetAllProducts(int pageno, int pagesize)
        {
            var products = await _productsRepository.Table.ToPagedListAsync(pagesize, pageno);
            return products;
        }

        public virtual async Task<string> GenerateToken( string username)
        {
            await Task.Delay(1000);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("name", username) }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token =  tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        #endregion
    }
}
