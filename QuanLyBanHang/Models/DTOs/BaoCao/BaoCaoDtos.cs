using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyBanHang.Models.DTOs.BaoCao
{
    // =========================================================================
    // 4 REPORT DTOs KHỚP CHÍNH XÁC 100% VỚI CLASS DIAGRAM
    // =========================================================================

    // UC-58: Khớp dbo.fn_BaoCaoTongQuan & Class Diagram
    public class BaoCaoTongQuanDTO
    {
        public decimal TongDoanhThu { get; set; }
        public long TongDonHang { get; set; }
        public long DonChoXacNhan { get; set; }
        public long DonDaXacNhan { get; set; }
        public long DonDangChuanBi { get; set; }
        public long DonDangGiao { get; set; }
        public long DonHoanThanh { get; set; }
        public long DonDaHuy { get; set; }
        public long TongKhachHangMoi { get; set; }
        public long SoSanPhamSapHetHang { get; set; }
    }

    // UC-59: Khớp Class Diagram
    public class DoanhThuDTO
    {
        public string MocThoiGian { get; set; }
        public int SoDonHoanThanh { get; set; }
        public decimal DoanhThu { get; set; }
    }

    // UC-60: Khớp Class Diagram
    public class SanPhamBanChayDTO
    {
        public int ThuHang { get; set; }
        public int SanPhamId { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public int SoLuongBan { get; set; }
        public decimal DoanhThu { get; set; }
    }

    // UC-61: Khớp Class Diagram
    public class SanPhamTonLauDTO
    {
        public int SanPhamId { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public int SoLuongTon { get; set; }
        public DateTimeOffset? NgayBanGanNhat { get; set; }
        public int SoNgayTon { get; set; }
        public decimal GiaTriTon { get; set; }
    }

    // =========================================================================
    // QUERY & WRAPPER DTOs CHO UC-58 ĐẾN UC-62
    // =========================================================================

    public class BieuDoDoanhThuResponse
    {
        public BieuDoDoanhThuResponse()
        {
            DuLieu = new List<DoanhThuDTO>();
        }

        public List<DoanhThuDTO> DuLieu { get; set; }
        public decimal TongDoanhThu { get; set; }
        public decimal GiaTriCaoNhat { get; set; }
        public decimal GiaTriThapNhat { get; set; }
    }

    public class SanPhamBanChayQuery
    {
        public DateTimeOffset? TuNgay { get; set; }
        public DateTimeOffset? DenNgay { get; set; }
        public int? DanhMucId { get; set; }

        [Range(1, 100, ErrorMessage = "Số lượng top sản phẩm từ 1 đến 100.")]
        public int Top { get; set; } = 10;
    }

    public class SanPhamTonLauQuery
    {
        [Range(1, 3650, ErrorMessage = "Số ngày tồn lâu phải là số nguyên dương.")]
        public int SoNgayKhongBan { get; set; } = 30;
        public int? DanhMucId { get; set; }
    }
}
