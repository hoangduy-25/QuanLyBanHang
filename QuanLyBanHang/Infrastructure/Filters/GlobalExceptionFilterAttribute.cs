using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using QuanLyBanHang.Models.DTOs.Common;
using QuanLyBanHang.Services.Exceptions;

namespace QuanLyBanHang.Infrastructure.Filters
{
    /// <summary>
    /// Bắt lỗi toàn cục cho Web API: ánh xạ AppException sang mã HTTP tương ứng (400/401/403/404/409)
    /// và ẩn chi tiết kỹ thuật khi xảy ra lỗi hệ thống 500.
    /// </summary>
    public class GlobalExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            var appEx = context.Exception as AppException;
            if (appEx != null)
            {
                context.Response = context.Request.CreateResponse(
                    appEx.StatusCode,
                    ApiResponse<object>.Fail(appEx.Message, appEx.Details));
                return;
            }

            context.Response = context.Request.CreateResponse(
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail("Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau."));
        }
    }
}
