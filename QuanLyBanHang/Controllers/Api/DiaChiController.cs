using System;
using System.Threading.Tasks;
using System.Web.Http;
using QuanLyBanHang.Infrastructure.Filters;
using QuanLyBanHang.Models.DTOs.DiaChi;
using QuanLyBanHang.Services.Implementations;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Controllers.Api
{
    [RoutePrefix("api/dia-chi")]
    [JwtAuthorize(Roles = "KhachHang")]
    public class DiaChiController : BaseApiController
    {
        private readonly IDiaChiService _service;

        public DiaChiController()
        {
            _service = new DiaChiService(UnitOfWork);
        }

        // UC-07: GET /api/dia-chi
        [HttpGet, Route("")]
        public async Task<IHttpActionResult> XemSoDiaChi()
        {
            var res = await _service.XemSoDiaChiAsync(CurrentUserId.Value);
            return OkApi(res);
        }

        // UC-07: POST /api/dia-chi
        [HttpPost, Route("")]
        public async Task<IHttpActionResult> ThemDiaChi([FromBody] TaoSuaDiaChiRequest request)
        {
            var res = await _service.ThemDiaChiAsync(CurrentUserId.Value, request);
            return CreatedApi(res, "Thêm địa chỉ nhận hàng thành công.");
        }

        // UC-07: PUT /api/dia-chi/{id}
        [HttpPut, Route("{id:int}")]
        public async Task<IHttpActionResult> SuaDiaChi(int id, [FromBody] TaoSuaDiaChiRequest request)
        {
            var res = await _service.SuaDiaChiAsync(CurrentUserId.Value, id, request);
            return OkApi(res, "Cập nhật địa chỉ nhận hàng thành công.");
        }

        // UC-07: DELETE /api/dia-chi/{id}
        [HttpDelete, Route("{id:int}")]
        public async Task<IHttpActionResult> XoaDiaChi(int id)
        {
            await _service.XoaDiaChiAsync(CurrentUserId.Value, id);
            return OkApi<object>(null, "Xóa địa chỉ nhận hàng thành công.");
        }

        // UC-07: PATCH /api/dia-chi/{id}/mac-dinh
        [HttpPatch, Route("{id:int}/mac-dinh")]
        public async Task<IHttpActionResult> DatMacDinh(int id)
        {
            var res = await _service.DatMacDinhAsync(CurrentUserId.Value, id);
            return OkApi(res, "Đã đặt làm địa chỉ mặc định.");
        }
    }
}
