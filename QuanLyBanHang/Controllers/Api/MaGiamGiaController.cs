using System;
using System.Threading.Tasks;
using System.Web.Http;
using QuanLyBanHang.Infrastructure.Filters;
using QuanLyBanHang.Models.DTOs.MaGiamGia;
using QuanLyBanHang.Services.Implementations;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Controllers.Api
{
    [RoutePrefix("api/ma-giam-gia")]
    public class MaGiamGiaController : BaseApiController
    {
        private readonly IMaGiamGiaService _service;

        public MaGiamGiaController()
        {
            _service = new MaGiamGiaService(UnitOfWork);
        }

        // UC-45: GET /api/ma-giam-gia (Khách hàng, Quản lý)
        [HttpGet, Route("")]
        [JwtAuthorize(Roles = "KhachHang,QuanLy")]
        public async Task<IHttpActionResult> XemDanhSach()
        {
            var res = await _service.XemDanhSachAsync(IsManager);
            return OkApi(res);
        }

        // UC-46: GET /api/ma-giam-gia/tim-kiem (Khách hàng, Quản lý)
        [HttpGet, Route("tim-kiem")]
        [JwtAuthorize(Roles = "KhachHang,QuanLy")]
        public async Task<IHttpActionResult> TimKiem([FromUri] string tuKhoa = null)
        {
            var res = await _service.TimKiemAsync(tuKhoa, IsManager);
            return OkApi(res);
        }

        // UC-47: GET /api/ma-giam-gia/{id} (Khách hàng, Quản lý)
        [HttpGet, Route("{id:guid}")]
        [JwtAuthorize(Roles = "KhachHang,QuanLy")]
        public async Task<IHttpActionResult> XemChiTiet(Guid id)
        {
            var res = await _service.XemChiTietAsync(id, IsManager);
            return OkApi(res);
        }

        // UC-48: POST /api/ma-giam-gia (Quản lý)
        [HttpPost, Route("")]
        [JwtAuthorize(Roles = "QuanLy")]
        public async Task<IHttpActionResult> Them([FromBody] TaoMaGiamGiaRequest request)
        {
            var res = await _service.ThemAsync(request);
            return CreatedApi(res, "Thêm mã giảm giá thành công.");
        }

        // UC-49: PUT /api/ma-giam-gia/{id} (Quản lý)
        [HttpPut, Route("{id:guid}")]
        [JwtAuthorize(Roles = "QuanLy")]
        public async Task<IHttpActionResult> Sua(Guid id, [FromBody] SuaMaGiamGiaRequest request)
        {
            var res = await _service.SuaAsync(id, request);
            return OkApi(res, "Cập nhật mã giảm giá thành công.");
        }

        // UC-50: DELETE /api/ma-giam-gia/{id} (Quản lý)
        [HttpDelete, Route("{id:guid}")]
        [JwtAuthorize(Roles = "QuanLy")]
        public async Task<IHttpActionResult> Xoa(Guid id)
        {
            var msg = await _service.XoaAsync(id);
            return OkApi<object>(null, msg);
        }
    }
}
