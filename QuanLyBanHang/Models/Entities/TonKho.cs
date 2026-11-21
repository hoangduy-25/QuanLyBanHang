using System;
using System.Collections.Generic;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.TonKho (4 cột) - UC-20, UC-24 đến UC-27.
    /// Quan hệ 1-1 với SanPham được đảm bảo bởi UNIQUE(SanPhamId) trong SQL.
    /// </summary>
    public class TonKho
    {
        public TonKho()
        {
            SoLuongTon = 0;
            NguongToiThieu = 0;
            BienDongKhos = new HashSet<BienDongKho>();
        }

        public Guid Id { get; set; }
        public Guid SanPhamId { get; set; }
        public int SoLuongTon { get; set; }
        public int NguongToiThieu { get; set; }

        public virtual SanPham SanPham { get; set; }
        public virtual ICollection<BienDongKho> BienDongKhos { get; set; }
    }
}
