using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyBanHang.Models.DTOs.XacThuc
{
    // UC-01: POST /api/xac-thuc/dang-ky
    public class DangKyRequest
    {
        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự.")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(254)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [RegularExpression(@"^(0|\+84)[0-9]{9,10}$", ErrorMessage = "Số điện thoại không đúng định dạng Việt Nam.")]
        [StringLength(20)]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống.")]
        [Compare("MatKhau", ErrorMessage = "Xác nhận mật khẩu không khớp.")]
        public string XacNhanMatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn câu hỏi bảo mật.")]
        [StringLength(255)]
        public string CauHoiBaoMat { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập câu trả lời bảo mật.")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Câu trả lời bảo mật tối thiểu 2 ký tự.")]
        public string CauTraLoiBaoMat { get; set; }
    }

    // UC-02: POST /api/xac-thuc/dang-nhap
    public class DangNhapRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập email hoặc số điện thoại.")]
        [StringLength(254)]
        public string TaiKhoan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        public string MatKhau { get; set; }
    }

    public class DangNhapResponse
    {
        public string AccessToken { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public int TaiKhoanId { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string VaiTro { get; set; }
        public string RedirectUrl { get; set; }
    }

    // UC-03: GET /api/xac-thuc/cau-hoi-bao-mat
    public class CauHoiBaoMatResponse
    {
        public string EmailOrPhone { get; set; }
        public string CauHoiBaoMat { get; set; }
    }

    // UC-03: POST /api/xac-thuc/quen-mat-khau
    public class QuenMatKhauRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập email hoặc số điện thoại.")]
        [StringLength(254)]
        public string TaiKhoan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập câu trả lời bảo mật.")]
        public string CauTraLoiBaoMat { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu mới phải có ít nhất 8 ký tự.")]
        public string MatKhauMoi { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [Compare("MatKhauMoi", ErrorMessage = "Xác nhận mật khẩu mới không khớp.")]
        public string XacNhanMatKhauMoi { get; set; }
    }

    // UC-04: POST /api/xac-thuc/doi-mat-khau
    public class DoiMatKhauRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ.")]
        public string MatKhauCu { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu mới phải có ít nhất 8 ký tự.")]
        public string MatKhauMoi { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [Compare("MatKhauMoi", ErrorMessage = "Xác nhận mật khẩu mới không khớp.")]
        public string XacNhanMatKhauMoi { get; set; }
    }

    // UC-05: PUT /api/xac-thuc/cap-nhat-cau-hoi-bao-mat
    public class CapNhatCauHoiBaoMatRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn câu hỏi bảo mật.")]
        [StringLength(255)]
        public string CauHoiBaoMat { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập câu trả lời bảo mật.")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Câu trả lời bảo mật quá ngắn (tối thiểu 2 ký tự).")]
        public string CauTraLoiBaoMat { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại để xác nhận.")]
        public string MatKhauHienTai { get; set; }
    }
}
