using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBanHang.Models.Enums;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.MaGiamGia (13 cột) - UC-35, UC-45 đến UC-50.
    /// </summary>
    public class MaGiamGia
    {
        public MaGiamGia()
        {
            DonToiThieu = 0;
            SoLuotDaDung = 0;
            HienThi = true;
            IsDeleted = false;
            DonHangs = new HashSet<DonHang>();
        }

        public int Id { get; set; }
        public string Ma { get; set; }
        public string MoTa { get; set; }
        public string Loai { get; set; }
        public decimal GiaTri { get; set; }
        public decimal? GiamToiDa { get; set; }
        public decimal DonToiThieu { get; set; }
        public DateTimeOffset NgayBatDau { get; set; }
        public DateTimeOffset NgayKetThuc { get; set; }
        public int SoLuotToiDa { get; set; }
        public int SoLuotDaDung { get; set; }
        public bool HienThi { get; set; }
        public bool IsDeleted { get; set; }

        [NotMapped]
        public LoaiGiamGia LoaiEnum
        {
            get { return (LoaiGiamGia)Enum.Parse(typeof(LoaiGiamGia), Loai); }
            set { Loai = value.ToString(); }
        }

        public virtual ICollection<DonHang> DonHangs { get; set; }
    }
}
