using System;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.PhanHoiDanhGia (6 cột) - UC-51, UC-57.
    /// Quan hệ 1..0..1 với DanhGia được đảm bảo bởi UNIQUE(DanhGiaId) trong SQL.
    /// </summary>
    public class PhanHoiDanhGia
    {
        public PhanHoiDanhGia()
        {
            NgayTao = DateTimeOffset.Now;
        }

        public Guid Id { get; set; }
        public Guid DanhGiaId { get; set; }
        public Guid TaiKhoanId { get; set; }
        public string NoiDung { get; set; }
        public DateTimeOffset NgayTao { get; set; }
        public DateTimeOffset? NgayCapNhat { get; set; }

        public virtual DanhGia DanhGia { get; set; }
        public virtual TaiKhoan TaiKhoan { get; set; }
    }
}
