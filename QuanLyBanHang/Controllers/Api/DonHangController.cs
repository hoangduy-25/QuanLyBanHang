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

        // UC-38: GET /api/don-hang (Khách hàng, Nhân viên, Quản lý chỉ xem)
        [HttpGet, Route("")]
        [JwtAuthorize(Roles = "KhachHang,NhanVien,QuanLy")]
        public async Task<IHttpActionResult> XemDanhSach([FromUri] DonHangFilterQuery query)
        {
            var res = await _service.XemDanhSachAsync(CurrentUserId.Value, CurrentUserRole, query);
            return OkApi(res);
        }

        // UC-39: GET /api/don-hang/{id} (Khách hàng, Nhân viên, Quản lý chỉ xem)
        [HttpGet, Route("{id:int}")]
        [JwtAuthorize(Roles = "KhachHang,NhanVien,QuanLy")]
        public async Task<IHttpActionResult> XemChiTiet(int id)
        {
            var res = await _service.XemChiTietAsync(CurrentUserId.Value, CurrentUserRole, id);
            return OkApi(res);
        }

        // UC-41: PATCH /api/don-hang/{id}/huy (Khách hàng, Nhân viên)
        [HttpPatch, Route("{id:int}/huy")]
        [JwtAuthorize(Roles = "KhachHang,NhanVien")]
        public async Task<IHttpActionResult> Huy(int id, [FromBody] HuyDonHangRequest request)
        {
            await _service.HuyAsync(CurrentUserId.Value, CurrentUserRole, id, request);
            return OkApi<object>(null, "Hủy đơn hàng thành công.");
        }

        // UC-42: GET /api/don-hang/{id}/hanh-trinh (Khách hàng)
        [HttpGet, Route("{id:int}/hanh-trinh")]
        [JwtAuthorize(Roles = "KhachHang")]
        public async Task<IHttpActionResult> TraCuuHanhTrinh(int id)
        {
            var res = await _service.TraCuuHanhTrinhAsync(CurrentUserId.Value, id);
            return OkApi(res);
        }

        // UC-43: PATCH /api/don-hang/{id}/xac-nhan (Nhân viên)
        [HttpPatch, Route("{id:int}/xac-nhan")]
        [JwtAuthorize(Roles = "NhanVien")]
        public async Task<IHttpActionResult> XacNhan(int id)
        {
            await _service.XacNhanAsync(CurrentUserId.Value, id);
            return OkApi<object>(null, "Xác nhận đơn hàng thành công.");
        }

        // UC-44: PATCH /api/don-hang/{id}/trang-thai (Nhân viên)
        [HttpPatch, Route("{id:int}/trang-thai")]
        [JwtAuthorize(Roles = "NhanVien")]
        public async Task<IHttpActionResult> CapNhatTrangThai(int id, [FromBody] CapNhatTrangThaiDonHangRequest request)
        {
            await _service.CapNhatTrangThaiAsync(CurrentUserId.Value, id, request);
            return OkApi<object>(null, "Cập nhật trạng thái đơn hàng thành công.");
        }
    }
}
