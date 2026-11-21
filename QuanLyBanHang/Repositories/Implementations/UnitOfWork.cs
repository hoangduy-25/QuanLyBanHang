using System.Data;
using System.Data.Entity;
using System.Threading.Tasks;
using QuanLyBanHang.Data;
using QuanLyBanHang.Repositories.Interfaces;

namespace QuanLyBanHang.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly QuanLyBanHangDbContext _context;
        private DbContextTransaction _transaction;

        public UnitOfWork(QuanLyBanHangDbContext context)
        {
            _context = context;
            TaiKhoans = new TaiKhoanRepository(_context);
            DiaChis = new DiaChiRepository(_context);
            DanhMucs = new DanhMucRepository(_context);
            SanPhams = new SanPhamRepository(_context);
            Khos = new KhoRepository(_context);
            GioHangs = new GioHangRepository(_context);
            DonHangs = new DonHangRepository(_context);
            MaGiamGias = new MaGiamGiaRepository(_context);
            DanhGias = new DanhGiaRepository(_context);
            BaoCaos = new BaoCaoRepository(_context);
        }

        public ITaiKhoanRepository TaiKhoans { get; private set; }
        public IDiaChiRepository DiaChis { get; private set; }
        public IDanhMucRepository DanhMucs { get; private set; }
        public ISanPhamRepository SanPhams { get; private set; }
        public IKhoRepository Khos { get; private set; }
        public IGioHangRepository GioHangs { get; private set; }
        public IDonHangRepository DonHangs { get; private set; }
        public IMaGiamGiaRepository MaGiamGias { get; private set; }
        public IDanhGiaRepository DanhGias { get; private set; }
        public IBaoCaoRepository BaoCaos { get; private set; }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_transaction == null)
                _transaction = _context.Database.BeginTransaction(isolationLevel);
        }

        public void CommitTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void RollbackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
            _context.Dispose();
        }
    }
}
