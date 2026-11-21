using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace QuanLyBanHang.Infrastructure.Handlers
{
    /// <summary>
    /// Message handler xử lý CORS và Preflight OPTIONS thuần .NET Framework 4.7.2
    /// (không cần cài thêm gói NuGet Microsoft.AspNet.WebApi.Cors).
    /// </summary>
    public class CorsMessageHandler : DelegatingHandler
    {
        private const string OriginHeader = "Origin";
        private const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        private const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
        private const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";
        private const string AccessControlAllowCredentials = "Access-Control-Allow-Credentials";
        private const string VaryHeader = "Vary";

        private static HashSet<string> GetAllowedOrigins()
        {
            var raw = ConfigurationManager.AppSettings["AllowedCorsOrigins"];
            if (string.IsNullOrWhiteSpace(raw))
            {
                raw = "http://localhost:5173,http://127.0.0.1:5173,https://localhost:44329";
            }

            return new HashSet<string>(
                raw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(o => o.Trim().TrimEnd('/')),
                StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsOriginAllowed(string origin)
        {
            if (string.IsNullOrWhiteSpace(origin))
                return false;

            var normalized = origin.Trim().TrimEnd('/');
            var allowed = GetAllowedOrigins();
            if (allowed.Contains("*") || allowed.Contains(normalized))
                return true;

            // Cho phép mọi cổng localhost / 127.0.0.1 trong môi trường phát triển
            Uri uri;
            if (Uri.TryCreate(normalized, UriKind.Absolute, out uri))
            {
                if (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AppendCorsHeaders(HttpResponseMessage response, string origin)
        {
            if (response == null || string.IsNullOrWhiteSpace(origin))
                return;

            response.Headers.Remove(AccessControlAllowOrigin);
            response.Headers.Add(AccessControlAllowOrigin, origin);
            response.Headers.Remove(AccessControlAllowMethods);
            response.Headers.Add(AccessControlAllowMethods, "GET, POST, PUT, PATCH, DELETE, OPTIONS");
            response.Headers.Remove(AccessControlAllowHeaders);
            response.Headers.Add(AccessControlAllowHeaders, "Content-Type, Authorization, Accept, X-Requested-With");
            response.Headers.Remove(AccessControlAllowCredentials);
            response.Headers.Add(AccessControlAllowCredentials, "true");
            if (!response.Headers.Vary.Contains(OriginHeader))
            {
                response.Headers.Add(VaryHeader, OriginHeader);
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string origin = null;
            IEnumerable<string> originValues;
            if (request.Headers.TryGetValues(OriginHeader, out originValues))
            {
                origin = originValues.FirstOrDefault();
            }

            bool isAllowed = IsOriginAllowed(origin);

            if (request.Method == HttpMethod.Options)
            {
                var preflightResponse = new HttpResponseMessage(HttpStatusCode.OK);
                if (isAllowed)
                {
                    AppendCorsHeaders(preflightResponse, origin);
                }
                return preflightResponse;
            }

            var response = await base.SendAsync(request, cancellationToken);
            if (isAllowed)
            {
                AppendCorsHeaders(response, origin);
            }
            return response;
        }
    }
}
