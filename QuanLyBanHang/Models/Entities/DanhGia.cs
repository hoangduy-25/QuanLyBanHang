using System;
using System.Collections.Generic;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.DanhGia (12 cột) - UC-51 đến UC-56.
    /// </summary>
    public class DanhGia
    {
        public DanhGia()
        {
            DaChinhSua = false;
            IsDeleted = false;
            NgayTao = DateTimeOffset.Now;

            PhanHoiDanhGias = new HashSet<PhanHoiDanhGia>();
            ThichDanhGias = new HashSet<ThichDanhGia>();
        }

        public Guid Id { get; set; }
        public Guid TaiKhoanId { get; set; }
        public Guid SanPhamId { get; set; }
        public Guid DonHangId { get; set; }
        public int SoSao { get; set; }
        public string NoiDung { get; set; }
        public string HinhAnh { get; set; }
        public bool DaChinhSua { get; set; }
        public DateTimeOffset NgayTao { get; set; }
        public DateTimeOffset? NgayCapNhat { get; set; }
        public string LyDoXoa { get; set; }
        public bool IsDeleted { get; set; }

        public virtual TaiKhoan TaiKhoan { get; set; }
        public virtual SanPham SanPham { get; set; }
        public virtual DonHang DonHang { get; set; }
        public virtual ICollection<PhanHoiDanhGia> PhanHoiDanhGias { get; set; }
        public virtual ICollection<ThichDanhGia> ThichDanhGias { get; set; }
    }
}
