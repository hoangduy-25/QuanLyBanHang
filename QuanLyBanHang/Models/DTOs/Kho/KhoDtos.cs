using System;
using System.ComponentModel.DataAnnotations;
using QuanLyBanHang.Models.DTOs.Common;

namespace QuanLyBanHang.Models.DTOs.Kho
{
    // UC-24: GET /api/kho
    public class TonKhoFilterQuery : PaginationQuery
    {
        public string TuKhoa { get; set; }
        public Guid? DanhMucId { get; set; }
        /// <summary>DuHang | SapHet | HetHang</summary>
        public string TrangThaiTon { get; set; }
        public string SapXep { get; set; } = "SoLuongAsc";
    }

    public class TonKhoResponse
    {
        public Guid TonKhoId { get; set; }
        public Guid SanPhamId { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string TenDanhMuc { get; set; }
        public int SoLuongTon { get; set; }
        public int NguongToiThieu { get; set; }
        public string TrangThaiTon
        {
            get
            {
                if (SoLuongTon <= 0) return "HetHang";
                if (SoLuongTon <= NguongToiThieu) return "SapHet";
                return "DuHang";
            }
        }
    }

    // UC-25: POST /api/kho/nhap-tang
    public class NhapTangKhoRequest
    {
        [Required]
        public Guid SanPhamId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng nhập phải là số nguyên dương.")]
        public int SoLuongNhap { get; set; }

        [Range(0, 999999999999.99, ErrorMessage = "Giá nhập không được âm.")]
        public decimal? GiaNhap { get; set; }

        [StringLength(200)]
        public string NhaCungCap { get; set; }

        [StringLength(500)]
        public string GhiChu { get; set; }
    }

    // UC-26: POST /api/kho/dieu-chinh-giam
    public class DieuChinhGiamKhoRequest
    {
        [Required]
        public Guid SanPhamId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng giảm phải là số nguyên dương.")]
        public int SoLuongGiam { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do điều chỉnh giảm.")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Lý do từ 3 đến 500 ký tự.")]
        public string LyDo { get; set; }
    }

    // UC-27: PATCH /api/kho/{id}/nguong-toi-thieu
    public class DatNguongToiThieuRequest
    {
        [Range(0, int.MaxValue, ErrorMessage = "Ngưỡng tối thiểu phải là số nguyên không âm.")]
        public int NguongToiThieu { get; set; }
    }

    // UC-28: GET /api/kho/lich-su
    public class BienDongKhoFilterQuery : PaginationQuery
    {
        public Guid? SanPhamId { get; set; }
        public string Loai { get; set; }
        public DateTimeOffset? TuNgay { get; set; }
        public DateTimeOffset? DenNgay { get; set; }
    }

    public class BienDongKhoResponse
    {
        public Guid Id { get; set; }
        public Guid TonKhoId { get; set; }
        public Guid SanPhamId { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public Guid? DonHangId { get; set; }
        public string MaDon { get; set; }
        public string Loai { get; set; }
        public int SoLuongThayDoi { get; set; }
        public int SoLuongTruoc { get; set; }
        public int SoLuongSau { get; set; }
        public decimal? GiaNhap { get; set; }
        public string NhaCungCap { get; set; }
        public string NguoiThucHien { get; set; }
        public string LyDo { get; set; }
        public DateTimeOffset ThoiDiem { get; set; }
    }
}
