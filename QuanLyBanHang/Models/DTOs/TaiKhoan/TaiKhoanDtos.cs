using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuanLyBanHang.Models.DTOs.Common;

namespace QuanLyBanHang.Models.DTOs.TaiKhoan
{
    // UC-06: GET /api/tai-khoan/ho-so
    public class HoSoResponse
    {
        public Guid Id { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string VaiTro { get; set; }
        public string TrangThai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string AnhDaiDien { get; set; }
        public bool DaThietLapCauHoiBaoMat { get; set; }
        public string CauHoiBaoMat { get; set; }
        public DateTimeOffset NgayTao { get; set; }
    }

    // UC-06: PUT /api/tai-khoan/ho-so
    public class CapNhatHoSoRequest
    {
        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự.")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [RegularExpression(@"^(0|\+84)[0-9]{9,10}$", ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [StringLength(20)]
        public string SoDienThoai { get; set; }

        public DateTime? NgaySinh { get; set; }

        [RegularExpression(@"^(Nam|Nu|Khac)$", ErrorMessage = "Giới tính phải là Nam, Nu hoặc Khac.")]
        public string GioiTinh { get; set; }

        [StringLength(500)]
        public string AnhDaiDien { get; set; }
    }

    // UC-08 & UC-09: GET /api/tai-khoan & /api/tai-khoan/tim-kiem
    public class TaiKhoanFilterQuery : PaginationQuery
    {
        public string TuKhoa { get; set; }
        public string VaiTro { get; set; }
        public string TrangThai { get; set; }
        public string SapXep { get; set; } = "NgayTaoDesc";
    }

    public class TaiKhoanListItemResponse
    {
        public Guid Id { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string VaiTro { get; set; }
        public string TrangThai { get; set; }
        public string LyDoKhoa { get; set; }
        public DateTimeOffset NgayTao { get; set; }
    }

    // UC-10: PATCH /api/tai-khoan/{id}/khoa
    public class KhoaTaiKhoanRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập lý do khóa tài khoản.")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Lý do khóa từ 3 đến 500 ký tự.")]
        public string LyDo { get; set; }
    }

    public class KhoaMoTaiKhoanResponse
    {
        public Guid TaiKhoanId { get; set; }
        public string TrangThaiMoi { get; set; }
        public string LyDo { get; set; }
        public int SoDonDangXuLy { get; set; }
        public string CanhBao { get; set; }
    }

    public class LichSuKhoaResponse
    {
        public Guid Id { get; set; }
        public Guid TaiKhoanId { get; set; }
        public Guid QuanLyId { get; set; }
        public string TenQuanLy { get; set; }
        public string TrangThaiMoi { get; set; }
        public string LyDo { get; set; }
        public DateTimeOffset ThoiDiem { get; set; }
    }
}
