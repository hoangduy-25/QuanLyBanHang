using System;
using System.Net;
using System.Security.Claims;
using System.Web.Http;
using QuanLyBanHang.Data;
using QuanLyBanHang.Models.DTOs.Common;
using QuanLyBanHang.Models.Enums;
using QuanLyBanHang.Repositories.Implementations;
using QuanLyBanHang.Repositories.Interfaces;

namespace QuanLyBanHang.Controllers.Api
{
    /// <summary>
    /// Base ApiController quản lý vòng đời DbContext/UnitOfWork và đọc thông tin Claims từ JWT.
    /// </summary>
    public abstract class BaseApiController : ApiController
    {
        protected readonly IUnitOfWork UnitOfWork;

        protected BaseApiController()
        {
            UnitOfWork = new UnitOfWork(new QuanLyBanHangDbContext());
        }

        protected Guid? CurrentUserId
        {
            get
            {
                var principal = RequestContext != null ? RequestContext.Principal as ClaimsPrincipal : null;
                var sub = principal != null ? principal.FindFirst(ClaimTypes.NameIdentifier) : null;
                Guid id;
                return (sub != null && Guid.TryParse(sub.Value, out id)) ? (Guid?)id : null;
            }
        }

        protected string CurrentUserRole
        {
            get
            {
                var principal = RequestContext != null ? RequestContext.Principal as ClaimsPrincipal : null;
                var role = principal != null ? principal.FindFirst(ClaimTypes.Role) : null;
                return role != null ? role.Value : null;
            }
        }

        protected string CurrentUserName
        {
            get
            {
                var principal = RequestContext != null ? RequestContext.Principal as ClaimsPrincipal : null;
                var name = principal != null ? principal.FindFirst(ClaimTypes.Name) : null;
                return name != null ? name.Value : null;
            }
        }

        protected bool IsManager
        {
            get { return CurrentUserRole == VaiTro.QuanLy.ToString(); }
        }

        protected IHttpActionResult OkApi<T>(T data, string message = "Thành công")
        {
            return Ok(ApiResponse<T>.Ok(data, message));
        }

        protected IHttpActionResult CreatedApi<T>(T data, string message = "Tạo mới thành công")
        {
            return Content(HttpStatusCode.Created, ApiResponse<T>.Ok(data, message));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UnitOfWork != null)
            {
                UnitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
