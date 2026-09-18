using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Learning_Management_System.Helpers
{
    public class JwtAuthorizeAttribute : AuthorizationFilterAttribute
    {
        private readonly string[] allowedRoles;

        public JwtAuthorizeAttribute(params string[] roles)
        {
            this.allowedRoles = roles ?? new string[0];
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext == null) throw new ArgumentNullException(nameof(actionContext));

            // Periksa jika action atau controller mengizinkan anonymous
            if (actionContext.ActionDescriptor.GetCustomAttributes<System.Web.Http.AllowAnonymousAttribute>().Any() ||
                actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<System.Web.Http.AllowAnonymousAttribute>().Any())
            {
                return;
            }

            var authHeader = actionContext.Request.Headers.Authorization;
            if (authHeader == null || string.IsNullOrWhiteSpace(authHeader.Parameter) || !string.Equals(authHeader.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase))
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.Unauthorized,
                    new { success = false, message = "Akses ditolak. Token otorisasi JWT (Bearer) tidak ditemukan." }
                );
                return;
            }

            var principal = JwtHelper.ValidateToken(authHeader.Parameter);
            if (principal == null)
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.Unauthorized,
                    new { success = false, message = "Token JWT kedaluwarsa atau tidak valid." }
                );
                return;
            }

            // Validasi Role jika ditentukan
            if (allowedRoles.Length > 0)
            {
                var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value ?? principal.FindFirst("role")?.Value;
                if (string.IsNullOrEmpty(roleClaim) || !allowedRoles.Any(r => string.Equals(r, roleClaim, StringComparison.OrdinalIgnoreCase)))
                {
                    actionContext.Response = actionContext.Request.CreateResponse(
                        HttpStatusCode.Forbidden,
                        new { success = false, message = "Akses terlarang. Peran akun Anda tidak memiliki hak akses ke endpoint ini." }
                    );
                    return;
                }
            }

            // Set context principal
            Thread.CurrentPrincipal = principal;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }

            base.OnAuthorization(actionContext);
        }
    }
}
