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
        Task<Guid> DangKyAsync(DangKyRequest request);
        Task<DangNhapResponse> DangNhapAsync(DangNhapRequest request);
        Task<CauHoiBaoMatResponse> LayCauHoiBaoMatAsync(string emailOrPhone);
        Task QuenMatKhauAsync(QuenMatKhauRequest request);
        Task DoiMatKhauAsync(Guid taiKhoanId, DoiMatKhauRequest request);
        Task CapNhatCauHoiBaoMatAsync(Guid taiKhoanId, CapNhatCauHoiBaoMatRequest request);
    }

    public interface ITaiKhoanService
    {
        Task<HoSoResponse> LayHoSoAsync(Guid taiKhoanId);
        Task<HoSoResponse> CapNhatThongTinCaNhanAsync(Guid taiKhoanId, CapNhatHoSoRequest request);
        Task<PagedResult<TaiKhoanListItemResponse>> XemDanhSachAsync(TaiKhoanFilterQuery query);
        Task<PagedResult<TaiKhoanListItemResponse>> TimKiemAsync(TaiKhoanFilterQuery query);
        Task<KhoaMoTaiKhoanResponse> KhoaAsync(Guid quanLyId, Guid taiKhoanId, KhoaTaiKhoanRequest request);
        Task<KhoaMoTaiKhoanResponse> MoKhoaAsync(Guid quanLyId, Guid taiKhoanId);
        Task<List<LichSuKhoaResponse>> XemLichSuKhoaAsync(Guid taiKhoanId);
    }

    public interface IDiaChiService
    {
        Task<List<DiaChiResponse>> XemSoDiaChiAsync(Guid taiKhoanId);
        Task<DiaChiResponse> ThemDiaChiAsync(Guid taiKhoanId, TaoSuaDiaChiRequest request);
        Task<DiaChiResponse> SuaDiaChiAsync(Guid taiKhoanId, Guid diaChiId, TaoSuaDiaChiRequest request);
        Task XoaDiaChiAsync(Guid taiKhoanId, Guid diaChiId);
        Task<DiaChiResponse> DatMacDinhAsync(Guid taiKhoanId, Guid diaChiId);
    }

    public interface IDanhMucService
    {
        Task<List<DanhMucResponse>> XemDanhSachAsync(bool laQuanLy);
        Task<List<DanhMucResponse>> TimKiemAsync(string tuKhoa, bool laQuanLy);
        Task<DanhMucResponse> ThemAsync(TaoSuaDanhMucRequest request);
        Task<DanhMucResponse> SuaAsync(Guid id, TaoSuaDanhMucRequest request);
        Task XoaAsync(Guid id);
        Task<DanhMucResponse> AnHienAsync(Guid id, AnHienDanhMucRequest request);
    }

    public interface ISanPhamService
    {
        Task<PagedResult<SanPhamListItemResponse>> XemDanhSachAsync(SanPhamFilterQuery query, bool laQuanLy);
        Task<SanPhamChiTietResponse> XemChiTietAsync(Guid id, bool laQuanLy);
        Task<PagedResult<SanPhamListItemResponse>> TimKiemAsync(SanPhamFilterQuery query, bool laQuanLy);
        Task<SanPhamChiTietResponse> ThemAsync(TaoSanPhamRequest request, Guid quanLyId);
        Task<SanPhamChiTietResponse> SuaAsync(Guid id, SuaSanPhamRequest request);
        Task XoaAsync(Guid id);
        Task<SanPhamChiTietResponse> AnHienAsync(Guid id, AnHienSanPhamRequest request);
    }

    public interface IKhoService
    {
        Task<PagedResult<TonKhoResponse>> XemTonKhoAsync(TonKhoFilterQuery query);
        Task<TonKhoResponse> NhapTangAsync(Guid quanLyId, NhapTangKhoRequest request);
        Task<TonKhoResponse> DieuChinhGiamAsync(Guid quanLyId, DieuChinhGiamKhoRequest request);
        Task<TonKhoResponse> DatNguongToiThieuAsync(Guid tonKhoId, DatNguongToiThieuRequest request);
        Task<PagedResult<BienDongKhoResponse>> XemLichSuAsync(BienDongKhoFilterQuery query);
    }

    public interface IGioHangService
    {
        Task<GioHangResponse> XemGioHangAsync(Guid taiKhoanId);
        Task<GioHangResponse> ThemSanPhamAsync(Guid taiKhoanId, ThemVaoGioRequest request);
        Task<GioHangResponse> SuaSoLuongAsync(Guid taiKhoanId, Guid sanPhamId, SuaSoLuongGioRequest request);
        Task<GioHangResponse> XoaSanPhamAsync(Guid taiKhoanId, Guid sanPhamId);
        Task<List<YeuThichItemResponse>> XemYeuThichAsync(Guid taiKhoanId);
        Task ThemYeuThichAsync(Guid taiKhoanId, Guid sanPhamId);
        Task XoaYeuThichAsync(Guid taiKhoanId, Guid sanPhamId);
    }

    public interface IDatHangService
    {
        Task<ChonDiaChiResponse> ChonDiaChiAsync(Guid taiKhoanId, ChonDiaChiRequest request);
        Task<ApDungMaGiamGiaResponse> ApDungMaGiamGiaAsync(Guid taiKhoanId, ApDungMaGiamGiaRequest request);
        Task<ChonPhuongThucThanhToanResponse> ChonPhuongThucThanhToanAsync(Guid taiKhoanId, ChonPhuongThucThanhToanRequest request);
        Task<XemTruocDatHangResponse> XemTruocAsync(Guid taiKhoanId, XemTruocDatHangRequest request);
        Task<XacNhanDatHangResponse> XacNhanDatHangAsync(Guid taiKhoanId, XacNhanDatHangRequest request);
    }

    public interface IDonHangService
    {
        Task<PagedResult<DonHangListItemResponse>> XemDanhSachAsync(Guid nguoiDungId, string vaiTro, DonHangFilterQuery query);
        Task<DonHangChiTietResponse> XemChiTietAsync(Guid nguoiDungId, string vaiTro, Guid donHangId);
        Task HuyAsync(Guid nguoiDungId, string vaiTro, Guid donHangId, HuyDonHangRequest request);
        Task<List<LichSuDonHangItemResponse>> TraCuuHanhTrinhAsync(Guid khachHangId, Guid donHangId);
        Task XacNhanAsync(Guid nhanVienId, Guid donHangId);
        Task CapNhatTrangThaiAsync(Guid nhanVienId, Guid donHangId, CapNhatTrangThaiDonHangRequest request);
    }

    public interface IMaGiamGiaService
    {
        Task<List<MaGiamGiaResponse>> XemDanhSachAsync(bool laQuanLy);
        Task<List<MaGiamGiaResponse>> TimKiemAsync(string tuKhoa, bool laQuanLy);
        Task<MaGiamGiaResponse> XemChiTietAsync(Guid id, bool laQuanLy);
        Task<MaGiamGiaResponse> ThemAsync(TaoMaGiamGiaRequest request);
        Task<MaGiamGiaResponse> SuaAsync(Guid id, SuaMaGiamGiaRequest request);
        Task<string> XoaAsync(Guid id);
    }

    public interface IDanhGiaService
    {
        Task<PagedResult<DanhGiaResponse>> XemDanhSachAsync(DanhGiaFilterQuery query, Guid? currentUserId, string vaiTro);
        Task<PagedResult<DanhGiaResponse>> LocAsync(DanhGiaFilterQuery query, Guid? currentUserId, string vaiTro);
        Task<DanhGiaResponse> TaoAsync(Guid khachHangId, TaoDanhGiaRequest request);
        Task<DanhGiaResponse> SuaAsync(Guid khachHangId, Guid danhGiaId, SuaDanhGiaRequest request);
        Task<ThichDanhGiaResponse> ThichAsync(Guid nguoiDungId, Guid danhGiaId);
        Task XoaAsync(Guid nguoiDungId, string vaiTro, Guid danhGiaId, XoaDanhGiaRequest request);
        Task<PhanHoiDanhGiaResponse> TraLoiAsync(Guid nguoiTraLoiId, Guid danhGiaId, TraLoiDanhGiaRequest request);
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
