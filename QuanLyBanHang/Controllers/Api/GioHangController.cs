using System;
using System.Threading.Tasks;
using System.Web.Http;
using QuanLyBanHang.Infrastructure.Filters;
using QuanLyBanHang.Models.DTOs.GioHang;
using QuanLyBanHang.Services.Implementations;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Controllers.Api
{
    [RoutePrefix("api/gio-hang")]
    [JwtAuthorize(Roles = "KhachHang")]
    public class GioHangController : BaseApiController
    {
        private readonly IGioHangService _service;

        public GioHangController()
        {
            _service = new GioHangService(UnitOfWork);
        }

        // UC-29: GET /api/gio-hang
        [HttpGet, Route("")]
        public async Task<IHttpActionResult> XemGioHang()
        {
            var res = await _service.XemGioHangAsync(CurrentUserId.Value);
            return OkApi(res);
        }

        // UC-30: POST /api/gio-hang/san-pham
        [HttpPost, Route("san-pham")]
        public async Task<IHttpActionResult> ThemSanPham([FromBody] ThemVaoGioRequest request)
        {
            var res = await _service.ThemSanPhamAsync(CurrentUserId.Value, request);
            return OkApi(res, "Đã thêm sản phẩm vào giỏ hàng.");
        }

        // UC-31: PUT /api/gio-hang/san-pham/{sanPhamId}
        [HttpPut, Route("san-pham/{sanPhamId:guid}")]
        public async Task<IHttpActionResult> SuaSoLuong(Guid sanPhamId, [FromBody] SuaSoLuongGioRequest request)
        {
            var res = await _service.SuaSoLuongAsync(CurrentUserId.Value, sanPhamId, request);
            return OkApi(res, "Cập nhật số lượng giỏ hàng thành công.");
        }

        // UC-32: DELETE /api/gio-hang/san-pham/{sanPhamId}
        [HttpDelete, Route("san-pham/{sanPhamId:guid}")]
        public async Task<IHttpActionResult> XoaSanPham(Guid sanPhamId)
        {
            var res = await _service.XoaSanPhamAsync(CurrentUserId.Value, sanPhamId);
            return OkApi(res, "Đã xóa sản phẩm khỏi giỏ hàng.");
        }

        // UC-33: GET /api/gio-hang/yeu-thich
        [HttpGet, Route("yeu-thich")]
        public async Task<IHttpActionResult> XemYeuThich()
        {
            var res = await _service.XemYeuThichAsync(CurrentUserId.Value);
            return OkApi(res);
        }

        // UC-33: POST /api/gio-hang/yeu-thich/{sanPhamId}
        [HttpPost, Route("yeu-thich/{sanPhamId:guid}")]
        public async Task<IHttpActionResult> ThemYeuThich(Guid sanPhamId)
        {
            await _service.ThemYeuThichAsync(CurrentUserId.Value, sanPhamId);
            return OkApi<object>(null, "Đã thêm sản phẩm vào danh sách yêu thích.");
        }

        // UC-33: DELETE /api/gio-hang/yeu-thich/{sanPhamId}
        [HttpDelete, Route("yeu-thich/{sanPhamId:guid}")]
        public async Task<IHttpActionResult> XoaYeuThich(Guid sanPhamId)
        {
            await _service.XoaYeuThichAsync(CurrentUserId.Value, sanPhamId);
            return OkApi<object>(null, "Đã bỏ sản phẩm khỏi danh sách yêu thích.");
        }
    }
}
