using System;
using System.Collections.Generic;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.SanPham (10 cột) - UC-17 đến UC-23.
    /// </summary>
    public class SanPham
    {
        public SanPham()
        {
            MoTa = string.Empty;
            HienThi = true;
            IsDeleted = false;
            NgayTao = DateTimeOffset.Now;

            TonKhos = new HashSet<TonKho>();
            ChiTietGioHangs = new HashSet<ChiTietGioHang>();
            YeuThichSanPhams = new HashSet<YeuThichSanPham>();
            ChiTietDonHangs = new HashSet<ChiTietDonHang>();
            DanhGias = new HashSet<DanhGia>();
        }

        public Guid Id { get; set; }
        public Guid DanhMucId { get; set; }
        public string MaSanPham { get; set; }
        public string Ten { get; set; }
        public string MoTa { get; set; }
        public string HinhAnh { get; set; }
        public decimal GiaBan { get; set; }
        public bool HienThi { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset NgayTao { get; set; }

        public virtual DanhMuc DanhMuc { get; set; }
        public virtual ICollection<TonKho> TonKhos { get; set; }
        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }
        public virtual ICollection<YeuThichSanPham> YeuThichSanPhams { get; set; }
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public virtual ICollection<DanhGia> DanhGias { get; set; }
    }
}
