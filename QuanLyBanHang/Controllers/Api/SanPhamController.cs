using System;
using System.Threading.Tasks;
using System.Web.Http;
using QuanLyBanHang.Infrastructure.Filters;
using QuanLyBanHang.Models.DTOs.SanPham;
using QuanLyBanHang.Services.Implementations;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Controllers.Api
{
    [RoutePrefix("api/san-pham")]
    public class SanPhamController : BaseApiController
    {
        private readonly ISanPhamService _service;

        public SanPhamController()
        {
            _service = new SanPhamService(UnitOfWork);
        }

        // UC-17: GET /api/san-pham
        [HttpGet, Route("")]
        [JwtAuthorize(Optional = true)]
        public async Task<IHttpActionResult> XemDanhSach([FromUri] SanPhamFilterQuery query)
        {
            var res = await _service.XemDanhSachAsync(query, IsManager);
            return OkApi(res);
        }

        // UC-19: GET /api/san-pham/tim-kiem
        [HttpGet, Route("tim-kiem")]
        [JwtAuthorize(Optional = true)]
        public async Task<IHttpActionResult> TimKiem([FromUri] SanPhamFilterQuery query)
        {
            var res = await _service.TimKiemAsync(query, IsManager);
            return OkApi(res);
        }

        // UC-18: GET /api/san-pham/{id}
        [HttpGet, Route("{id:int}")]
        [JwtAuthorize(Optional = true)]
        public async Task<IHttpActionResult> XemChiTiet(int id)
        {
            var res = await _service.XemChiTietAsync(id, IsManager);
            return OkApi(res);
        }

        // UC-20: POST /api/san-pham (Quản lý)
        [HttpPost, Route("")]
        [JwtAuthorize(Roles = "QuanLy")]
        public async Task<IHttpActionResult> Them([FromBody] TaoSanPhamRequest request)
        {
            var res = await _service.ThemAsync(request, CurrentUserId.Value);
            return CreatedApi(res, "Thêm sản phẩm thành công.");
        }

        // UC-21: PUT /api/san-pham/{id} (Quản lý)
        [HttpPut, Route("{id:int}")]
        [JwtAuthorize(Roles = "QuanLy")]
        public async Task<IHttpActionResult> Sua(int id, [FromBody] SuaSanPhamRequest request)
        {
            var res = await _service.SuaAsync(id, request);
            return OkApi(res, "Cập nhật sản phẩm thành công.");
        }

        // UC-22: DELETE /api/san-pham/{id} (Quản lý)
        [HttpDelete, Route("{id:int}")]
        [JwtAuthorize(Roles = "QuanLy")]
        public async Task<IHttpActionResult> Xoa(int id)
        {
            await _service.XoaAsync(id);
            return OkApi<object>(null, "Xóa sản phẩm thành công.");
        }

        // UC-23: PATCH /api/san-pham/{id}/an-hien (Quản lý)
        [HttpPatch, Route("{id:int}/an-hien")]
        [JwtAuthorize(Roles = "QuanLy")]
        public async Task<IHttpActionResult> AnHien(int id, [FromBody] AnHienSanPhamRequest request)
        {
            var res = await _service.AnHienAsync(id, request);
            return OkApi(res, "Cập nhật trạng thái hiển thị sản phẩm thành công.");
        }
    }
}
