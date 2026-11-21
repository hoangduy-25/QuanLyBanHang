using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBanHang.Data;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Repositories.Interfaces;

namespace QuanLyBanHang.Repositories.Implementations
{
    public class MaGiamGiaRepository : GenericRepository<MaGiamGia>, IMaGiamGiaRepository
    {
        public MaGiamGiaRepository(QuanLyBanHangDbContext context) : base(context) { }

        public Task<List<MaGiamGia>> GetAllActiveAsync(bool chiKhaDungChoKhach, string tuKhoa = null)
        {
            var now = DateTimeOffset.Now;
            var q = DbSet.AsNoTracking().Where(m => !m.IsDeleted);

            if (chiKhaDungChoKhach)
            {
                q = q.Where(m =>
                    m.HienThi &&
                    m.NgayBatDau <= now &&
                    m.NgayKetThuc >= now &&
                    m.SoLuotDaDung < m.SoLuotToiDa);
            }

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var kw = tuKhoa.Trim().ToLower();
                q = q.Where(m =>
                    m.Ma.ToLower().Contains(kw) ||
                    (m.MoTa != null && m.MoTa.ToLower().Contains(kw)));
            }

            return q.OrderByDescending(m => m.NgayKetThuc).ToListAsync();
        }

        public Task<MaGiamGia> GetActiveByIdAsync(int id)
        {
            return DbSet.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
        }

        public Task<MaGiamGia> GetActiveByMaAsync(string ma)
        {
            var normalized = (ma ?? string.Empty).Trim().ToLower();
            return DbSet.FirstOrDefaultAsync(m => !m.IsDeleted && m.Ma.ToLower() == normalized);
        }

        public Task<bool> ExistsMaAsync(string ma, int? excludeId = null)
        {
            var normalized = (ma ?? string.Empty).Trim().ToLower();
            return DbSet.AnyAsync(m =>
                !m.IsDeleted &&
                m.Ma.ToLower() == normalized &&
                (!excludeId.HasValue || m.Id != excludeId.Value));
        }

        public Task<bool> HasBeenUsedInDonHangAsync(int maGiamGiaId)
        {
            return Context.DonHangs.AnyAsync(d => d.MaGiamGiaId == maGiamGiaId);
        }
    }
}
