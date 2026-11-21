using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyBanHang.Models.DTOs.GioHang
{
    // UC-29: GET /api/gio-hang
    public class GioHangResponse
    {
        public GioHangResponse()
        {
            Items = new List<ChiTietGioHangItemResponse>();
        }

        public int GioHangId { get; set; }
        public List<ChiTietGioHangItemResponse> Items { get; set; }
        public int TongSoLuongKhaDung { get; set; }
        public decimal TongTienTamTinh { get; set; }
    }

    public class ChiTietGioHangItemResponse
    {
        public int SanPhamId { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public decimal DonGiaHienTai { get; set; }
        public decimal DonGiaLucThem { get; set; }
        public bool GiaDaThayDoi { get { return DonGiaHienTai != DonGiaLucThem; } }
        public int SoLuong { get; set; }
        public int SoLuongTon { get; set; }
        public bool KhaDung { get; set; }
        public string LyDoKhongKhaDung { get; set; }
        public decimal ThanhTien { get { return KhaDung ? DonGiaHienTai * SoLuong : 0; } }
    }

    // UC-30: POST /api/gio-hang/san-pham
    public class ThemVaoGioRequest
    {
        [Required]
        public int SanPhamId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int SoLuong { get; set; } = 1;
    }

    // UC-31: PUT /api/gio-hang/san-pham/{sanPhamId}
    public class SuaSoLuongGioRequest
    {
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không hợp lệ.")]
        public int SoLuong { get; set; }
    }

    // UC-33: GET /api/gio-hang/yeu-thich
    public class YeuThichItemResponse
    {
        public int SanPhamId { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public decimal GiaBan { get; set; }
        public bool ConHang { get; set; }
        public bool HienThi { get; set; }
        public DateTimeOffset NgayTao { get; set; }
    }
}
