using System;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBanHang.Models.Enums;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.LichSuDonHang (6 cột) - UC-37, UC-41 đến UC-44.
    /// </summary>
    public class LichSuDonHang
    {
        public LichSuDonHang()
        {
            ThoiDiem = DateTimeOffset.Now;
        }

        public Guid Id { get; set; }
        public Guid DonHangId { get; set; }
        public Guid? TaiKhoanId { get; set; }
        public string TrangThai { get; set; }
        public DateTimeOffset ThoiDiem { get; set; }
        public string GhiChu { get; set; }

        [NotMapped]
        public TrangThaiDonHang TrangThaiEnum
        {
            get { return (TrangThaiDonHang)Enum.Parse(typeof(TrangThaiDonHang), TrangThai); }
            set { TrangThai = value.ToString(); }
        }

        public virtual DonHang DonHang { get; set; }
        public virtual TaiKhoan TaiKhoan { get; set; }
    }
}
