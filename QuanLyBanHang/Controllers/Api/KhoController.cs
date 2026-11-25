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
            var msg = "Điều chỉnh giảm tồn kho thành công.";
            if (res.SoLuongTon == 0)
                msg += " Cảnh báo: Sản phẩm đã hết hàng (tồn = 0).";
            else if (res.SoLuongTon <= res.NguongToiThieu)
                msg += string.Format(" Cảnh báo: Tồn kho ({0}) đã chạm hoặc thấp hơn ngưỡng tối thiểu ({1}).", res.SoLuongTon, res.NguongToiThieu);
            return OkApi(res, msg);
        }

        // UC-27: PATCH /api/kho/{id}/nguong-toi-thieu
        [HttpPatch, Route("{id:int}/nguong-toi-thieu")]
        public async Task<IHttpActionResult> DatNguongToiThieu(int id, [FromBody] DatNguongToiThieuRequest request)
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
