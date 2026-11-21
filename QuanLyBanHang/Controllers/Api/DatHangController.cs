using System.Threading.Tasks;
using System.Web.Http;
using QuanLyBanHang.Infrastructure.Filters;
using QuanLyBanHang.Models.DTOs.DatHang;
using QuanLyBanHang.Services.Implementations;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Controllers.Api
{
    [RoutePrefix("api/dat-hang")]
    [JwtAuthorize(Roles = "KhachHang")]
    public class DatHangController : BaseApiController
    {
        private readonly IDatHangService _service;

        public DatHangController()
        {
            _service = new DatHangService(UnitOfWork, new GioHangService(UnitOfWork));
        }

        // UC-34: POST /api/dat-hang/chon-dia-chi
        [HttpPost, Route("chon-dia-chi")]
        public async Task<IHttpActionResult> ChonDiaChi([FromBody] ChonDiaChiRequest request)
        {
            var res = await _service.ChonDiaChiAsync(CurrentUserId.Value, request);
            return OkApi(res);
        }

        // UC-35: POST /api/dat-hang/ap-dung-ma-giam-gia
        [HttpPost, Route("ap-dung-ma-giam-gia")]
        public async Task<IHttpActionResult> ApDungMaGiamGia([FromBody] ApDungMaGiamGiaRequest request)
        {
            var res = await _service.ApDungMaGiamGiaAsync(CurrentUserId.Value, request);
            return OkApi(res, "Áp dụng mã giảm giá thành công.");
        }

        // UC-36: POST /api/dat-hang/chon-phuong-thuc-thanh-toan
        [HttpPost, Route("chon-phuong-thuc-thanh-toan")]
        public async Task<IHttpActionResult> ChonPhuongThucThanhToan([FromBody] ChonPhuongThucThanhToanRequest request)
        {
            var res = await _service.ChonPhuongThucThanhToanAsync(CurrentUserId.Value, request);
            return OkApi(res);
        }

        // UC-40: POST /api/dat-hang/xem-truoc
        [HttpPost, Route("xem-truoc")]
        public async Task<IHttpActionResult> XemTruoc([FromBody] XemTruocDatHangRequest request)
        {
            var res = await _service.XemTruocAsync(CurrentUserId.Value, request);
            return OkApi(res);
        }

        // UC-37 & UC-40: POST /api/dat-hang/xac-nhan
        [HttpPost, Route("xac-nhan")]
        public async Task<IHttpActionResult> XacNhanDatHang([FromBody] XacNhanDatHangRequest request)
        {
            var res = await _service.XacNhanDatHangAsync(CurrentUserId.Value, request);
            return CreatedApi(res, "Đặt hàng thành công.");
        }
    }
}
