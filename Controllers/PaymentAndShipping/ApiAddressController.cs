using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Common;
using Nop.Plugin.Misc.RestAPI.Model;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using StackExchange.Profiling.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.RestAPI.Controllers.PaymentAndShipping
{
    public class ApiAddressController : ApiServicesCoreController
    {
        #region fields
        private readonly ICustomerService _customerService;
        #endregion

        #region ctor
        public ApiAddressController(ICustomerService customerService)
        {
            _customerService = customerService;
        }
        #endregion

        #region methods
        //get user addresses
        [HttpGet]
        public async Task<ActionResult> CustomerAddresses(ApiCustomerModel apiCustomerModel)
        {
            //o means null customer id
            if (apiCustomerModel.CustomerId == 0)
            {
                return BadRequest(new { error = "invalid customer address" });
            }
            //get customer if id is not null
            var customer = await _customerService.GetCustomerByIdAsync(apiCustomerModel.CustomerId);
            //if customer result is empty then no customer with that id exists
            if (customer == null)
            {
                return NotFound(new
                {
                    error = "customer not found"
                });
            }
            //if customer exists, check that its registered and active
            if (!await _customerService.IsRegisteredAsync(customer))
            {
                return BadRequest(new
                {
                    error = "user must be registered"
                });
            }
            //get customer address if we reached this far
            var address = await _customerService.GetAddressesByCustomerIdAsync(apiCustomerModel.CustomerId);
            if (address == null)
            {
                return Ok(new
                {
                    message = "please add an address"
                });
            }
            //adddress/es are found. return the list 
            return Ok(address);

        }

        //add user address
        [HttpPost]
        public async Task<ActionResult> AddCustomerAddress(ApiCustomerModel apiCustomerModel)
        {
            //0 means null customer id
            if (apiCustomerModel.CustomerId == 0)
            {
                return BadRequest(new { error = "invalid customer address" });
            }
            //get customer if id is not null
            var customer = await _customerService.GetCustomerByIdAsync(apiCustomerModel.CustomerId);
            //if customer result is empty then no customer with that id exists
            if (customer == null)
            {
                return NotFound(new
                {
                    error = "customer not found"
                });
            }
            //if customer exists, check that its registered and active
            if (!await _customerService.IsRegisteredAsync(customer))
            {
                return BadRequest(new
                {
                    error = "user must be registered"
                });
            }
            //check address model if empty
            if (apiCustomerModel.Address == null)
            {
                return BadRequest(new
                {
                    error = "address information must not be empty"
                });
            }
            //address model is not empty and customer is valid. Insert the address
            try
            {


                await _customerService.InsertCustomerAddressAsync(customer, apiCustomerModel.Address);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = ex.ToJson()
                });
            }

            //get all the customer addresses
            var existAddress = await _customerService.GetAddressesByCustomerIdAsync(apiCustomerModel.CustomerId);

            return Ok(existAddress);

        }

        

        #endregion
    }
}
