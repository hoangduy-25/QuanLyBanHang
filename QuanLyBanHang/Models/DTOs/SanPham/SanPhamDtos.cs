using System;
using System.ComponentModel.DataAnnotations;
using QuanLyBanHang.Models.DTOs.Common;

namespace QuanLyBanHang.Models.DTOs.SanPham
{
    // UC-17 & UC-19
    public class SanPhamFilterQuery : PaginationQuery
    {
        public string TuKhoa { get; set; }
        public int? DanhMucId { get; set; }
        public decimal? GiaTu { get; set; }
        public decimal? GiaDen { get; set; }
        public bool? HienThi { get; set; }
        public string SapXep { get; set; } = "NgayTaoDesc";
    }

    public class SanPhamListItemResponse
    {
        public int Id { get; set; }
        public int DanhMucId { get; set; }
        public string TenDanhMuc { get; set; }
        public string MaSanPham { get; set; }
        public string Ten { get; set; }
        public string HinhAnh { get; set; }
        public decimal GiaBan { get; set; }
        public int SoLuongTon { get; set; }
        public bool ConHang { get { return SoLuongTon > 0; } }
        public bool HienThi { get; set; }
        public double DiemTrungBinh { get; set; }
        public int SoLuotDanhGia { get; set; }
        public DateTimeOffset NgayTao { get; set; }
    }

    // UC-18: GET /api/san-pham/{id}
    public class SanPhamChiTietResponse : SanPhamListItemResponse
    {
        public string MoTa { get; set; }
        public int NguongToiThieu { get; set; }
    }

    // UC-20: POST /api/san-pham
    public class TaoSanPhamRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public int DanhMucId { get; set; }

        [Required(ErrorMessage = "Mã sản phẩm không được để trống.")]
        [StringLength(50)]
        public string MaSanPham { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        [StringLength(200)]
        public string Ten { get; set; }

        [StringLength(2000)]
        public string MoTa { get; set; } = string.Empty;

        [StringLength(500)]
        public string HinhAnh { get; set; }

        [Range(0.01, 999999999999.99, ErrorMessage = "Giá bán phải lớn hơn 0.")]
        public decimal GiaBan { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng ban đầu không được âm.")]
        public int SoLuongBanDau { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Ngưỡng tối thiểu không được âm.")]
        public int NguongToiThieu { get; set; }

        public bool HienThi { get; set; } = true;
    }

    // UC-21: PUT /api/san-pham/{id} (Không sửa tồn kho tại đây - sửa qua module Kho)
    public class SuaSanPhamRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public int DanhMucId { get; set; }

        [Required(ErrorMessage = "Mã sản phẩm không được để trống.")]
        [StringLength(50)]
        public string MaSanPham { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        [StringLength(200)]
        public string Ten { get; set; }

        [StringLength(2000)]
        public string MoTa { get; set; } = string.Empty;

        [StringLength(500)]
        public string HinhAnh { get; set; }

        [Range(0.01, 999999999999.99, ErrorMessage = "Giá bán phải lớn hơn 0.")]
        public decimal GiaBan { get; set; }

        public bool HienThi { get; set; } = true;
    }

    // UC-23: PATCH /api/san-pham/{id}/an-hien
    public class AnHienSanPhamRequest
    {
        public bool HienThi { get; set; }
    }
}
