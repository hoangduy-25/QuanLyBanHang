# HƯỚNG DẪN TÍCH HỢP BƯỚC 6 (ASP.NET MVC 5 CONTROLLERS & RAZOR VIEWS)

## 1. Giải nén & Include In Project
Giải nén toàn bộ `Controllers/`, `Views/`, và `Scripts/hcshop-api.js` vào `QuanLyBanHang/QuanLyBanHang/` (ghi đè `HomeController.cs` và `Views/Shared/_Layout.cshtml` mẫu ban đầu), sau đó **Include In Project**.

## 2. Kết nối tùy chọn với Frontend React (`hcshop-ui`)
Nếu bạn muốn chạy riêng giao diện React (`https://github.com/glowingnec/hcshop-ui`) gọi trực tiếp vào 71 API của Backend này:
1. Cài NuGet: `Install-Package Microsoft.AspNet.WebApi.Cors -Version 5.2.9`
2. Trong `App_Start/WebApiConfig.cs`, thêm:
```csharp
config.EnableCors(new System.Web.Http.Cors.EnableCorsAttribute("http://localhost:5173,https://glowingnec.github.io", "*", "*"));
```
