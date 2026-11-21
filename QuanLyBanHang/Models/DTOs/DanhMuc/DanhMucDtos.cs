using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyBanHang.Models.DTOs.DanhMuc
{
    // UC-11 & UC-12
    public class DanhMucResponse
    {
        public Guid Id { get; set; }
        public string Ten { get; set; }
        public string MoTa { get; set; }
        public string HinhAnh { get; set; }
        public bool HienThi { get; set; }
        public int SoSanPham { get; set; }
        public DateTimeOffset NgayTao { get; set; }
    }

    // UC-13 & UC-14
    public class TaoSuaDanhMucRequest
    {
        [Required(ErrorMessage = "Tên danh mục không được để trống.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Tên danh mục từ 2 đến 150 ký tự.")]
        public string Ten { get; set; }

        [StringLength(1000)]
        public string MoTa { get; set; }

        [StringLength(500)]
        public string HinhAnh { get; set; }

        public bool HienThi { get; set; } = true;
    }

    // UC-16: PATCH /api/danh-muc/{id}/an-hien
    public class AnHienDanhMucRequest
    {
        public bool HienThi { get; set; }
    }
}
