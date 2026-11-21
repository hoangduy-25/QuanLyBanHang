using System;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.ChiTietDonHang (6 cột) - UC-37, UC-39, UC-60.
    /// ThanhTien là Computed Column trong SQL Server (DonGia * SoLuong).
    /// </summary>
    public class ChiTietDonHang
    {
        public Guid Id { get; set; }
        public Guid DonHangId { get; set; }
        public Guid SanPhamId { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; }

        public virtual DonHang DonHang { get; set; }
        public virtual SanPham SanPham { get; set; }
    }
}
