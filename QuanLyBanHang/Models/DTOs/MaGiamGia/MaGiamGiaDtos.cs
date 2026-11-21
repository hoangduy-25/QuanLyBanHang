using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyBanHang.Models.DTOs.MaGiamGia
{
    // UC-45, UC-46, UC-47
    public class MaGiamGiaResponse
    {
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
        public int SoLuotConLai { get { return Math.Max(0, SoLuotToiDa - SoLuotDaDung); } }
        public bool HienThi { get; set; }
    }

    // UC-48: POST /api/ma-giam-gia
    public class TaoMaGiamGiaRequest
    {
        [Required(ErrorMessage = "Mã giảm giá không được để trống.")]
        [StringLength(50, MinimumLength = 2)]
        public string Ma { get; set; }

        [StringLength(500)]
        public string MoTa { get; set; }

        [Required]
        [RegularExpression(@"^(SoTien|PhanTram)$", ErrorMessage = "Loại giảm giá phải là SoTien hoặc PhanTram.")]
        public string Loai { get; set; }

        [Range(0.01, 999999999999.99, ErrorMessage = "Giá trị giảm phải lớn hơn 0.")]
        public decimal GiaTri { get; set; }

        [Range(0.01, 999999999999.99, ErrorMessage = "Mức giảm tối đa phải lớn hơn 0.")]
        public decimal? GiamToiDa { get; set; }

        [Range(0, 999999999999.99, ErrorMessage = "Giá trị đơn tối thiểu không được âm.")]
        public decimal DonToiThieu { get; set; }

        [Required]
        public DateTimeOffset NgayBatDau { get; set; }

        [Required]
        public DateTimeOffset NgayKetThuc { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượt sử dụng tối đa phải lớn hơn 0.")]
        public int SoLuotToiDa { get; set; }

        public bool HienThi { get; set; } = true;
    }

    // UC-49: PUT /api/ma-giam-gia/{id}
    public class SuaMaGiamGiaRequest : TaoMaGiamGiaRequest
    {
    }
}
