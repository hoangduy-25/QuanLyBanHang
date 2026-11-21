using System;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBanHang.Models.Enums;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.LichSuKhoaTaiKhoan (6 cột) - UC-10: Nhật ký khóa/mở khóa tài khoản.
    /// </summary>
    public class LichSuKhoaTaiKhoan
    {
        public LichSuKhoaTaiKhoan()
        {
            ThoiDiem = DateTimeOffset.Now;
        }

        public Guid Id { get; set; }
        public Guid TaiKhoanId { get; set; }
        public Guid QuanLyId { get; set; }
        public string TrangThaiMoi { get; set; }
        public string LyDo { get; set; }
        public DateTimeOffset ThoiDiem { get; set; }

        [NotMapped]
        public TrangThaiTaiKhoan TrangThaiMoiEnum
        {
            get { return (TrangThaiTaiKhoan)Enum.Parse(typeof(TrangThaiTaiKhoan), TrangThaiMoi); }
            set { TrangThaiMoi = value.ToString(); }
        }

        public virtual TaiKhoan TaiKhoan { get; set; }
        public virtual TaiKhoan QuanLy { get; set; }
    }
}
