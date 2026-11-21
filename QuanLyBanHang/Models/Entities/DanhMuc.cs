using System;
using System.Collections.Generic;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.DanhMuc (7 cột) - UC-11 đến UC-16.
    /// </summary>
    public class DanhMuc
    {
        public DanhMuc()
        {
            HienThi = true;
            IsDeleted = false;
            NgayTao = DateTimeOffset.Now;
            SanPhams = new HashSet<SanPham>();
        }

        public Guid Id { get; set; }
        public string Ten { get; set; }
        public string MoTa { get; set; }
        public string HinhAnh { get; set; }
        public bool HienThi { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset NgayTao { get; set; }

        public virtual ICollection<SanPham> SanPhams { get; set; }
    }
}
