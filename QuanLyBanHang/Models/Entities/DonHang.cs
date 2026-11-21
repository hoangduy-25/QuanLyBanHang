using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBanHang.Models.Enums;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.DonHang (15 cột) - UC-34 đến UC-44.
    /// TongThanhToan là Computed Column trong SQL Server (TamTinh - TienGiam).
    /// </summary>
    public class DonHang
    {
        public DonHang()
        {
            NgayDat = DateTimeOffset.Now;
            TrangThai = TrangThaiDonHang.ChoXacNhan.ToString();
            DaThanhToan = false;
            TienGiam = 0;

            ChiTietDonHangs = new HashSet<ChiTietDonHang>();
            LichSuDonHangs = new HashSet<LichSuDonHang>();
            BienDongKhos = new HashSet<BienDongKho>();
            DanhGias = new HashSet<DanhGia>();
        }

        public int Id { get; set; }
        public int TaiKhoanId { get; set; }
        public int? MaGiamGiaId { get; set; }
        public string MaDon { get; set; }
        public DateTimeOffset NgayDat { get; set; }
        public string TrangThai { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public bool DaThanhToan { get; set; }
        public string TenNguoiNhan { get; set; }
        public string SoDienThoaiNhan { get; set; }
        public string DiaChiGiaoHang { get; set; }
        public decimal TamTinh { get; set; }
        public decimal TienGiam { get; set; }
        public decimal TongThanhToan { get; set; }
        public string LyDoHuy { get; set; }

        [NotMapped]
        public TrangThaiDonHang TrangThaiEnum
        {
            get { return (TrangThaiDonHang)Enum.Parse(typeof(TrangThaiDonHang), TrangThai); }
            set { TrangThai = value.ToString(); }
        }

        [NotMapped]
        public PhuongThucThanhToan PhuongThucThanhToanEnum
        {
            get { return (PhuongThucThanhToan)Enum.Parse(typeof(PhuongThucThanhToan), PhuongThucThanhToan); }
            set { PhuongThucThanhToan = value.ToString(); }
        }

        public virtual TaiKhoan TaiKhoan { get; set; }
        public virtual MaGiamGia MaGiamGia { get; set; }
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public virtual ICollection<LichSuDonHang> LichSuDonHangs { get; set; }
        public virtual ICollection<BienDongKho> BienDongKhos { get; set; }
        public virtual ICollection<DanhGia> DanhGias { get; set; }
    }
}
