using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuanLyBanHang.Models.DTOs.BaoCao;
using QuanLyBanHang.Models.DTOs.Common;
using QuanLyBanHang.Models.DTOs.DanhGia;
using QuanLyBanHang.Models.DTOs.DonHang;
using QuanLyBanHang.Models.DTOs.Kho;
using QuanLyBanHang.Models.DTOs.SanPham;
using QuanLyBanHang.Models.DTOs.TaiKhoan;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Repositories.Interfaces
{
    public interface ITaiKhoanRepository : IGenericRepository<TaiKhoan>
    {
        Task<TaiKhoan> GetByEmailOrPhoneAsync(string emailOrPhone);
        Task<bool> ExistsEmailAsync(string email, int? excludeId = null);
        Task<bool> ExistsPhoneAsync(string phone, int? excludeId = null);
        Task<PagedResult<TaiKhoan>> GetPagedAsync(TaiKhoanFilterQuery query);
        void AddLichSuKhoa(LichSuKhoaTaiKhoan lichSu);
        Task<List<LichSuKhoaTaiKhoan>> GetLichSuKhoaAsync(int taiKhoanId);
    }

    public interface IDiaChiRepository : IGenericRepository<DiaChiNhanHang>
    {
        Task<List<DiaChiNhanHang>> GetByTaiKhoanIdAsync(int taiKhoanId);
        Task<DiaChiNhanHang> GetByIdAndOwnerAsync(int id, int taiKhoanId);
        Task<DiaChiNhanHang> GetMacDinhAsync(int taiKhoanId);
        Task<int> CountByTaiKhoanAsync(int taiKhoanId);
    }

    public interface IDanhMucRepository : IGenericRepository<DanhMuc>
    {
        Task<List<DanhMuc>> GetAllActiveAsync(bool chiHienThi, string tuKhoa = null);
        Task<DanhMuc> GetActiveByIdAsync(int id);
        Task<bool> ExistsTenAsync(string ten, int? excludeId = null);
        Task<int> CountSanPhamActiveAsync(int danhMucId);
    }

    public interface ISanPhamRepository : IGenericRepository<SanPham>
    {
        Task<PagedResult<SanPham>> GetPagedAsync(SanPhamFilterQuery query, bool chiHienThi);
        Task<SanPham> GetDetailByIdAsync(int id, bool chiHienThi);
        Task<bool> ExistsMaSanPhamAsync(string maSanPham, int? excludeId = null);
        Task<bool> ExistsTenTrongDanhMucAsync(int danhMucId, string ten, int? excludeId = null);
        Task<bool> HasDonHangAsync(int sanPhamId);
    }

    public interface IKhoRepository : IGenericRepository<TonKho>
    {
        Task<TonKho> GetBySanPhamIdAsync(int sanPhamId);
        Task<PagedResult<TonKho>> GetPagedTonKhoAsync(TonKhoFilterQuery query);
        Task<PagedResult<BienDongKho>> GetPagedBienDongAsync(BienDongKhoFilterQuery query);
        void AddBienDong(BienDongKho bienDong);
    }

    public interface IGioHangRepository : IGenericRepository<GioHang>
    {
        Task<GioHang> GetByTaiKhoanIdWithDetailsAsync(int taiKhoanId);
        Task<ChiTietGioHang> GetChiTietAsync(int gioHangId, int sanPhamId);
        void AddChiTiet(ChiTietGioHang chiTiet);
        void RemoveChiTiet(ChiTietGioHang chiTiet);
        Task RemoveAllBySanPhamIdAsync(int sanPhamId);
        Task<List<YeuThichSanPham>> GetYeuThichByTaiKhoanAsync(int taiKhoanId);
        Task<YeuThichSanPham> GetYeuThichAsync(int taiKhoanId, int sanPhamId);
        void AddYeuThich(YeuThichSanPham yeuThich);
        void RemoveYeuThich(YeuThichSanPham yeuThich);
    }

    public interface IDonHangRepository : IGenericRepository<DonHang>
    {
        Task<PagedResult<DonHang>> GetPagedAsync(DonHangFilterQuery query, int? taiKhoanId = null);
        Task<DonHang> GetDetailByIdAsync(int id);
        Task<DonHang> GetByMaDonAsync(string maDon);
        Task<List<LichSuDonHang>> GetHanhTrinhAsync(int donHangId);
        Task<int> CountDonChuaHoanTatByTaiKhoanAsync(int taiKhoanId);
        Task<bool> DaMuaVaHoanThanhAsync(int taiKhoanId, int donHangId, int sanPhamId);
        void AddLichSu(LichSuDonHang lichSu);
    }

    public interface IMaGiamGiaRepository : IGenericRepository<MaGiamGia>
    {
        Task<List<MaGiamGia>> GetAllActiveAsync(bool chiKhaDungChoKhach, string tuKhoa = null);
        Task<MaGiamGia> GetActiveByIdAsync(int id);
        Task<MaGiamGia> GetActiveByMaAsync(string ma);
        Task<bool> ExistsMaAsync(string ma, int? excludeId = null);
        Task<bool> HasBeenUsedInDonHangAsync(int maGiamGiaId);
    }

    public interface IDanhGiaRepository : IGenericRepository<DanhGia>
    {
        Task<PagedResult<DanhGia>> GetPagedAsync(DanhGiaFilterQuery query);
        Task<DanhGia> GetActiveDetailByIdAsync(int id);
        Task<DanhGia> GetByDonHangAndSanPhamAsync(int donHangId, int sanPhamId);
        Task<ThichDanhGia> GetThichAsync(int taiKhoanId, int danhGiaId);
        void AddThich(ThichDanhGia thich);
        void RemoveThich(ThichDanhGia thich);
        Task<int> CountThichAsync(int danhGiaId);
        Task<PhanHoiDanhGia> GetPhanHoiByDanhGiaIdAsync(int danhGiaId);
        void AddPhanHoi(PhanHoiDanhGia phanHoi);
    }

    public interface IBaoCaoRepository
    {
        BaoCaoTongQuanDTO GetBaoCaoTongQuan(DateTimeOffset? tuNgay, DateTimeOffset? denNgay);
        Task<List<DoanhThuDTO>> GetDoanhThuAsync(DateTimeOffset tuNgay, DateTimeOffset denNgay, string donVi);
        Task<List<SanPhamBanChayDTO>> GetSanPhamBanChayAsync(SanPhamBanChayQuery query);
        Task<List<SanPhamTonLauDTO>> GetSanPhamTonLauAsync(SanPhamTonLauQuery query);
    }
}
