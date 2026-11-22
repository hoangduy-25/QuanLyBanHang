using QuanLyBanHang.Data;
using QuanLyBanHang.Infrastructure.Auth;
using QuanLyBanHang.Models.DTOs.Common;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace QuanLyBanHang.Infrastructure.Filters
{
    /// <summary>
    /// Xác thực JWT Bearer Token và kiểm tra phân quyền theo VaiTro (KhachHang, NhanVien, QuanLy).
    /// Đặt Optional = true cho các API công khai nhưng có hành vi khác nhau nếu người dùng đã đăng nhập (vd: Quản lý thấy sản phẩm ẩn).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class JwtAuthorizeAttribute : AuthorizationFilterAttribute
    {
        public string Roles { get; set; }
        public bool Optional { get; set; }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var authHeader = actionContext.Request.Headers.Authorization;
            string token = (authHeader != null && string.Equals(authHeader.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase))
                ? authHeader.Parameter
                : null;

            var principal = JwtTokenHelper.ValidateToken(token);
            if (principal != null)
            {
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var tokenStamp = principal.FindFirst("pst")?.Value;
                if (int.TryParse(userIdClaim, out var userId))
                {
                    using (var db = new QuanLyBanHangDbContext())
                    {
                        var userState = db.TaiKhoans
                            .Where(t => t.Id == userId)
                            .Select(t => new { t.TrangThai, t.MatKhauHash })
                            .FirstOrDefault();

                        var currentStamp = (userState?.MatKhauHash ?? string.Empty).Split('.').LastOrDefault();
                        if (userState == null || userState.TrangThai == "BiKhoa" ||
                            (!string.IsNullOrEmpty(tokenStamp) && tokenStamp != currentStamp))
                        {
                            principal = null;
                        }
                    }
                }
            }

            if (principal != null)
            {
                actionContext.RequestContext.Principal = principal;
            }

            if (Optional)
                return;

            if (principal == null)
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.Unauthorized,
                    ApiResponse<object>.Fail("Vui lòng đăng nhập để tiếp tục."));
                return;
            }

            if (!string.IsNullOrWhiteSpace(Roles))
            {
                var allowedRoles = Roles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim())
                    .ToList();

                if (!allowedRoles.Any(r => principal.IsInRole(r)))
                {
                    actionContext.Response = actionContext.Request.CreateResponse(
                        HttpStatusCode.Forbidden,
                        ApiResponse<object>.Fail("Bạn không có quyền thực hiện chức năng này."));
                }
            }
        }
    }
}
