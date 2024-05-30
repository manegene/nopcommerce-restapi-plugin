using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.RestAPI.Model;
using Nop.Plugin.Misc.RestAPI.Services;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.RestAPI.Controllers.Products
{
    public class ApiProductsController:ApiServicesCoreController
    {
        #region fields
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IApiCustomService _apiOrderService;
        #endregion
        #region ctor
        public ApiProductsController( IProductService productService, ICategoryService categoryService, IApiCustomService apiOrderService) 
        { 
            _productService = productService;
            _categoryService = categoryService;
            _apiOrderService = apiOrderService;

        }
        #endregion

        #region methods

        //get all products
        [HttpGet]
        public async Task<ActionResult> GetAllProducts(ApiProductsPageList page)
        {
            int pagenumber = page.Pagenumber;
            int pagesize= page.PageSize;
            var products= await _apiOrderService.GetAllProducts( pagenumber, pagesize );

            return Ok( products );
        }

        //load product by id
        [HttpGet("{productid}")]
        public async Task<ActionResult> GetProductById(int productid)
        {
            if (productid <= 0)
            {
                return BadRequest(new {
                    error="invalid product id"
                });
            }

            //a valid product id exist
             return Ok( await _productService.GetProductByIdAsync(productid));
        }

        //load all products shown on homepage
        [HttpGet]
        [Route("homeproducts")]
        public async Task<ActionResult> GetHomeProducts()
        {
            return Ok( await _productService.GetAllProductsDisplayedOnHomepageAsync());
        }

        //load all products marked as new
        [HttpGet]
        [Route("newproducts")]
        public async Task<ActionResult> GetNewProducts()
        {
            return Ok(await _productService.GetProductsMarkedAsNewAsync());
        }

        //get products by category
        [HttpGet]
        [Route("bycategory")]
        public async Task<ActionResult> GetProductsbyCategory(int categoryId)
        {
            var category= await _categoryService.GetCategoryByIdAsync(categoryId);

            //category not fond hence cannot proceed to find any products for passed ID
            if (category == null)
            {
                return BadRequest(new
                {
                    error = "category not found"
                });
            }

            var products= await _productService.GetCategoryFeaturedProductsAsync(categoryId);
            return Ok(products);

        }

        #endregion
    }
}
