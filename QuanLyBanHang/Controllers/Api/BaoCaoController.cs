using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using QuanLyBanHang.Infrastructure.Filters;
using QuanLyBanHang.Models.DTOs.BaoCao;
using QuanLyBanHang.Services.Implementations;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Controllers.Api
{
    [RoutePrefix("api/bao-cao")]
    [JwtAuthorize(Roles = "QuanLy")]
    public class BaoCaoController : BaseApiController
    {
        private readonly IBaoCaoService _service;

        public BaoCaoController()
        {
            _service = new BaoCaoService(UnitOfWork);
        }

        // UC-58: GET /api/bao-cao/tong-quan
        [HttpGet, Route("tong-quan")]
        public async Task<IHttpActionResult> XemTongQuan([FromUri] DateTimeOffset? tuNgay = null, [FromUri] DateTimeOffset? denNgay = null)
        {
            var res = await _service.XemTongQuanAsync(tuNgay, denNgay);
            return OkApi(res);
        }

        // UC-59: GET /api/bao-cao/doanh-thu
        [HttpGet, Route("doanh-thu")]
        public async Task<IHttpActionResult> XemBieuDoDoanhThu(
            [FromUri] DateTimeOffset? tuNgay = null,
            [FromUri] DateTimeOffset? denNgay = null,
            [FromUri] string donVi = "ngay")
        {
            var res = await _service.XemBieuDoDoanhThuAsync(tuNgay, denNgay, donVi);
            return OkApi(res);
        }

        // UC-60: GET /api/bao-cao/san-pham-ban-chay
        [HttpGet, Route("san-pham-ban-chay")]
        public async Task<IHttpActionResult> XemSanPhamBanChay([FromUri] SanPhamBanChayQuery query)
        {
            var res = await _service.XemSanPhamBanChayAsync(query);
            return OkApi(res);
        }

        // UC-61: GET /api/bao-cao/san-pham-ton-lau
        [HttpGet, Route("san-pham-ton-lau")]
        public async Task<IHttpActionResult> XemSanPhamTonLau([FromUri] SanPhamTonLauQuery query)
        {
            var res = await _service.XemSanPhamTonLauAsync(query);
            return OkApi(res);
        }

        // UC-62: GET /api/bao-cao/in-bao-cao
        [HttpGet, Route("in-bao-cao")]
        public async Task<HttpResponseMessage> InBaoCao(
            [FromUri] string loaiBaoCao = "tong-quan",
            [FromUri] DateTimeOffset? tuNgay = null,
            [FromUri] DateTimeOffset? denNgay = null,
            [FromUri] string donVi = "ngay",
            [FromUri] int top = 10,
            [FromUri] int soNgayTonLau = 30)
        {
            var html = await _service.InBaoCaoAsync(loaiBaoCao, tuNgay, denNgay, donVi, top, soNgayTonLau, CurrentUserName);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html")
            };
            return response;
        }
    }
}
