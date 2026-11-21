using System;
using System.Threading.Tasks;
using System.Web.Http;
using QuanLyBanHang.Infrastructure.Filters;
using QuanLyBanHang.Models.DTOs.DanhGia;
using QuanLyBanHang.Services.Implementations;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Controllers.Api
{
    [RoutePrefix("api/danh-gia")]
    public class DanhGiaController : BaseApiController
    {
        private readonly IDanhGiaService _service;

        public DanhGiaController()
        {
            _service = new DanhGiaService(UnitOfWork);
        }

        // UC-51: GET /api/danh-gia (Công khai, Khách hàng, Nhân viên, Quản lý)
        [HttpGet, Route("")]
        [JwtAuthorize(Optional = true)]
        public async Task<IHttpActionResult> XemDanhSach([FromUri] DanhGiaFilterQuery query)
        {
            var res = await _service.XemDanhSachAsync(query, CurrentUserId, CurrentUserRole);
            return OkApi(res);
        }

        // UC-52: GET /api/danh-gia/loc (Công khai, Khách hàng, Nhân viên, Quản lý)
        [HttpGet, Route("loc")]
        [JwtAuthorize(Optional = true)]
        public async Task<IHttpActionResult> Loc([FromUri] DanhGiaFilterQuery query)
        {
            var res = await _service.LocAsync(query, CurrentUserId, CurrentUserRole);
            return OkApi(res);
        }

        // UC-53: POST /api/danh-gia (Khách hàng)
        [HttpPost, Route("")]
        [JwtAuthorize(Roles = "KhachHang")]
        public async Task<IHttpActionResult> Tao([FromBody] TaoDanhGiaRequest request)
        {
            var res = await _service.TaoAsync(CurrentUserId.Value, request);
            return CreatedApi(res, "Gửi đánh giá thành công.");
        }

        // UC-54: PUT /api/danh-gia/{id} (Khách hàng)
        [HttpPut, Route("{id:int}")]
        [JwtAuthorize(Roles = "KhachHang")]
        public async Task<IHttpActionResult> Sua(int id, [FromBody] SuaDanhGiaRequest request)
        {
            var res = await _service.SuaAsync(CurrentUserId.Value, id, request);
            return OkApi(res, "Chỉnh sửa đánh giá thành công.");
        }

        // UC-55: POST /api/danh-gia/{id}/thich (Khách hàng, Nhân viên, Quản lý)
        [HttpPost, Route("{id:int}/thich")]
        [JwtAuthorize(Roles = "KhachHang,NhanVien,QuanLy")]
        public async Task<IHttpActionResult> Thich(int id)
        {
            var res = await _service.ThichAsync(CurrentUserId.Value, id);
            return OkApi(res);
        }

        // UC-56: DELETE /api/danh-gia/{id} (Khách hàng, Quản lý)
        [HttpDelete, Route("{id:int}")]
        [JwtAuthorize(Roles = "KhachHang,QuanLy")]
        public async Task<IHttpActionResult> Xoa(int id, [FromBody] XoaDanhGiaRequest request = null)
        {
            await _service.XoaAsync(CurrentUserId.Value, CurrentUserRole, id, request);
            return OkApi<object>(null, "Xóa đánh giá thành công.");
        }

        // UC-57: POST /api/danh-gia/{id}/tra-loi (Nhân viên, Quản lý)
        [HttpPost, Route("{id:int}/tra-loi")]
        [JwtAuthorize(Roles = "NhanVien,QuanLy")]
        public async Task<IHttpActionResult> TraLoi(int id, [FromBody] TraLoiDanhGiaRequest request)
        {
            var res = await _service.TraLoiAsync(CurrentUserId.Value, id, request);
            return OkApi(res, "Phản hồi đánh giá thành công.");
        }
    }
}
