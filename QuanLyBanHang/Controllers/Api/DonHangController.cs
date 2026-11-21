using System;
using System.Threading.Tasks;
using System.Web.Http;
using QuanLyBanHang.Infrastructure.Filters;
using QuanLyBanHang.Models.DTOs.DonHang;
using QuanLyBanHang.Services.Implementations;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Controllers.Api
{
    [RoutePrefix("api/don-hang")]
    public class DonHangController : BaseApiController
    {
        private readonly IDonHangService _service;

        public DonHangController()
        {
            _service = new DonHangService(UnitOfWork);
        }

        // UC-38: GET /api/don-hang (Khách hàng, Nhân viên)
        [HttpGet, Route("")]
        [JwtAuthorize(Roles = "KhachHang,NhanVien")]
        public async Task<IHttpActionResult> XemDanhSach([FromUri] DonHangFilterQuery query)
        {
            var res = await _service.XemDanhSachAsync(CurrentUserId.Value, CurrentUserRole, query);
            return OkApi(res);
        }

        // UC-39: GET /api/don-hang/{id} (Khách hàng, Nhân viên)
        [HttpGet, Route("{id:guid}")]
        [JwtAuthorize(Roles = "KhachHang,NhanVien")]
        public async Task<IHttpActionResult> XemChiTiet(Guid id)
        {
            var res = await _service.XemChiTietAsync(CurrentUserId.Value, CurrentUserRole, id);
            return OkApi(res);
        }

        // UC-41: PATCH /api/don-hang/{id}/huy (Khách hàng, Nhân viên)
        [HttpPatch, Route("{id:guid}/huy")]
        [JwtAuthorize(Roles = "KhachHang,NhanVien")]
        public async Task<IHttpActionResult> Huy(Guid id, [FromBody] HuyDonHangRequest request)
        {
            await _service.HuyAsync(CurrentUserId.Value, CurrentUserRole, id, request);
            return OkApi<object>(null, "Hủy đơn hàng thành công.");
        }

        // UC-42: GET /api/don-hang/{id}/hanh-trinh (Khách hàng)
        [HttpGet, Route("{id:guid}/hanh-trinh")]
        [JwtAuthorize(Roles = "KhachHang")]
        public async Task<IHttpActionResult> TraCuuHanhTrinh(Guid id)
        {
            var res = await _service.TraCuuHanhTrinhAsync(CurrentUserId.Value, id);
            return OkApi(res);
        }

        // UC-43: PATCH /api/don-hang/{id}/xac-nhan (Nhân viên)
        [HttpPatch, Route("{id:guid}/xac-nhan")]
        [JwtAuthorize(Roles = "NhanVien")]
        public async Task<IHttpActionResult> XacNhan(Guid id)
        {
            await _service.XacNhanAsync(CurrentUserId.Value, id);
            return OkApi<object>(null, "Xác nhận đơn hàng thành công.");
        }

        // UC-44: PATCH /api/don-hang/{id}/trang-thai (Nhân viên)
        [HttpPatch, Route("{id:guid}/trang-thai")]
        [JwtAuthorize(Roles = "NhanVien")]
        public async Task<IHttpActionResult> CapNhatTrangThai(Guid id, [FromBody] CapNhatTrangThaiDonHangRequest request)
        {
            await _service.CapNhatTrangThaiAsync(CurrentUserId.Value, id, request);
            return OkApi<object>(null, "Cập nhật trạng thái đơn hàng thành công.");
        }
    }
}
