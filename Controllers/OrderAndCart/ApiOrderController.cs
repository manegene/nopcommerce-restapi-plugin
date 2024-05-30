using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Plugin.Misc.RestAPI.Model;
using Nop.Plugin.Misc.RestAPI.Services;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Factories;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.RestAPI.Controllers.OrderAndCart
{
    public class ApiOrderController : ApiServicesCoreController
    {
        #region fields
        private readonly ICustomerService _customerService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly IOrderService _orderService;
        private readonly ICurrencyService _currencyService;
        private readonly IPaymentService _paymentService;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly IWebHelper _webHelper;
        private readonly ICustomNumberFormatter _customNumberFormatter;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILocalizationService _localizationService;
        private readonly IApiCustomService _apiOrderService;
        private readonly IOrderProcessingService _orderProcessingService;
        #endregion

        #region ctor
        public ApiOrderController(ICustomerService customerService,
            IShoppingCartService shoppingCartService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IOrderModelFactory orderModelFactory,
            IStoreContext storeContext,
            IOrderService orderService,
            ICurrencyService currencyService,
            IPaymentService paymentService,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            IWebHelper webHelper,
            ICustomNumberFormatter customNumberFormatter,
            ICustomerActivityService customerActivityService,
            IEventPublisher eventPublisher,
            ILocalizationService localizationService,
            IApiCustomService apiOrderService,
            IOrderProcessingService orderProcessingService)
        {
            _customerService = customerService;
            _shoppingCartService = shoppingCartService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _orderModelFactory = orderModelFactory;
            _storeContext = storeContext;
            _orderService = orderService;
            _currencyService = currencyService;
            _paymentService = paymentService;
            _checkoutAttributeFormatter = checkoutAttributeFormatter;
            _webHelper = webHelper;
            _customNumberFormatter = customNumberFormatter;
            _customerActivityService = customerActivityService;
            _eventPublisher = eventPublisher;
            _localizationService = localizationService;
            _apiOrderService = apiOrderService;
            _orderProcessingService = orderProcessingService;
        }
        #endregion
        
        #region methods

        //get user orders
        [HttpGet]
        public async Task<ActionResult> GetOrderAsync(ApiCustomerModel apiCustomerModel) 
        {
            var email = apiCustomerModel.Customer.Email;
            if (string.IsNullOrEmpty(email))
            {
                return NotFound(new
                {
                    error = "empty email not allowed"
                });
            }
            var emailid= email.Trim();

            var customer = await _customerService.GetCustomerByEmailAsync(emailid);

            if ( customer == null)
            {
                return NotFound(new
                {
                    error="unknown user id"
                });
            }

            if (!await _customerService.IsRegisteredAsync(await _customerService.GetCustomerByEmailAsync(emailid)))
            {
                return BadRequest(new
                {
                    error = "user is not registered"
                });
            }

            var model = await _apiOrderService.GetOrderByCustomerId(customer.Id);

            return Ok(model);

        }

        //place order
        [HttpPost]
        public async Task<ActionResult> PlaceOrderAsync(ApiOrderModel apiOrderModel)
        {
            var customer = new Customer();

            if (await _apiOrderService.CheckModelIsEmptyAsync(apiOrderModel))
            {
                //all objects are empty or null or integer fields have 0 values
                return BadRequest(new
                {
                    error = "empty fields not allowed"
                });
            }

            if (await _customerService.GetCustomerByEmailAsync(apiOrderModel.Email)==null)
            {
                return NotFound(new
                {
                    error = "unknown user id"
                });
            }
            else
            {
                //get the customer by id
                 customer = await _customerService.GetCustomerByEmailAsync(apiOrderModel.Email);
            }

            if (!await _customerService.IsRegisteredAsync(customer))
            {
                return BadRequest(new
                {
                    error = "user is not registered"
                });
            }

            //get customer shoppng cart
            var cart= await _shoppingCartService.GetShoppingCartAsync(customer,ShoppingCartType.ShoppingCart);

            Debug.WriteLine(cart.ToString());
            if (cart.Count ==0)
            {
                return BadRequest(new
                {
                    error = "user cart is empty"
                });
            }
            var store =  _storeContext.GetCurrentStore();

            //prepare order request
            var ordeProcessPaymentRequest = new ProcessPaymentRequest
            {
                StoreId = store.Id,
                CustomerId = customer.Id,
                OrderGuid = Guid.NewGuid(),
                OrderGuidGeneratedOnUtc = DateTime.UtcNow,
                OrderTotal = Convert.ToDecimal(_orderTotalCalculationService.GetShoppingCartTotalAsync(cart, true).Result.shoppingCartTotal),
                PaymentMethodSystemName = apiOrderModel.PaymentMethodSysName,
            };
     
            var orderResult = await _orderProcessingService.PlaceOrderAsync(ordeProcessPaymentRequest);

            return Ok(orderResult);
        }
        #endregion
    }
}
