using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBanHang.Models.Enums;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.TaiKhoan (16 cột) - UC-01 đến UC-10.
    /// Lưu ý: NgaySinh sử dụng DateTime? thay cho DateOnly? để tương thích .NET Framework 4.7.2.
    /// Các cột enum (VaiTro, TrangThai, GioiTinh) lưu chuỗi varchar/nvarchar trong SQL Server
    /// nên thuộc tính ánh xạ trực tiếp là string để LINQ to Entities dịch thẳng sang SQL.
    /// </summary>
    public class TaiKhoan
    {
        public TaiKhoan()
        {
            VaiTro = Enums.VaiTro.KhachHang.ToString();
            TrangThai = TrangThaiTaiKhoan.HoatDong.ToString();
            SoLanDangNhapSai = 0;
            NgayTao = DateTimeOffset.Now;

            DiaChiNhanHangs = new HashSet<DiaChiNhanHang>();
            LichSuBiKhoas = new HashSet<LichSuKhoaTaiKhoan>();
            LichSuThucHienKhoas = new HashSet<LichSuKhoaTaiKhoan>();
            GioHangs = new HashSet<GioHang>();
            YeuThichSanPhams = new HashSet<YeuThichSanPham>();
            DonHangs = new HashSet<DonHang>();
            LichSuDonHangs = new HashSet<LichSuDonHang>();
            BienDongKhos = new HashSet<BienDongKho>();
            DanhGias = new HashSet<DanhGia>();
            PhanHoiDanhGias = new HashSet<PhanHoiDanhGia>();
            ThichDanhGias = new HashSet<ThichDanhGia>();
        }

        public int Id { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string MatKhauHash { get; set; }
        public string VaiTro { get; set; }
        public string TrangThai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string AnhDaiDien { get; set; }
        public string CauHoiBaoMat { get; set; }
        public string CauTraLoiHash { get; set; }
        public string LyDoKhoa { get; set; }
        public int SoLanDangNhapSai { get; set; }
        public DateTimeOffset? KhoaTamDen { get; set; }
        public DateTimeOffset NgayTao { get; set; }

        [NotMapped]
        public VaiTro VaiTroEnum
        {
            get { return (VaiTro)Enum.Parse(typeof(VaiTro), VaiTro); }
            set { VaiTro = value.ToString(); }
        }

        [NotMapped]
        public TrangThaiTaiKhoan TrangThaiEnum
        {
            get { return (TrangThaiTaiKhoan)Enum.Parse(typeof(TrangThaiTaiKhoan), TrangThai); }
            set { TrangThai = value.ToString(); }
        }

        [NotMapped]
        public GioiTinh? GioiTinhEnum
        {
            get { return string.IsNullOrWhiteSpace(GioiTinh) ? (GioiTinh?)null : (GioiTinh)Enum.Parse(typeof(GioiTinh), GioiTinh); }
            set { GioiTinh = value.HasValue ? value.Value.ToString() : null; }
        }

        // Navigation properties
        public virtual ICollection<DiaChiNhanHang> DiaChiNhanHangs { get; set; }
        public virtual ICollection<LichSuKhoaTaiKhoan> LichSuBiKhoas { get; set; }
        public virtual ICollection<LichSuKhoaTaiKhoan> LichSuThucHienKhoas { get; set; }
        public virtual ICollection<GioHang> GioHangs { get; set; }
        public virtual ICollection<YeuThichSanPham> YeuThichSanPhams { get; set; }
        public virtual ICollection<DonHang> DonHangs { get; set; }
        public virtual ICollection<LichSuDonHang> LichSuDonHangs { get; set; }
        public virtual ICollection<BienDongKho> BienDongKhos { get; set; }
        public virtual ICollection<DanhGia> DanhGias { get; set; }
        public virtual ICollection<PhanHoiDanhGia> PhanHoiDanhGias { get; set; }
        public virtual ICollection<ThichDanhGia> ThichDanhGias { get; set; }
    }
}
