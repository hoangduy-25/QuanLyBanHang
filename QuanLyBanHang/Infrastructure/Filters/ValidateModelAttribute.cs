using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using QuanLyBanHang.Models.DTOs.Common;

namespace QuanLyBanHang.Infrastructure.Filters
{
    /// <summary>
    /// Tự động kiểm tra Data Annotations (ModelState) và trả về HTTP 400 theo định dạng ApiResponse thống nhất.
    /// </summary>
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.ModelState != null && !actionContext.ModelState.IsValid)
            {
                var errors = actionContext.ModelState
                    .Where(kv => kv.Value.Errors.Count > 0)
                    .ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Dữ liệu không hợp lệ." : e.ErrorMessage).ToArray()
                    );

                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.BadRequest,
                    ApiResponse<object>.Fail("Dữ liệu đầu vào không hợp lệ.", errors));
            }
        }
    }
}
