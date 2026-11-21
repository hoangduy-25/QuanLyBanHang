using System;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBanHang.Models.Enums;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.BienDongKho (12 cột) - UC-25, UC-26, UC-28, UC-37, UC-41.
    /// </summary>
    public class BienDongKho
    {
        public BienDongKho()
        {
            ThoiDiem = DateTimeOffset.Now;
        }

        public Guid Id { get; set; }
        public Guid TonKhoId { get; set; }
        public Guid? TaiKhoanId { get; set; }
        public Guid? DonHangId { get; set; }
        public string Loai { get; set; }
        public int SoLuongThayDoi { get; set; }
        public int SoLuongTruoc { get; set; }
        public int SoLuongSau { get; set; }
        public decimal? GiaNhap { get; set; }
        public string NhaCungCap { get; set; }
        public string LyDo { get; set; }
        public DateTimeOffset ThoiDiem { get; set; }

        [NotMapped]
        public LoaiBienDongKho LoaiEnum
        {
            get { return (LoaiBienDongKho)Enum.Parse(typeof(LoaiBienDongKho), Loai); }
            set { Loai = value.ToString(); }
        }

        public virtual TonKho TonKho { get; set; }
        public virtual TaiKhoan TaiKhoan { get; set; }
        public virtual DonHang DonHang { get; set; }
    }
}
