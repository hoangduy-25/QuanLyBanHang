using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuanLyBanHang.Models.DTOs.BaoCao;
using QuanLyBanHang.Models.DTOs.Common;
using QuanLyBanHang.Models.DTOs.DanhGia;
using QuanLyBanHang.Models.DTOs.DanhMuc;
using QuanLyBanHang.Models.DTOs.DatHang;
using QuanLyBanHang.Models.DTOs.DiaChi;
using QuanLyBanHang.Models.DTOs.DonHang;
using QuanLyBanHang.Models.DTOs.GioHang;
using QuanLyBanHang.Models.DTOs.Kho;
using QuanLyBanHang.Models.DTOs.MaGiamGia;
using QuanLyBanHang.Models.DTOs.SanPham;
using QuanLyBanHang.Models.DTOs.TaiKhoan;
using QuanLyBanHang.Models.DTOs.XacThuc;

namespace QuanLyBanHang.Services.Interfaces
{
    public interface IXacThucService
    {
        Task<int> DangKyAsync(DangKyRequest request);
        Task<DangNhapResponse> DangNhapAsync(DangNhapRequest request);
        Task<CauHoiBaoMatResponse> LayCauHoiBaoMatAsync(string emailOrPhone);
        Task QuenMatKhauAsync(QuenMatKhauRequest request);
        Task DoiMatKhauAsync(int taiKhoanId, DoiMatKhauRequest request);
        Task CapNhatCauHoiBaoMatAsync(int taiKhoanId, CapNhatCauHoiBaoMatRequest request);
    }

    public interface ITaiKhoanService
    {
        Task<HoSoResponse> LayHoSoAsync(int taiKhoanId);
        Task<HoSoResponse> CapNhatThongTinCaNhanAsync(int taiKhoanId, CapNhatHoSoRequest request);
        Task<PagedResult<TaiKhoanListItemResponse>> XemDanhSachAsync(TaiKhoanFilterQuery query);
        Task<PagedResult<TaiKhoanListItemResponse>> TimKiemAsync(TaiKhoanFilterQuery query);
        Task<KhoaMoTaiKhoanResponse> KhoaAsync(int quanLyId, int taiKhoanId, KhoaTaiKhoanRequest request);
        Task<KhoaMoTaiKhoanResponse> MoKhoaAsync(int quanLyId, int taiKhoanId);
        Task<List<LichSuKhoaResponse>> XemLichSuKhoaAsync(int taiKhoanId);
    }

    public interface IDiaChiService
    {
        Task<List<DiaChiResponse>> XemSoDiaChiAsync(int taiKhoanId);
        Task<DiaChiResponse> ThemDiaChiAsync(int taiKhoanId, TaoSuaDiaChiRequest request);
        Task<DiaChiResponse> SuaDiaChiAsync(int taiKhoanId, int diaChiId, TaoSuaDiaChiRequest request);
        Task XoaDiaChiAsync(int taiKhoanId, int diaChiId);
        Task<DiaChiResponse> DatMacDinhAsync(int taiKhoanId, int diaChiId);
    }

    public interface IDanhMucService
    {
        Task<List<DanhMucResponse>> XemDanhSachAsync(bool laQuanLy);
        Task<List<DanhMucResponse>> TimKiemAsync(string tuKhoa, bool laQuanLy);
        Task<DanhMucResponse> ThemAsync(TaoSuaDanhMucRequest request);
        Task<DanhMucResponse> SuaAsync(int id, TaoSuaDanhMucRequest request);
        Task XoaAsync(int id);
        Task<DanhMucResponse> AnHienAsync(int id, AnHienDanhMucRequest request);
    }

    public interface ISanPhamService
    {
        Task<PagedResult<SanPhamListItemResponse>> XemDanhSachAsync(SanPhamFilterQuery query, bool laQuanLy);
        Task<SanPhamChiTietResponse> XemChiTietAsync(int id, bool laQuanLy);
        Task<PagedResult<SanPhamListItemResponse>> TimKiemAsync(SanPhamFilterQuery query, bool laQuanLy);
        Task<SanPhamChiTietResponse> ThemAsync(TaoSanPhamRequest request, int quanLyId);
        Task<SanPhamChiTietResponse> SuaAsync(int id, SuaSanPhamRequest request);
        Task XoaAsync(int id);
        Task<SanPhamChiTietResponse> AnHienAsync(int id, AnHienSanPhamRequest request);
    }

    public interface IKhoService
    {
        Task<PagedResult<TonKhoResponse>> XemTonKhoAsync(TonKhoFilterQuery query);
        Task<TonKhoResponse> NhapTangAsync(int quanLyId, NhapTangKhoRequest request);
        Task<TonKhoResponse> DieuChinhGiamAsync(int quanLyId, DieuChinhGiamKhoRequest request);
        Task<TonKhoResponse> DatNguongToiThieuAsync(int tonKhoId, DatNguongToiThieuRequest request);
        Task<PagedResult<BienDongKhoResponse>> XemLichSuAsync(BienDongKhoFilterQuery query);
    }

    public interface IGioHangService
    {
        Task<GioHangResponse> XemGioHangAsync(int taiKhoanId);
        Task<GioHangResponse> ThemSanPhamAsync(int taiKhoanId, ThemVaoGioRequest request);
        Task<GioHangResponse> SuaSoLuongAsync(int taiKhoanId, int sanPhamId, SuaSoLuongGioRequest request);
        Task<GioHangResponse> XoaSanPhamAsync(int taiKhoanId, int sanPhamId);
        Task<List<YeuThichItemResponse>> XemYeuThichAsync(int taiKhoanId);
        Task ThemYeuThichAsync(int taiKhoanId, int sanPhamId);
        Task XoaYeuThichAsync(int taiKhoanId, int sanPhamId);
    }

    public interface IDatHangService
    {
        Task<ChonDiaChiResponse> ChonDiaChiAsync(int taiKhoanId, ChonDiaChiRequest request);
        Task<ApDungMaGiamGiaResponse> ApDungMaGiamGiaAsync(int taiKhoanId, ApDungMaGiamGiaRequest request);
        Task<ChonPhuongThucThanhToanResponse> ChonPhuongThucThanhToanAsync(int taiKhoanId, ChonPhuongThucThanhToanRequest request);
        Task<XemTruocDatHangResponse> XemTruocAsync(int taiKhoanId, XemTruocDatHangRequest request);
        Task<XacNhanDatHangResponse> XacNhanDatHangAsync(int taiKhoanId, XacNhanDatHangRequest request);
    }

    public interface IDonHangService
    {
        Task<PagedResult<DonHangListItemResponse>> XemDanhSachAsync(int nguoiDungId, string vaiTro, DonHangFilterQuery query);
        Task<DonHangChiTietResponse> XemChiTietAsync(int nguoiDungId, string vaiTro, int donHangId);
        Task HuyAsync(int nguoiDungId, string vaiTro, int donHangId, HuyDonHangRequest request);
        Task<List<LichSuDonHangItemResponse>> TraCuuHanhTrinhAsync(int khachHangId, int donHangId);
        Task XacNhanAsync(int nhanVienId, int donHangId);
        Task CapNhatTrangThaiAsync(int nhanVienId, int donHangId, CapNhatTrangThaiDonHangRequest request);
    }

    public interface IMaGiamGiaService
    {
        Task<List<MaGiamGiaResponse>> XemDanhSachAsync(bool laQuanLy);
        Task<List<MaGiamGiaResponse>> TimKiemAsync(string tuKhoa, bool laQuanLy);
        Task<MaGiamGiaResponse> XemChiTietAsync(int id, bool laQuanLy);
        Task<MaGiamGiaResponse> ThemAsync(TaoMaGiamGiaRequest request);
        Task<MaGiamGiaResponse> SuaAsync(int id, SuaMaGiamGiaRequest request);
        Task<string> XoaAsync(int id);
    }

    public interface IDanhGiaService
    {
        Task<PagedResult<DanhGiaResponse>> XemDanhSachAsync(DanhGiaFilterQuery query, int? currentUserId, string vaiTro);
        Task<PagedResult<DanhGiaResponse>> LocAsync(DanhGiaFilterQuery query, int? currentUserId, string vaiTro);
        Task<DanhGiaResponse> TaoAsync(int khachHangId, TaoDanhGiaRequest request);
        Task<DanhGiaResponse> SuaAsync(int khachHangId, int danhGiaId, SuaDanhGiaRequest request);
        Task<ThichDanhGiaResponse> ThichAsync(int nguoiDungId, int danhGiaId);
        Task XoaAsync(int nguoiDungId, string vaiTro, int danhGiaId, XoaDanhGiaRequest request);
        Task<PhanHoiDanhGiaResponse> TraLoiAsync(int nguoiTraLoiId, int danhGiaId, TraLoiDanhGiaRequest request);
    }

    public interface IBaoCaoService
    {
        Task<BaoCaoTongQuanDTO> XemTongQuanAsync(DateTimeOffset? tuNgay, DateTimeOffset? denNgay);
        Task<BieuDoDoanhThuResponse> XemBieuDoDoanhThuAsync(DateTimeOffset? tuNgay, DateTimeOffset? denNgay, string donVi);
        Task<List<SanPhamBanChayDTO>> XemSanPhamBanChayAsync(SanPhamBanChayQuery query);
        Task<List<SanPhamTonLauDTO>> XemSanPhamTonLauAsync(SanPhamTonLauQuery query);
        Task<string> InBaoCaoAsync(string loaiBaoCao, DateTimeOffset? tuNgay, DateTimeOffset? denNgay, string donVi, int top, int soNgayTonLau, string nguoiLap);
    }
}
