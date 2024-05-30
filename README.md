# nopcommerce-restapi-plugin
Integrate your NopCommerce powered store with this API plugin.
The plugins is JWt secured aand requires no special configuration. Just upload and install and you can access your store through secure API endpoints. The base address is the your store web address. eg, https://localhost/restapi{/method_specific_route}. The method specific route will be indicated for each method in more details

## suppported methods groups
* User account
* Products and categories
* Store payment methods
* User address
* Cart and Wishlist
* Order

### User account. 
All methods use HTTP verb: POST
Controller: apiuser
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
   * Success response: Json body
     ``` JSON
     {
     "status": true,
     "message": "detailed_message"
     }
     ```
   * Failure response: Json body
     ``` JSON
     {
     "error":"error_message"
     }
     ``` 
2. Sign up
   * route: /signup
   * Required request type: Json body
     Nopommerce customer model. Varies depending on store settings
     ``` JSON
     {
     "password":"password_value",
     "customer":"nopcommerce_customer_model_values"
     }
     ```
   * Success response: Json body
     ``` JSON
     {
     "status":true,
     "validation":"email_validation_link",
     "username":"registered_customer_username"
     ```
   * Failed respose: Json body
     ```JSON
     {
     "error":"detailed_error_message"
     }
     ```
   
3. Login
   * route /signin
   * Required request type: Json body
   ``` JSON
   {
    "password":"user_password",
    "customer":{
        "username":"user@email"
    }
 }
 ```
 * success response: Json body

 ```JSON
 {
    "success": "true",
    "user": "user@email",
    "token": "token_to _be_used_to_authenticate_all_other_endpoints"
 }
 ```
 * Failure response: Json body
 ``` JSON
 {
    "error": "detailed_error_message"
 }
 ```
 * Example
 <img width="636" height="400" alt="auth" src="https://github.com/manegene/nopcommerce-restapi-plugin/assets/13959629/e110cd12-55e7-4d52-8585-e4a1de79b826">

### products and categories
All methods use HTTP ver: GET
Controller: apiproducts
1. AllProducts
   * Route: /apiproducts
   * 
