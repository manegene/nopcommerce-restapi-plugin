using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.RestAPI.Model;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.RestAPI.Controllers.OrderAndCart
{
    public class ApiCartController:ApiServicesCoreController
    {
        #region fields
        private readonly ICustomerService _customerService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        #endregion

        #region ctor
        public ApiCartController(ICustomerService customerService, 
            IShoppingCartService shoppingCartService,
            IProductService productService,
            IStoreContext storeContext)
        {
            _customerService = customerService;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _storeContext = storeContext;
        }
        #endregion

        #region functions
        private async Task<ActionResult<Customer>> CheckUser(string email)
        {
            

            //check email is not empty
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new
                {
                    error = "user email cannot be empty"
                });
            }

            //remove trailing white spaces
            var Email = email.Trim();

            //check if user exist
            var user = await _customerService.GetCustomerByEmailAsync(Email);
            if (user == null)
            {
                return NotFound(new
                {
                    error = "user is unkown to us"
                });
            }

            return user;
        }
        #endregion

        #region methods
        //get user cart/wishlist items
        [HttpGet]
        public async Task<ActionResult> Getcart(ApiCartModel apiCartModel)
        {
            var chekuser = CheckUser(apiCartModel.Email);

            if (chekuser == null)
            {
                return BadRequest(new
                {
                    error = chekuser.Result.ToString()
                });
            }

            var user = chekuser.Result.Value;

            //user is valid and user exist
            var cart = await _shoppingCartService.GetShoppingCartAsync(user, (ShoppingCartType?)apiCartModel.Carttype);

            return Ok(cart);
        }

        //add carrt/wishlist items
        [HttpPost]
        public async Task<ActionResult> Addcart(ApiCartModel apiCartModel)
        {
            if(apiCartModel.ProductId.Equals(0))
            {
                return BadRequest(new
                {
                    error="product id cannot be empty"
                });
            }
            var product = await _productService.GetProductByIdAsync(apiCartModel.ProductId);

            if (product == null)
            {
                return NotFound(new
                {
                    error = "product not found"
                });
            }

            var chekuser = CheckUser(apiCartModel.Email);

            if (chekuser == null)
            {
                return BadRequest(new
                {
                    error = chekuser.Result.ToString()
                });
            }
            
            var store = await _storeContext.GetCurrentStoreAsync();

            var user = chekuser.Result.Value;
            var add = await _shoppingCartService.AddToCartAsync(user, product, (ShoppingCartType)apiCartModel.Carttype, store.Id, apiCartModel.ProductAttibutes);

            return Ok(add);
        }
        #endregion

    }
}
