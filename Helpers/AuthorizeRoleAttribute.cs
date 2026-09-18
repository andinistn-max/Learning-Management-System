using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Learning_Management_System.Services.Context;

namespace Learning_Management_System.Helpers
{
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        private readonly string[] allowedRoles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            this.allowedRoles = roles ?? new string[0];
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null) return false;

            // 1. Auto-Rehydrate Session jika sesi terhapus (misal akibat AppDomain restart/build) namun Cookie Auth aktif
            if (httpContext.Session != null && (httpContext.Session["Role"] == null || httpContext.Session["UserId"] == null))
            {
                if (httpContext.User != null && httpContext.User.Identity != null && httpContext.User.Identity.IsAuthenticated)
                {
                    string userEmail = httpContext.User.Identity.Name;
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        try
                        {
                            using (var db = new LmsDbContext())
                            {
                                var user = db.Users
                                    .Include(u => u.Role)
                                    .Include(u => u.Profile)
                                    .FirstOrDefault(u => u.Email.ToLower() == userEmail.ToLower() && u.IsActive);

                                if (user != null)
                                {
                                    string roleName = user.Role != null ? user.Role.NamaRole : "Siswa";
                                    httpContext.Session["UserId"] = user.IdUser;
                                    httpContext.Session["NamaLengkap"] = user.NamaLengkap;
                                    httpContext.Session["Email"] = user.Email;
                                    httpContext.Session["Role"] = roleName;
                                    httpContext.Session["RoleName"] = roleName;
                                    httpContext.Session["FotoProfile"] = user.FotoProfile;
                                }
                            }
                        }
                        catch
                        {
                            // Abaikan jika database sedang sibuk
                        }
                    }
                }
            }

            var role = httpContext.Session?["Role"]?.ToString();
            if (string.IsNullOrWhiteSpace(role)) return false;

            // 2. Evaluasi kecocokan role (case-insensitive & support alias)
            foreach (var r in allowedRoles)
            {
                if (string.Equals(role, r, StringComparison.OrdinalIgnoreCase)) return true;

                // Alias Admin & Administrator
                if ((string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) || string.Equals(r, "Administrator", StringComparison.OrdinalIgnoreCase)) &&
                    (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Administrator", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                // Alias Guru & Mentor
                if ((string.Equals(r, "Guru", StringComparison.OrdinalIgnoreCase) || string.Equals(r, "Mentor", StringComparison.OrdinalIgnoreCase)) &&
                    (string.Equals(role, "Guru", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Mentor", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                // Alias Siswa & Student
                if ((string.Equals(r, "Siswa", StringComparison.OrdinalIgnoreCase) || string.Equals(r, "Student", StringComparison.OrdinalIgnoreCase)) &&
                    (string.Equals(role, "Siswa", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext == null) return;

            string returnUrl = filterContext.HttpContext?.Request?.RawUrl;

            // Belum login sama sekali -> Arahkan ke Login
            if (filterContext.HttpContext?.Session?["UserId"] == null)
            {
                var routeValues = new RouteValueDictionary(new { controller = "Account", action = "Login" });
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    routeValues["returnUrl"] = returnUrl;
                }
                filterContext.Result = new RedirectToRouteResult(routeValues);
            }
            else
            {
                // Sudah login tapi role tidak sesuai -> Arahkan ke Forbidden dengan informasi returnUrl
                var routeValues = new RouteValueDictionary(new { controller = "Error", action = "Forbidden" });
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    routeValues["returnUrl"] = returnUrl;
                }
                filterContext.Result = new RedirectToRouteResult(routeValues);
            }
        }
    }
}