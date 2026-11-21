using System;
using System.Data;
using System.Threading.Tasks;

namespace QuanLyBanHang.Repositories.Interfaces
{
    /// <summary>
    /// Unit of Work điều phối giao dịch (Transaction) cho các nghiệp vụ đa bảng
    /// (UC-20 Thêm SP + Tồn kho, UC-25/26 Biến động kho, UC-37 Đặt hàng, UC-41 Hủy đơn).
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        ITaiKhoanRepository TaiKhoans { get; }
        IDiaChiRepository DiaChis { get; }
        IDanhMucRepository DanhMucs { get; }
        ISanPhamRepository SanPhams { get; }
        IKhoRepository Khos { get; }
        IGioHangRepository GioHangs { get; }
        IDonHangRepository DonHangs { get; }
        IMaGiamGiaRepository MaGiamGias { get; }
        IDanhGiaRepository DanhGias { get; }
        IBaoCaoRepository BaoCaos { get; }

        Task<int> SaveChangesAsync();
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        void CommitTransaction();
        void RollbackTransaction();
    }
}
