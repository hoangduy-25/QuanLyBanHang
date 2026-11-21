using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyBanHang.Models.DTOs.DiaChi
{
    // UC-07: GET /api/dia-chi
    public class DiaChiResponse
    {
        public int Id { get; set; }
        public string TenNguoiNhan { get; set; }
        public string SoDienThoai { get; set; }
        public string TinhThanh { get; set; }
        public string QuanHuyen { get; set; }
        public string PhuongXa { get; set; }
        public string DiaChiCuThe { get; set; }
        public bool MacDinh { get; set; }
        public string DiaChiDayDu
        {
            get { return string.Format("{0}, {1}, {2}, {3}", DiaChiCuThe, PhuongXa, QuanHuyen, TinhThanh); }
        }
    }

    // UC-07: POST /api/dia-chi & PUT /api/dia-chi/{id}
    public class TaoSuaDiaChiRequest
    {
        [Required(ErrorMessage = "Tên người nhận không được để trống.")]
        [StringLength(100)]
        public string TenNguoiNhan { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [RegularExpression(@"^(0|\+84)[0-9]{9,10}$", ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [StringLength(20)]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Tỉnh/Thành phố không được để trống.")]
        [StringLength(100)]
        public string TinhThanh { get; set; }

        [Required(ErrorMessage = "Quận/Huyện không được để trống.")]
        [StringLength(100)]
        public string QuanHuyen { get; set; }

        [Required(ErrorMessage = "Phường/Xã không được để trống.")]
        [StringLength(100)]
        public string PhuongXa { get; set; }

        [Required(ErrorMessage = "Địa chỉ cụ thể không được để trống.")]
        [StringLength(300)]
        public string DiaChiCuThe { get; set; }

        public bool MacDinh { get; set; }
    }
}
