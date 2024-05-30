using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//root contoller for all api endpoints. implements generic controls
namespace Nop.Plugin.Misc.RestAPI.Controllers
{
    [Route("restapi/[controller]")]
    [ApiController]
    [Authorize]
    public class ApiServicesCoreController:ControllerBase
    {
    }
}
