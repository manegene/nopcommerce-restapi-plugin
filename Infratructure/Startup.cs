using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.RestAPI.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.RestAPI.Infratructure
{
    public class Startup : INopStartup
    {
        //startup order
        public int Order => 200;

        public void Configure(IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var key = Encoding.ASCII.GetBytes("VGhpcyBpcyBhIGJhc2U2NCBlbmNvZGVkIHN0cmluZw==");

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true, // Ensure token expiration is validated
                    ClockSkew = TimeSpan.Zero // Optional: To make expiration check exact
                };
                x.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.NoResult();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync("{\"error\": \"invalid token\"}");
                    },
                    OnChallenge = context =>
                    {
                        //skip the default logic if the response has already been handled.

                        if (!context.Response.HasStarted)
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync("{\"error\": \"Unauthorized\"}");
                        }
                       return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync("{\"error\": \"Forbidden\"}");
                    },
                    OnMessageReceived = context =>
                    {
                        var endpoint = context.HttpContext.GetEndpoint();
                        var path = context.HttpContext.Request.Path;

                        

                        var allowAnonymous = endpoint?.Metadata?.GetMetadata<IAllowAnonymous>();
                        if (allowAnonymous == null && !path.StartsWithSegments("/restapi"))
                        {
                            return Task.CompletedTask; // Skip the authentication process for some site pages that may have authorize attribute
                        }

                        if (allowAnonymous != null)
                        {
                            return Task.CompletedTask; // Skip the authentication process for the endpoint that do not require authentication
                        }


                        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                        if (string.IsNullOrEmpty(token))
                        {

                            context.NoResult();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync("{\"error\": \"No token provided.\"}");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddControllers();
            services.AddSingleton<IApiCustomService, ApiCustomService>();

        }
    }
}
