using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuanLyBanHang.Models.DTOs.GioHang;

namespace QuanLyBanHang.Models.DTOs.DatHang
{
    // UC-34: POST /api/dat-hang/chon-dia-chi
    public class ChonDiaChiRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn địa chỉ nhận hàng.")]
        public Guid DiaChiId { get; set; }
    }

    public class ChonDiaChiResponse
    {
        public Guid DiaChiId { get; set; }
        public string TenNguoiNhan { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChiGiaoHang { get; set; }
        public bool HoTroGiaoHang { get; set; }
    }

    // UC-35: POST /api/dat-hang/ap-dung-ma-giam-gia
    public class ApDungMaGiamGiaRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập mã giảm giá.")]
        [StringLength(50)]
        public string MaGiamGia { get; set; }
    }

    public class ApDungMaGiamGiaResponse
    {
        public Guid MaGiamGiaId { get; set; }
        public string Ma { get; set; }
        public decimal TamTinh { get; set; }
        public decimal TienGiam { get; set; }
        public decimal TongThanhToan { get; set; }
    }

    // UC-36: POST /api/dat-hang/chon-phuong-thuc-thanh-toan
    public class ChonPhuongThucThanhToanRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        [RegularExpression(@"^(KhiNhanHang|ChuyenKhoan|ViDienTu)$", ErrorMessage = "Phương thức thanh toán không hợp lệ.")]
        public string PhuongThucThanhToan { get; set; }
    }

    public class ChonPhuongThucThanhToanResponse
    {
        public string PhuongThucThanhToan { get; set; }
        public string HuongDanThanhToan { get; set; }
    }

    // UC-40: POST /api/dat-hang/xem-truoc
    public class XemTruocDatHangRequest
    {
        public Guid? DiaChiId { get; set; }
        public string MaGiamGia { get; set; }
        public string PhuongThucThanhToan { get; set; }
    }

    public class XemTruocDatHangResponse
    {
        public XemTruocDatHangResponse()
        {
            SanPhams = new List<ChiTietGioHangItemResponse>();
        }

        public ChonDiaChiResponse DiaChi { get; set; }
        public List<ChiTietGioHangItemResponse> SanPhams { get; set; }
        public string MaGiamGia { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public decimal TamTinh { get; set; }
        public decimal TienGiam { get; set; }
        public decimal TongThanhToan { get; set; }
    }

    // UC-37 & UC-40: POST /api/dat-hang/xac-nhan
    public class XacNhanDatHangRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn địa chỉ nhận hàng.")]
        public Guid DiaChiId { get; set; }

        public string MaGiamGia { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        [RegularExpression(@"^(KhiNhanHang|ChuyenKhoan|ViDienTu)$", ErrorMessage = "Phương thức thanh toán không hợp lệ.")]
        public string PhuongThucThanhToan { get; set; }
    }

    public class XacNhanDatHangResponse
    {
        public Guid DonHangId { get; set; }
        public string MaDon { get; set; }
        public string TrangThai { get; set; }
        public decimal TongThanhToan { get; set; }
        public DateTimeOffset NgayDat { get; set; }
    }
}
