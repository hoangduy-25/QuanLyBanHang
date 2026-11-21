using System;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.ChiTietGioHang (5 cột) - UC-29 đến UC-32.
    /// </summary>
    public class ChiTietGioHang
    {
        public Guid Id { get; set; }
        public Guid GioHangId { get; set; }
        public Guid SanPhamId { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGiaLucThem { get; set; }

        public virtual GioHang GioHang { get; set; }
        public virtual SanPham SanPham { get; set; }
    }
}
