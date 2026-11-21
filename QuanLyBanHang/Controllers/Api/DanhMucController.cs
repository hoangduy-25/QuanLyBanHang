using System;
using System.Threading.Tasks;
using System.Web.Http;
using QuanLyBanHang.Infrastructure.Filters;
using QuanLyBanHang.Models.DTOs.DanhMuc;
using QuanLyBanHang.Services.Implementations;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Controllers.Api
{
    [RoutePrefix("api/danh-muc")]
    public class DanhMucController : BaseApiController
    {
        private readonly IDanhMucService _service;

        public DanhMucController()
        {
            _service = new DanhMucService(UnitOfWork);
        }

        // UC-11: GET /api/danh-muc
        [HttpGet, Route("")]
        [JwtAuthorize(Optional = true)]
        public async Task<IHttpActionResult> XemDanhSach()
        {
            var res = await _service.XemDanhSachAsync(IsManager);
            return OkApi(res);
        }

        // UC-12: GET /api/danh-muc/tim-kiem?tuKhoa=...
        [HttpGet, Route("tim-kiem")]
        [JwtAuthorize(Optional = true)]
        public async Task<IHttpActionResult> TimKiem([FromUri] string tuKhoa = null)
        {
            var res = await _service.TimKiemAsync(tuKhoa, IsManager);
            return OkApi(res);
        }

        // UC-13: POST /api/danh-muc (Quản lý)
        [HttpPost, Route("")]
        [JwtAuthorize(Roles = "QuanLy")]
        public async Task<IHttpActionResult> Them([FromBody] TaoSuaDanhMucRequest request)
        {
            var res = await _service.ThemAsync(request);
            return CreatedApi(res, "Thêm danh mục thành công.");
        }

        // UC-14: PUT /api/danh-muc/{id} (Quản lý)
        [HttpPut, Route("{id:int}")]
        [JwtAuthorize(Roles = "QuanLy")]
        public async Task<IHttpActionResult> Sua(int id, [FromBody] TaoSuaDanhMucRequest request)
        {
            var res = await _service.SuaAsync(id, request);
            return OkApi(res, "Cập nhật danh mục thành công.");
        }

        // UC-15: DELETE /api/danh-muc/{id} (Quản lý)
        [HttpDelete, Route("{id:int}")]
        [JwtAuthorize(Roles = "QuanLy")]
        public async Task<IHttpActionResult> Xoa(int id)
        {
            await _service.XoaAsync(id);
            return OkApi<object>(null, "Xóa danh mục thành công.");
        }

        // UC-16: PATCH /api/danh-muc/{id}/an-hien (Quản lý)
        [HttpPatch, Route("{id:int}/an-hien")]
        [JwtAuthorize(Roles = "QuanLy")]
        public async Task<IHttpActionResult> AnHien(int id, [FromBody] AnHienDanhMucRequest request)
        {
            var res = await _service.AnHienAsync(id, request);
            return OkApi(res, "Cập nhật trạng thái hiển thị danh mục thành công.");
        }
    }
}
