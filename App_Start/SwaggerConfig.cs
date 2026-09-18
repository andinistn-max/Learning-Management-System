using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using WebActivatorEx;
using Learning_Management_System;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Learning_Management_System
{
    public class SwaggerConfig
    {
        private static bool _isRegistered = false;

        public static void Register()
        {
            if (_isRegistered) return;
            _isRegistered = true;

            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "PUB Learning Hub REST API")
                        .Description("Dokumentasi Resmi OpenAPI / Swagger RESTful Web API PUB Learning Hub (LMS). Menyediakan layanan lengkap untuk modul Autentikasi (JWT), Manajemen Kelas, Materi Modul, Penugasan Siswa, Kuis Online, Presensi / Absensi, dan Rekapitulasi Nilai Terkalkulasi.")
                        .Contact(cc => cc.Name("PUB Learning Hub Developer").Email("support@publearninghub.id"));

                    // Skema Autentikasi Bearer JWT
                    c.ApiKey("Bearer")
                        .Description("Masukkan token JWT dengan format: Bearer {token_anda}")
                        .Name("Authorization")
                        .In("header");

                    c.OperationFilter<AssignOAuth2SecurityRequirements>();
                })
                .EnableSwaggerUi(c =>
                {
                    c.DocumentTitle("PUB Learning Hub - Swagger REST API Explorer");
                    c.EnableApiKeySupport("Authorization", "header");
                });
        }
    }

    public class AssignOAuth2SecurityRequirements : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.security == null)
            {
                operation.security = new List<IDictionary<string, IEnumerable<string>>>();
            }

            var oAuthRequirements = new Dictionary<string, IEnumerable<string>>
            {
                { "Bearer", new string[] { } }
            };

            operation.security.Add(oAuthRequirements);
        }
    }
}
