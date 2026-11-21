using System;
using System.ComponentModel.DataAnnotations;
using QuanLyBanHang.Models.DTOs.Common;

namespace QuanLyBanHang.Models.DTOs.DanhGia
{
    // UC-51 & UC-52: GET /api/danh-gia & /api/danh-gia/loc
    public class DanhGiaFilterQuery : PaginationQuery
    {
        public Guid? SanPhamId { get; set; }
        [Range(1, 5)]
        public int? SoSao { get; set; }
        public bool? CoHinhAnh { get; set; }
        public bool? CoPhanHoi { get; set; }
        public DateTimeOffset? TuNgay { get; set; }
        public DateTimeOffset? DenNgay { get; set; }
    }

    public class PhanHoiDanhGiaResponse
    {
        public Guid Id { get; set; }
        public Guid TaiKhoanId { get; set; }
        public string TenNguoiTraLoi { get; set; }
        public string NoiDung { get; set; }
        public DateTimeOffset NgayTao { get; set; }
        public DateTimeOffset? NgayCapNhat { get; set; }
    }

    public class DanhGiaResponse
    {
        public Guid Id { get; set; }
        public Guid TaiKhoanId { get; set; }
        public string TenNguoiDanhGia { get; set; }
        public Guid SanPhamId { get; set; }
        public string TenSanPham { get; set; }
        public Guid DonHangId { get; set; }
        public int SoSao { get; set; }
        public string NoiDung { get; set; }
        public string HinhAnh { get; set; }
        public bool DaChinhSua { get; set; }
        public int SoLuotThich { get; set; }
        public bool DaThich { get; set; }
        public DateTimeOffset NgayTao { get; set; }
        public DateTimeOffset? NgayCapNhat { get; set; }
        public PhanHoiDanhGiaResponse PhanHoi { get; set; }
    }

    // UC-53: POST /api/danh-gia
    public class TaoDanhGiaRequest
    {
        [Required]
        public Guid SanPhamId { get; set; }

        [Required]
        public Guid DonHangId { get; set; }

        [Range(1, 5, ErrorMessage = "Số sao đánh giá phải từ 1 đến 5.")]
        public int SoSao { get; set; }

        [Required(ErrorMessage = "Nội dung đánh giá không được để trống.")]
        [StringLength(2000, MinimumLength = 2, ErrorMessage = "Nội dung đánh giá tối đa 2000 ký tự.")]
        public string NoiDung { get; set; }

        [StringLength(500)]
        public string HinhAnh { get; set; }
    }

    // UC-54: PUT /api/danh-gia/{id}
    public class SuaDanhGiaRequest
    {
        [Range(1, 5, ErrorMessage = "Số sao đánh giá phải từ 1 đến 5.")]
        public int SoSao { get; set; }

        [Required(ErrorMessage = "Nội dung đánh giá không được để trống.")]
        [StringLength(2000, MinimumLength = 2, ErrorMessage = "Nội dung đánh giá tối đa 2000 ký tự.")]
        public string NoiDung { get; set; }

        [StringLength(500)]
        public string HinhAnh { get; set; }
    }

    // UC-55: POST /api/danh-gia/{id}/thich
    public class ThichDanhGiaResponse
    {
        public Guid DanhGiaId { get; set; }
        public bool DaThich { get; set; }
        public int SoLuotThich { get; set; }
    }

    // UC-56: DELETE /api/danh-gia/{id}
    public class XoaDanhGiaRequest
    {
        [StringLength(500)]
        public string LyDoXoa { get; set; }
    }

    // UC-57: POST /api/danh-gia/{id}/tra-loi
    public class TraLoiDanhGiaRequest
    {
        [Required(ErrorMessage = "Nội dung phản hồi không được để trống.")]
        [StringLength(2000, MinimumLength = 2, ErrorMessage = "Nội dung phản hồi tối đa 2000 ký tự.")]
        public string NoiDung { get; set; }
    }
}
