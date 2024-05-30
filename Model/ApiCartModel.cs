using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.RestAPI.Model
{
    public class ApiCartModel
    {
        public string Email {  get; set; }
        public int ProductId {  get; set; }
        public int Carttype {  get; set; }
        public string ProductAttibutes { get; set;}


    }
}
