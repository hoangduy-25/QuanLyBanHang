using System;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.ChiTietDonHang (6 cột) - UC-37, UC-39, UC-60.
    /// ThanhTien là Computed Column trong SQL Server (DonGia * SoLuong).
    /// </summary>
    public class ChiTietDonHang
    {
        public int Id { get; set; }
        public int DonHangId { get; set; }
        public int SanPhamId { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; }

        public virtual DonHang DonHang { get; set; }
        public virtual SanPham SanPham { get; set; }
    }
}
