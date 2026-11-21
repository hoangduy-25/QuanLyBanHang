using System.Threading.Tasks;
using System.Web.Http;
using QuanLyBanHang.Infrastructure.Auth;
using QuanLyBanHang.Infrastructure.Filters;
using QuanLyBanHang.Models.DTOs.XacThuc;
using QuanLyBanHang.Services.Implementations;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Controllers.Api
{
    [RoutePrefix("api/xac-thuc")]
    public class XacThucController : BaseApiController
    {
        private readonly IXacThucService _service;

        public XacThucController()
        {
            _service = new XacThucService(UnitOfWork);
        }

        // UC-01: POST /api/xac-thuc/dang-ky (Công khai)
        [HttpPost, Route("dang-ky")]
        public async Task<IHttpActionResult> DangKy([FromBody] DangKyRequest request)
        {
            var id = await _service.DangKyAsync(request);
            return CreatedApi(new { taiKhoanId = id }, "Đăng ký tài khoản thành công.");
        }

        // UC-02: POST /api/xac-thuc/dang-nhap (Công khai)
        [HttpPost, Route("dang-nhap")]
        public async Task<IHttpActionResult> DangNhap([FromBody] DangNhapRequest request)
        {
            var res = await _service.DangNhapAsync(request);
            res.AccessToken = JwtTokenHelper.GenerateToken(res.TaiKhoanId, res.HoTen, res.Email, res.VaiTro, res.ExpiresAt);
            return OkApi(res, "Đăng nhập thành công.");
        }

        // UC-03: GET /api/xac-thuc/cau-hoi-bao-mat?taiKhoan=... (Công khai)
        [HttpGet, Route("cau-hoi-bao-mat")]
        public async Task<IHttpActionResult> LayCauHoiBaoMat([FromUri] string taiKhoan)
        {
            var res = await _service.LayCauHoiBaoMatAsync(taiKhoan);
            return OkApi(res);
        }

        // UC-03: POST /api/xac-thuc/quen-mat-khau (Công khai)
        [HttpPost, Route("quen-mat-khau")]
        public async Task<IHttpActionResult> QuenMatKhau([FromBody] QuenMatKhauRequest request)
        {
            await _service.QuenMatKhauAsync(request);
            return OkApi<object>(null, "Đặt lại mật khẩu thành công.");
        }

        // UC-04: POST /api/xac-thuc/doi-mat-khau (Khách hàng, Nhân viên, Quản lý)
        [HttpPost, Route("doi-mat-khau")]
        [JwtAuthorize(Roles = "KhachHang,NhanVien,QuanLy")]
        public async Task<IHttpActionResult> DoiMatKhau([FromBody] DoiMatKhauRequest request)
        {
            await _service.DoiMatKhauAsync(CurrentUserId.Value, request);
            return OkApi<object>(null, "Đổi mật khẩu thành công.");
        }

        // UC-05: PUT /api/xac-thuc/cap-nhat-cau-hoi-bao-mat (Khách hàng, Nhân viên, Quản lý)
        [HttpPut, Route("cap-nhat-cau-hoi-bao-mat")]
        [JwtAuthorize(Roles = "KhachHang,NhanVien,QuanLy")]
        public async Task<IHttpActionResult> CapNhatCauHoiBaoMat([FromBody] CapNhatCauHoiBaoMatRequest request)
        {
            await _service.CapNhatCauHoiBaoMatAsync(CurrentUserId.Value, request);
            return OkApi<object>(null, "Cập nhật câu hỏi bảo mật thành công.");
        }
    }
}
