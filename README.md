# nopcommerce-restapi-plugin
Integrate your NopCommerce powered store with this API plugin.
The plugins is JWt secured aand requires no special configuration. Just upload and install and you can access your store through secure API endpoints. The base address is the your store web address. eg, https://localhost/restapi{/method_specific_route}. The method specific route will be indicated for each method in more details

#### snapshot overview

## authorize to get a JWT 
 <img width="400" alt="auth" src="https://github.com/manegene/nopcommerce-restapi-plugin/assets/13959629/75dc5d5c-71c7-4e68-ba2d-a7f2e5e37e1e">

## use the JWT
 <img width="400" alt="use_bearertoken" src="https://github.com/manegene/nopcommerce-restapi-plugin/assets/13959629/fc579a91-e9e1-46b4-939a-d0083de7955c">

## wrong or expired JWT used
 <img width="400" alt="bad_token" src="https://github.com/manegene/nopcommerce-restapi-plugin/assets/13959629/40a33614-7e48-47e2-aff6-18ce82c21a28">


## suppported methods groups
* User account
* Products and categories
* Store payment methods
* User address
* Cart and Wishlist
* Order

### User account. 
* All methods:
  1. Use HTTP verb: POST
  2. Require AUTHORIZATIOn : Bearer Token
  3. Exemption: /signin route does not require authorozation. Its the endpoint to generate the bearer token with registered username and password
* Controller: apiuser
1. Reset password
   * route: /resetpassword
   * required request type: Json body
     ``` JSON
     {
     "customer":{
     "email": "customer_email_address"
     }
     }
     ```
   * Success response: Json 
     ``` JSON
     {
     "status": true,
     "message": "detailed_message"
     }
     ```
   * Failure response: Json 
     ``` JSON
     {
     "error":"error_message"
     }
     ``` 
2. Sign up
   * route: /signup
   * Required request type: Json body
     Nopommerce customer model. Varies depending on store customer settings
     ``` JSON
     {
     "password":"password_value",
     "customer":"nopcommerce_customer_model_values"
     }
     ```
   * Success response: Json 
     ``` JSON
     {
     "status":true,
     "validation":"email_validation_link",
     "username":"registered_customer_username"
     ```
   * Failed respose: Json 
     ```JSON
     {
     "error":"detailed_error_message"
     }
     ```
   
3. Login
   * route /signin
   * Required request type: Json 
   ``` JSON
   {
    "password":"user_password",
    "customer":{
        "username":"user@email"
    }
   }
   ```
   * success response: Json 

 ```JSON
 {
    "success": "true",
    "user": "user@email",
    "token": "token_to _be_used_to_authenticate_all_other_endpoints"
 }
 ```
 * Failure response: Json 
 ``` JSON
 {
    "error": "detailed_error_message"
 }
 ```
 * Example
 <img width="636" height="400" alt="auth" src="https://github.com/manegene/nopcommerce-restapi-plugin/assets/13959629/e110cd12-55e7-4d52-8585-e4a1de79b826">

### products and categories
* All methods:
  1. Use HTTP verb: GET
  2. Require AUTHORIZATION: Bearer Token
* Controller: apiproducts
1. AllProducts
   * Route: /
   * Require page information: Json body
     ``` JSON
     {
    "Pagenumber":INT_VALUE,
    "PageSize":INT_VALUE
     }
     ```
    * Response success: Json array with product information
    * Response failure: Json with error information

 2. Productbyid
    * Route: /{product_integer_id}
    * Require: productid_value
    * Response success: Json with product information
    * Response failure: Json with error information
  
  3. Prouctsmarkedasshowonhomepage
     * Route: /homeproducts
     * Response success: Json array
     * Response failure: Json with error information
  
  4. Productsmarkedasnew
     * Route: /newproducts
     * Response success: Json array
     * Response failure: Json with error information
     
  5. Productsinaspecificcategory
     * Route: /bycategory?categoryid={CATEGORY_INT_ID}
     * Require: categotyid parameter
     * Response success: Json array
     * Response failure: Json with error information

### Store payment methods
* All methods:
  1. Use HTTP verb: GET
  2. Require AUTHORIZATION: Bearer Token
* Controller: apipayment
  1. List all active payment options
     * Route: /
     * Response success: Json array
     * Response failure: Json with error information
  
### User address
* All methods:
  1. Use HTTP verb: GET to get user address
  2. Use HTTP verb: POST to add a new user address 
  3. Require AUTHORIZATION: Bearer Token
* Controller: apiaddress
  1. List all user addresses
     * Route: /
     * Response success: Json array
     * Response failure: Json with error information

  2. Add a new user address
     * Route: /
     * Require: Json body
       ``` JSON
       {
       "customer":
       {
       "email":"customer_email"
       }
       }
       ```
     * Response success: Json with the new address
     * Response failure: Json with error information

### Cart and Wishlist
* All methods:
  1. Use HTTP verb: GET to get user cart or wishlist products
  2. Use HTTP verb: POST to add a new product to user cart or wishlist 
  3. Require AUTHORIZATION: Bearer Token
  4. Cart types ids:
     * Shopping cart: 1
     * Wishlist: 2
     
* Controller: apicart
  1. List all user cart or wishlist
     * Route: /
     * Verb: GET
     * Require: Json body
       ``` JSON
       {
       "email":"user_email_address",
       "carttype: CART_TYPE_ID
       }
       ```
     * Response success: Json array
     * Response failure: Json with error information

  2. Add a new product to user cart or wishlist
     * Route: /
     * Verb: POST
     * Require: Json body
       ``` JSON
       {
       "email":"customer_email",
       "carttype":CART_TYPE_ID,
       "productid":product_id,
       "productattributes":"string_product_attributes"
       }
       
       ```
     * Response success: Json
     * Response failure: Json with error information

### Order
* All methods:
  1. Use HTTP verb: GET to get user ordered products
  2. Use HTTP verb: POST to place a new order 
  3. Require AUTHORIZATION: Bearer Token
      
* Controller: apiorder
  1. List all user ordered products
     * Route: /
     * Verb: GET
     * Require: Json body
       ``` JSON
       {
       "customer":
       {
       "email":"user_email_address",
       }
       }
       ```
     * Response success: Json array of customer orders
     * Response failure: Json with error information

  2. Place a new order
     * Route: /
     * Verb: POST
     * Require: Json body
       ``` JSON
       {
          "email":"user@email",
           "BillingAddressId":int,
           "ShippingAddressId":int,
           "PickupAddressId":id,
           "PaymentMethodSysName":"PaymentMethodSystemName",
           "CurrencyCode":"currecny_code",
           "AttributesDescription":"string_value",
           "AttributesXML":"string_value",
           "DiscountId":int
       }
       ```
     * Response success: Json with order information
     * Response failure: Json with error information
  
