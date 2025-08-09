
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Services.Helper.CustomExceptions;
using System.Net;
using System.Security.Claims;

namespace IntelliPM.API.Middlewares
{
    public class AuthorizeMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorizeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IAccountRepository accountRepository)
        {
            try
            {
                var requestPath = context.Request.Path;

                if (requestPath.StartsWithSegments("/api/auth"))
                {
                    await _next.Invoke(context);
                    return;
                }

                if (context.User.Identity is not ClaimsIdentity accIdentity || !accIdentity.IsAuthenticated)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Unauthorized access");
                    return;
                }

                var accountIdClaim = accIdentity.FindFirst("id");
                if (accountIdClaim == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Invalid token");
                    return;
                }

                var account = await accountRepository.GetAccountById(int.Parse(accountIdClaim.Value));
                if (account == null)
                {
                    throw new ApiException(HttpStatusCode.NotFound, "Account not found");
                }

                if (account.Status.Equals("UNVERIFIED"))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Account is not verified");
                    return;
                }

                await _next.Invoke(context);
            }
            catch (ApiException apiEx)
            {
                context.Response.StatusCode = (int)apiEx.StatusCode;
                await context.Response.WriteAsync(apiEx.Message);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync("Internal Server Error");
            }
        }
    }
}
