using System;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.ThichDanhGia (3 cột, khóa chính kết hợp TaiKhoanId + DanhGiaId) - UC-55.
    /// </summary>
    public class ThichDanhGia
    {
        public ThichDanhGia()
        {
            NgayTao = DateTimeOffset.Now;
        }

        public Guid TaiKhoanId { get; set; }
        public Guid DanhGiaId { get; set; }
        public DateTimeOffset NgayTao { get; set; }

        public virtual TaiKhoan TaiKhoan { get; set; }
        public virtual DanhGia DanhGia { get; set; }
    }
}
