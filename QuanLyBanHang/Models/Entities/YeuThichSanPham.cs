using System;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.YeuThichSanPham (3 cột, khóa chính kết hợp TaiKhoanId + SanPhamId) - UC-33.
    /// </summary>
    public class YeuThichSanPham
    {
        public YeuThichSanPham()
        {
            NgayTao = DateTimeOffset.Now;
        }

        public Guid TaiKhoanId { get; set; }
        public Guid SanPhamId { get; set; }
        public DateTimeOffset NgayTao { get; set; }

        public virtual TaiKhoan TaiKhoan { get; set; }
        public virtual SanPham SanPham { get; set; }
    }
}
