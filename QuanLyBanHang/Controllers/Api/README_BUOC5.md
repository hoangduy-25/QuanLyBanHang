# HƯỚNG DẪN TÍCH HỢP BƯỚC 5 (WEB API 2 - 71 ENDPOINTS)

## 1. Cài đặt NuGet Package ASP.NET Web API 2 (trong Package Manager Console)
```powershell
Install-Package Microsoft.AspNet.WebApi -Version 5.2.9
```

## 2. Giải nén & Include In Project
Giải nén `App_Start/WebApiConfig.cs`, `Infrastructure/`, và `Controllers/Api/` vào project `QuanLyBanHang/QuanLyBanHang/`, sau đó chuột phải -> **Include In Project**.

## 3. Đăng ký WebApiConfig trong Global.asax.cs
Thêm dòng `GlobalConfiguration.Configure(WebApiConfig.Register);` vào đầu `Application_Start()` (trước `RouteConfig.RegisterRoutes`):

```csharp
using System.Web.Http;
...
protected void Application_Start()
{
    AreaRegistration.RegisterAllAreas();
    GlobalConfiguration.Configure(WebApiConfig.Register);
    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
    RouteConfig.RegisterRoutes(RouteTable.Routes);
    BundleConfig.RegisterBundles(BundleTable.Bundles);
}
```

## 4. Thêm khóa bí mật JWT vào Web.config (`<appSettings>`)
```xml
<add key="JwtSecret" value="QuanLyBanHang_Secret_Key_2026_Min32Chars!" />
```
