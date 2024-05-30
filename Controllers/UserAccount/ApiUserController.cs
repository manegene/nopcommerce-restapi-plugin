using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.RestAPI.Model;
using Nop.Plugin.Misc.RestAPI.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using StackExchange.Profiling.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.RestAPI.Controllers.UserAccount
{
    public class ApiUserController:ApiServicesCoreController
    {
        #region  fields
        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWorkContext _workContext;
        private readonly IApiCustomService _apiOrderService;

        #endregion

        #region ctor
        public ApiUserController(ICustomerService customerService, 
            IStoreContext storeContext, 
            ICustomerRegistrationService customerRegistrationService,
            CustomerSettings customerSettings,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IGenericAttributeService genericAttributeService,
            IWorkflowMessageService workflowMessageService,
            IWorkContext workContext,
            IApiCustomService apiOrderService) 
        { 
            _customerService = customerService;
            _storeContext = storeContext;
            _customerRegistrationService = customerRegistrationService;
            _customerSettings = customerSettings;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _genericAttributeService = genericAttributeService;
            _workflowMessageService = workflowMessageService;
            _workContext = workContext;
            _apiOrderService = apiOrderService;
        }
        #endregion

        #region methods
        //register new user
        [HttpPost]
        [Route("signup")]
        public async Task<ActionResult> Signup(ApiCustomerModel customer)
        {
            //first check customer object is not null
            if (customer == null)
            {
                return BadRequest(new
                {
                    error = "not allowed"
                });
            }

            //validate email is valid
            if(!CommonHelper.IsValidEmail(customer.Customer.Email))
            {
                return BadRequest(new
                {
                    error="invalid email"
                });
            }

            //check if customer email is already registered
            if (!(string.IsNullOrEmpty(customer.Customer.Email)) && _customerService.GetCustomerByEmailAsync(customer.Customer.Email).Result.Email == customer.Customer.Email)
            {
                
                return BadRequest(new
                {
                    error=$"{customer.Customer.Email} already exists"
                });
            }
            //fill entity from model
            var currentStore = await _storeContext.GetCurrentStoreAsync();

            customer.Customer.CustomerGuid = Guid.NewGuid();
            customer.Customer.CreatedOnUtc = DateTime.UtcNow;
            customer.Customer.LastActivityDateUtc = DateTime.UtcNow;
            customer.Customer.RegisteredInStoreId = currentStore.Id;
            customer.Customer.Username = customer.Customer.Email;
            customer.Customer.Active=true;     


            //insert the user to the db
            await _customerService.InsertCustomerAsync(customer.Customer);

            //password
            if (!string.IsNullOrWhiteSpace(customer.Password))
            {
                var changePassRequest = new ChangePasswordRequest(customer.Customer.Email, false, _customerSettings.DefaultPasswordFormat, customer.Password);
                var changePassResult = await _customerRegistrationService.ChangePasswordAsync(changePassRequest);
                if (!changePassResult.Success)
                {
                    return BadRequest(new
                    {
                        error = $"password not set :{changePassResult.Errors}"
                    });
                }
            }

            //update customer role mapping
            var role =  _customerService.GetAllCustomerRolesAsync().Result.Where(role=>role.Name== "Registered").FirstOrDefault();
            if (role != null)
            {
                await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Customer.Id, CustomerRoleId = role.Id });
            }
             await _customerService.UpdateCustomerAsync(customer.Customer);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewCustomer",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCustomer"), customer.Customer.Id), customer.Customer);

            //if we got this far, it all green :)
            return Ok(new
            {
                success=$"customer {customer.Customer.Email} registered successfully with email"
            });

        }

        //login a registered user
        [HttpPost]
        [Route("signin")]
        [AllowAnonymous]
        public async Task<ActionResult> Signin(ApiCustomerModel model)
        {
           
            //verify user fields are not empty
            if (string.IsNullOrEmpty(model.Password) ||model.Customer==null)
            {
                return BadRequest(new
                {
                    error="username or password cannot be empty"
                });
            }

            var email = model.Customer.Username.Trim();
            var customer = await _customerService.GetCustomerByUsernameAsync(email);

            //check if user is registerd with provided email
            if (!_customerService.IsRegisteredAsync(customer).Result)
            {
                return BadRequest(new
                {
                    error = $"{email} is not registered"
                });
            }

            //validate the username and password combination

            var userCombination = await _customerRegistrationService.ValidateCustomerAsync(email, model.Password);
            if(userCombination != CustomerLoginResults.Successful)
            {
                return BadRequest(new
                {
                    error = "wrong credentials"
                });
            }


            //retun user email if login succeeds
            return Ok(new
            {
                success = "true",
                user = $"{email}",
                token=await _apiOrderService.GenerateToken(email)
            });
        }

        //forgot password
        [HttpPost]
        [Route("resetpassword")]
        public async Task<ActionResult> ResetPassword(ApiCustomerModel model)
        {
            var email = model.Customer.Username.Trim();
            var customer = model.Customer;

            //verify user fields are not empty
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new
                {
                    error = "username cannot be empty"
                });
            }

            //check if user is registerd with provided email
            if (!_customerService.IsRegisteredAsync(customer).Result)
            {
                return BadRequest(new
                {
                    error = $"{email} is not registered"
                });
            }

            //get customer by the provied email
             customer = await _customerService.GetCustomerByEmailAsync(email);
           
            if (customer is null || !customer.Active || customer.Deleted)
            {
                return BadRequest(new
                {
                    error = "customer state not allowed to reset password"
                });
            }

               //save token and current date
                var passwordRecoveryToken = Guid.NewGuid();
                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute,
                    passwordRecoveryToken.ToString());
                DateTime? generatedDateTime = DateTime.UtcNow;
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.PasswordRecoveryTokenDateGeneratedAttribute, generatedDateTime);

                //send email
                await _workflowMessageService.SendCustomerPasswordRecoveryMessageAsync(customer,
                    (await _workContext.GetWorkingLanguageAsync()).Id);

            return Ok(new
            {
                success=true,
                message=$"email sent to {customer.Email}"
            });

        }
        #endregion


    }
}
