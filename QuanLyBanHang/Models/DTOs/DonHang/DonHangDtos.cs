using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuanLyBanHang.Models.DTOs.Common;

namespace QuanLyBanHang.Models.DTOs.DonHang
{
    // UC-38: GET /api/don-hang
    public class DonHangFilterQuery : PaginationQuery
    {
        public string MaDon { get; set; }
        public string TrangThai { get; set; }
        public DateTimeOffset? TuNgay { get; set; }
        public DateTimeOffset? DenNgay { get; set; }
    }

    public class DonHangListItemResponse
    {
        public int Id { get; set; }
        public string MaDon { get; set; }
        public int TaiKhoanId { get; set; }
        public string TenKhachHang { get; set; }
        public DateTimeOffset NgayDat { get; set; }
        public string TrangThai { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public bool DaThanhToan { get; set; }
        public decimal TongThanhToan { get; set; }
        public int SoMatHang { get; set; }
    }

    // UC-39: GET /api/don-hang/{id}
    public class DonHangChiTietResponse
    {
        public DonHangChiTietResponse()
        {
            ChiTiets = new List<ChiTietDonHangItemResponse>();
            HanhTrinh = new List<LichSuDonHangItemResponse>();
        }

        public int Id { get; set; }
        public string MaDon { get; set; }
        public int TaiKhoanId { get; set; }
        public string TenKhachHang { get; set; }
        public DateTimeOffset NgayDat { get; set; }
        public string TrangThai { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public bool DaThanhToan { get; set; }
        public string TenNguoiNhan { get; set; }
        public string SoDienThoaiNhan { get; set; }
        public string DiaChiGiaoHang { get; set; }
        public string MaGiamGia { get; set; }
        public decimal TamTinh { get; set; }
        public decimal TienGiam { get; set; }
        public decimal TongThanhToan { get; set; }
        public string LyDoHuy { get; set; }
        public List<ChiTietDonHangItemResponse> ChiTiets { get; set; }
        public List<LichSuDonHangItemResponse> HanhTrinh { get; set; }
    }

    public class ChiTietDonHangItemResponse
    {
        public int SanPhamId { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; }
    }

    // UC-42: GET /api/don-hang/{id}/hanh-trinh
    public class LichSuDonHangItemResponse
    {
        public int Id { get; set; }
        public string TrangThai { get; set; }
        public DateTimeOffset ThoiDiem { get; set; }
        public string NguoiThucHien { get; set; }
        public string GhiChu { get; set; }
    }

    // UC-41: PATCH /api/don-hang/{id}/huy
    public class HuyDonHangRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập lý do hủy đơn hàng.")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Lý do hủy từ 3 đến 500 ký tự.")]
        public string LyDoHuy { get; set; }
    }

    // UC-44: PATCH /api/don-hang/{id}/trang-thai
    public class CapNhatTrangThaiDonHangRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn trạng thái mới.")]
        [RegularExpression(@"^(DangChuanBi|DangGiao|HoanThanh)$", ErrorMessage = "Trạng thái kế tiếp phải là DangChuanBi, DangGiao hoặc HoanThanh.")]
        public string TrangThaiMoi { get; set; }

        [StringLength(500)]
        public string GhiChu { get; set; }
    }
}
