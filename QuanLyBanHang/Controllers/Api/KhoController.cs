using System;
using System.Threading.Tasks;
using System.Web.Http;
using QuanLyBanHang.Infrastructure.Filters;
using QuanLyBanHang.Models.DTOs.Kho;
using QuanLyBanHang.Services.Implementations;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Controllers.Api
{
    [RoutePrefix("api/kho")]
    [JwtAuthorize(Roles = "QuanLy")]
    public class KhoController : BaseApiController
    {
        private readonly IKhoService _service;

        public KhoController()
        {
            _service = new KhoService(UnitOfWork);
        }

        // UC-24: GET /api/kho
        [HttpGet, Route("")]
        public async Task<IHttpActionResult> XemTonKho([FromUri] TonKhoFilterQuery query)
        {
            var res = await _service.XemTonKhoAsync(query);
            return OkApi(res);
        }

        // UC-25: POST /api/kho/nhap-tang
        [HttpPost, Route("nhap-tang")]
        public async Task<IHttpActionResult> NhapTang([FromBody] NhapTangKhoRequest request)
        {
            var res = await _service.NhapTangAsync(CurrentUserId.Value, request);
            return OkApi(res, "Nhập hàng tăng tồn kho thành công.");
        }

        // UC-26: POST /api/kho/dieu-chinh-giam
        [HttpPost, Route("dieu-chinh-giam")]
        public async Task<IHttpActionResult> DieuChinhGiam([FromBody] DieuChinhGiamKhoRequest request)
        {
            var res = await _service.DieuChinhGiamAsync(CurrentUserId.Value, request);
            return OkApi(res, "Điều chỉnh giảm tồn kho thành công.");
        }

        // UC-27: PATCH /api/kho/{id}/nguong-toi-thieu
        [HttpPatch, Route("{id:guid}/nguong-toi-thieu")]
        public async Task<IHttpActionResult> DatNguongToiThieu(Guid id, [FromBody] DatNguongToiThieuRequest request)
        {
            var res = await _service.DatNguongToiThieuAsync(id, request);
            return OkApi(res, "Cập nhật ngưỡng tồn kho tối thiểu thành công.");
        }

        // UC-28: GET /api/kho/lich-su
        [HttpGet, Route("lich-su")]
        public async Task<IHttpActionResult> XemLichSu([FromUri] BienDongKhoFilterQuery query)
        {
            var res = await _service.XemLichSuAsync(query);
            return OkApi(res);
        }
    }
}
