using System;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.ChiTietGioHang (5 cột) - UC-29 đến UC-32.
    /// </summary>
    public class ChiTietGioHang
    {
        public int Id { get; set; }
        public int GioHangId { get; set; }
        public int SanPhamId { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGiaLucThem { get; set; }

        public virtual GioHang GioHang { get; set; }
        public virtual SanPham SanPham { get; set; }
    }
}
