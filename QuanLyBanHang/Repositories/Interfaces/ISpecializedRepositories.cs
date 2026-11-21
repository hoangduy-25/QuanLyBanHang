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
        Task<bool> ExistsEmailAsync(string email, Guid? excludeId = null);
        Task<bool> ExistsPhoneAsync(string phone, Guid? excludeId = null);
        Task<PagedResult<TaiKhoan>> GetPagedAsync(TaiKhoanFilterQuery query);
        void AddLichSuKhoa(LichSuKhoaTaiKhoan lichSu);
        Task<List<LichSuKhoaTaiKhoan>> GetLichSuKhoaAsync(Guid taiKhoanId);
    }

    public interface IDiaChiRepository : IGenericRepository<DiaChiNhanHang>
    {
        Task<List<DiaChiNhanHang>> GetByTaiKhoanIdAsync(Guid taiKhoanId);
        Task<DiaChiNhanHang> GetByIdAndOwnerAsync(Guid id, Guid taiKhoanId);
        Task<DiaChiNhanHang> GetMacDinhAsync(Guid taiKhoanId);
        Task<int> CountByTaiKhoanAsync(Guid taiKhoanId);
    }

    public interface IDanhMucRepository : IGenericRepository<DanhMuc>
    {
        Task<List<DanhMuc>> GetAllActiveAsync(bool chiHienThi, string tuKhoa = null);
        Task<DanhMuc> GetActiveByIdAsync(Guid id);
        Task<bool> ExistsTenAsync(string ten, Guid? excludeId = null);
        Task<int> CountSanPhamActiveAsync(Guid danhMucId);
    }

    public interface ISanPhamRepository : IGenericRepository<SanPham>
    {
        Task<PagedResult<SanPham>> GetPagedAsync(SanPhamFilterQuery query, bool chiHienThi);
        Task<SanPham> GetDetailByIdAsync(Guid id, bool chiHienThi);
        Task<bool> ExistsMaSanPhamAsync(string maSanPham, Guid? excludeId = null);
        Task<bool> ExistsTenTrongDanhMucAsync(Guid danhMucId, string ten, Guid? excludeId = null);
        Task<bool> HasDonHangAsync(Guid sanPhamId);
    }

    public interface IKhoRepository : IGenericRepository<TonKho>
    {
        Task<TonKho> GetBySanPhamIdAsync(Guid sanPhamId);
        Task<PagedResult<TonKho>> GetPagedTonKhoAsync(TonKhoFilterQuery query);
        Task<PagedResult<BienDongKho>> GetPagedBienDongAsync(BienDongKhoFilterQuery query);
        void AddBienDong(BienDongKho bienDong);
    }

    public interface IGioHangRepository : IGenericRepository<GioHang>
    {
        Task<GioHang> GetByTaiKhoanIdWithDetailsAsync(Guid taiKhoanId);
        Task<ChiTietGioHang> GetChiTietAsync(Guid gioHangId, Guid sanPhamId);
        void AddChiTiet(ChiTietGioHang chiTiet);
        void RemoveChiTiet(ChiTietGioHang chiTiet);
        Task RemoveAllBySanPhamIdAsync(Guid sanPhamId);
        Task<List<YeuThichSanPham>> GetYeuThichByTaiKhoanAsync(Guid taiKhoanId);
        Task<YeuThichSanPham> GetYeuThichAsync(Guid taiKhoanId, Guid sanPhamId);
        void AddYeuThich(YeuThichSanPham yeuThich);
        void RemoveYeuThich(YeuThichSanPham yeuThich);
    }

    public interface IDonHangRepository : IGenericRepository<DonHang>
    {
        Task<PagedResult<DonHang>> GetPagedAsync(DonHangFilterQuery query, Guid? taiKhoanId = null);
        Task<DonHang> GetDetailByIdAsync(Guid id);
        Task<DonHang> GetByMaDonAsync(string maDon);
        Task<List<LichSuDonHang>> GetHanhTrinhAsync(Guid donHangId);
        Task<int> CountDonChuaHoanTatByTaiKhoanAsync(Guid taiKhoanId);
        Task<bool> DaMuaVaHoanThanhAsync(Guid taiKhoanId, Guid donHangId, Guid sanPhamId);
        void AddLichSu(LichSuDonHang lichSu);
    }

    public interface IMaGiamGiaRepository : IGenericRepository<MaGiamGia>
    {
        Task<List<MaGiamGia>> GetAllActiveAsync(bool chiKhaDungChoKhach, string tuKhoa = null);
        Task<MaGiamGia> GetActiveByIdAsync(Guid id);
        Task<MaGiamGia> GetActiveByMaAsync(string ma);
        Task<bool> ExistsMaAsync(string ma, Guid? excludeId = null);
        Task<bool> HasBeenUsedInDonHangAsync(Guid maGiamGiaId);
    }

    public interface IDanhGiaRepository : IGenericRepository<DanhGia>
    {
        Task<PagedResult<DanhGia>> GetPagedAsync(DanhGiaFilterQuery query);
        Task<DanhGia> GetActiveDetailByIdAsync(Guid id);
        Task<DanhGia> GetByDonHangAndSanPhamAsync(Guid donHangId, Guid sanPhamId);
        Task<ThichDanhGia> GetThichAsync(Guid taiKhoanId, Guid danhGiaId);
        void AddThich(ThichDanhGia thich);
        void RemoveThich(ThichDanhGia thich);
        Task<int> CountThichAsync(Guid danhGiaId);
        Task<PhanHoiDanhGia> GetPhanHoiByDanhGiaIdAsync(Guid danhGiaId);
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
