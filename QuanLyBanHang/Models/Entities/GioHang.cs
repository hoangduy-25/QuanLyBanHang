using System;
using System.Collections.Generic;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.GioHang (2 cột) - UC-29 đến UC-32.
    /// Quan hệ 1-1 với TaiKhoan được đảm bảo bởi UNIQUE(TaiKhoanId) trong SQL.
    /// </summary>
    public class GioHang
    {
        public GioHang()
        {
            ChiTietGioHangs = new HashSet<ChiTietGioHang>();
        }

        public int Id { get; set; }
        public int TaiKhoanId { get; set; }

        public virtual TaiKhoan TaiKhoan { get; set; }
        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }
    }
}
