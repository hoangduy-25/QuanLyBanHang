using System;

namespace QuanLyBanHang.Models.Entities
{
    /// <summary>
    /// Bảng dbo.DiaChiNhanHang (9 cột) - UC-07, UC-34.
    /// </summary>
    public class DiaChiNhanHang
    {
        public int Id { get; set; }
        public int TaiKhoanId { get; set; }
        public string TenNguoiNhan { get; set; }
        public string SoDienThoai { get; set; }
        public string TinhThanh { get; set; }
        public string QuanHuyen { get; set; }
        public string PhuongXa { get; set; }
        public string DiaChiCuThe { get; set; }
        public bool MacDinh { get; set; }

        public virtual TaiKhoan TaiKhoan { get; set; }
    }
}
