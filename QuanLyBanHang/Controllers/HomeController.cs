using System.IO;
using System.Text;
using System.Web.Mvc;

namespace QuanLyBanHang.Controllers
{
    /// <summary>
    /// Điểm vào phục vụ bản build React SPA (~/ClientApp/index.html) khi chạy gộp 1 cổng trên IIS Express.
    /// Khi đang phát triển, chạy React qua Vite dev server (http://localhost:5173) với proxy tới API này.
    /// </summary>
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var spaEntry = Server.MapPath("~/ClientApp/index.html");
            if (!string.IsNullOrEmpty(spaEntry) && System.IO.File.Exists(spaEntry))
            {
                return File(spaEntry, "text/html");
            }

            var html = @"<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""utf-8"" />
  <title>HCShop Backend API</title>
  <style>
    body { font-family: system-ui, -apple-system, sans-serif; background: #f8f9fa; color: #18181b; display: flex; align-items: center; justify-content: center; min-height: 100vh; margin: 0; padding: 24px; }
    .card { max-width: 560px; background: #fff; border: 1px solid #e4e4e7; border-radius: 16px; padding: 28px 32px; box-shadow: 0 4px 20px rgba(0,0,0,0.04); }
    h1 { margin: 0 0 8px; font-size: 20px; }
    p { color: #52525b; font-size: 14px; line-height: 1.6; }
    code, pre { background: #f4f4f5; border-radius: 8px; font-family: Consolas, monospace; font-size: 13px; }
    code { padding: 2px 6px; }
    pre { padding: 12px 14px; overflow-x: auto; }
    a { color: #2563eb; text-decoration: none; font-weight: 600; }
  </style>
</head>
<body>
  <div class=""card"">
    <h1>HCShop Web API đang hoạt động</h1>
    <p>Backend ASP.NET Web API 2 (<code>/api/*</code>) đã sẵn sàng. Chưa tìm thấy bản build React tại <code>QuanLyBanHang/ClientApp/index.html</code>.</p>
    <p><strong>Chế độ phát triển (Dev Mode):</strong> chạy Vite và mở <a href=""http://localhost:5173"">http://localhost:5173</a></p>
    <pre>cd client
npm install
npm run dev</pre>
    <p><strong>Chế độ gộp 1 cổng (Single-Port Mode):</strong> đóng gói React vào ASP.NET rồi tải lại trang này:</p>
    <pre>cd client
npm run build:aspnet</pre>
  </div>
</body>
</html>";

            return Content(html, "text/html", Encoding.UTF8);
        }
    }
}
