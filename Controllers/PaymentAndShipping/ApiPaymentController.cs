using Microsoft.AspNetCore.Mvc;
using Nop.Services.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.RestAPI.Controllers.PaymentAndShipping
{
    public class ApiPaymentController:ApiServicesCoreController
    {
        #region fields
        private readonly IPaymentPluginManager _pluginManager;
        #endregion

        #region ctor
        public ApiPaymentController(
            IPaymentPluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }
        #endregion

        #region methods
        [HttpGet]
        public async Task<ActionResult> GetPaymentMethods()
        {
            var methods = await _pluginManager.LoadActivePluginsAsync();

            return Ok(methods);


        }
        #endregion
    }
}
