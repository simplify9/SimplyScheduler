using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace SW.Scheduler.Web
{
    public static class AuthenticationMiddleware
    {
        private static (string userName, string password) GetCredentials(this string credentials)
        {
            var parts = credentials?.Split(':');
            return (parts?.ElementAtOrDefault(0), parts?.ElementAtOrDefault(1));
        }
        public static IApplicationBuilder UseAuthentication(this IApplicationBuilder applicationBuilder,
            SchedulerOptions schedulerOptions)
        {
            
            
            applicationBuilder.Use(async (context, next) =>
            {
                
                var request = context.Request;
                var header = request.Headers["Authorization"];

                if (!string.IsNullOrWhiteSpace(header))
                {
                    var authHeader = System.Net.Http.Headers.AuthenticationHeaderValue.Parse(header);

                    if ("Basic".Equals(authHeader.Scheme, StringComparison.OrdinalIgnoreCase))
                    {
                        var parameter = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter ?? ""));
                        var (suppliedUserName, suppliedPassword) = parameter.GetCredentials();
                        var (userName, password) = schedulerOptions.AdminCredentials.GetCredentials();
                        
                        if(suppliedUserName.Equals(userName, StringComparison.OrdinalIgnoreCase) && suppliedPassword == password)
                        {
                            await next();
                            return;
                        }
                    }
                }

                context.Response.StatusCode = 401;
                context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"App\"";
            });
            
            return applicationBuilder;
        }
        
        
    }
}