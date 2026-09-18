using System;
using System.Web.Mvc;

namespace Learning_Management_System.Controllers
{
    public class ErrorController : Controller
    {
        // GET: /Error/Forbidden (HTTP 403)
        [HttpGet]
        public ActionResult Forbidden(string returnUrl)
        {
            Response.StatusCode = 403;
            ViewBag.Title = "Akses Dibatasi - Perlu Hak Akses Khusus";
            ViewBag.IsPublicPage = true;
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.CurrentRole = Session["Role"]?.ToString() ?? "Pengguna";
            ViewBag.UserName = Session["NamaLengkap"]?.ToString() ?? "Pengguna";

            return View();
        }

        // GET: /Error/NotFound (HTTP 404)
        [HttpGet]
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            ViewBag.Title = "Halaman Tidak Ditemukan (404)";
            ViewBag.IsPublicPage = true;
            return View();
        }

        // GET: /Error/ServerError (HTTP 500)
        [HttpGet]
        public ActionResult ServerError()
        {
            Response.StatusCode = 500;
            ViewBag.Title = "Terjadi Kendala Sistem (500)";
            ViewBag.IsPublicPage = true;
            return View();
        }
    }
}
