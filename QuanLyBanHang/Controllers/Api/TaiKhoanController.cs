using System;
using System.Threading.Tasks;
using System.Web.Http;
using QuanLyBanHang.Infrastructure.Filters;
using QuanLyBanHang.Models.DTOs.TaiKhoan;
using QuanLyBanHang.Services.Implementations;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Controllers.Api
{
    [RoutePrefix("api/tai-khoan")]
    public class TaiKhoanController : BaseApiController
    {
        private readonly ITaiKhoanService _service;

        public TaiKhoanController()
        {
            _service = new TaiKhoanService(UnitOfWork);
        }

        // UC-06: GET /api/tai-khoan/ho-so
        [HttpGet, Route("ho-so")]
        [JwtAuthorize(Roles = "KhachHang,NhanVien,QuanLy")]
        public async Task<IHttpActionResult> LayHoSo()
        {
            var res = await _service.LayHoSoAsync(CurrentUserId.Value);
            return OkApi(res);
        }

        // UC-06: PUT /api/tai-khoan/ho-so
        [HttpPut, Route("ho-so")]
        [JwtAuthorize(Roles = "KhachHang,NhanVien,QuanLy")]
        public async Task<IHttpActionResult> CapNhatThongTinCaNhan([FromBody] CapNhatHoSoRequest request)
        {
            var res = await _service.CapNhatThongTinCaNhanAsync(CurrentUserId.Value, request);
            return OkApi(res, "Cập nhật thông tin cá nhân thành công.");
        }

        // UC-08: GET /api/tai-khoan (Quản lý)
        [HttpGet, Route("")]
        [JwtAuthorize(Roles = "QuanLy")]
        public async Task<IHttpActionResult> XemDanhSach([FromUri] TaiKhoanFilterQuery query)
        {
            var res = await _service.XemDanhSachAsync(query);
            return OkApi(res);
        }

        // UC-09: GET /api/tai-khoan/tim-kiem (Quản lý)
        [HttpGet, Route("tim-kiem")]
        [JwtAuthorize(Roles = "QuanLy")]
        public async Task<IHttpActionResult> TimKiem([FromUri] TaiKhoanFilterQuery query)
        {
            var res = await _service.TimKiemAsync(query);
            return OkApi(res);
        }

        // UC-10: PATCH /api/tai-khoan/{id}/khoa (Quản lý)
        [HttpPatch, Route("{id:int}/khoa")]
        [JwtAuthorize(Roles = "QuanLy")]
        public async Task<IHttpActionResult> Khoa(int id, [FromBody] KhoaTaiKhoanRequest request)
        {
            var res = await _service.KhoaAsync(CurrentUserId.Value, id, request);
            return OkApi(res, "Khóa tài khoản khách hàng thành công.");
        }

        // UC-10: PATCH /api/tai-khoan/{id}/mo-khoa (Quản lý)
        [HttpPatch, Route("{id:int}/mo-khoa")]
        [JwtAuthorize(Roles = "QuanLy")]
        public async Task<IHttpActionResult> MoKhoa(int id)
        {
            var res = await _service.MoKhoaAsync(CurrentUserId.Value, id);
            return OkApi(res, "Mở khóa tài khoản khách hàng thành công.");
        }
    }
}
