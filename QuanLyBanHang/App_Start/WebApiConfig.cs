using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QuanLyBanHang.Infrastructure.Filters;
using QuanLyBanHang.Infrastructure.Handlers;

namespace QuanLyBanHang
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Hỗ trợ CORS & Preflight OPTIONS cho React SPA (Vite dev server :5173)
            config.MessageHandlers.Add(new CorsMessageHandler());

            // Bật Attribute Routing ([RoutePrefix], [Route]) cho hệ thống RESTful API
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Đăng ký bộ lọc ValidateModel & Global Exception Handling toàn cục
            config.Filters.Add(new ValidateModelAttribute());
            config.Filters.Add(new GlobalExceptionFilterAttribute());

            // Trả về JSON thuần (camelCase, bỏ XML formatter để tránh lỗi serialize)
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            var jsonSettings = config.Formatters.JsonFormatter.SerializerSettings;
            jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
        }
    }
}
