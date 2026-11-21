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
    public class DanhMucRepository : GenericRepository<DanhMuc>, IDanhMucRepository
    {
        public DanhMucRepository(QuanLyBanHangDbContext context) : base(context) { }

        public Task<List<DanhMuc>> GetAllActiveAsync(bool chiHienThi, string tuKhoa = null)
        {
            var q = DbSet.AsNoTracking()
                .Include(d => d.SanPhams)
                .Where(d => !d.IsDeleted);

            if (chiHienThi)
                q = q.Where(d => d.HienThi);

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var kw = tuKhoa.Trim().ToLower();
                q = q.Where(d => d.Ten.ToLower().Contains(kw));
            }

            return q.OrderBy(d => d.Ten).ToListAsync();
        }

        public Task<DanhMuc> GetActiveByIdAsync(int id)
        {
            return DbSet.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

        public Task<bool> ExistsTenAsync(string ten, int? excludeId = null)
        {
            var normalized = (ten ?? string.Empty).Trim().ToLower();
            return DbSet.AnyAsync(d =>
                !d.IsDeleted &&
                d.Ten.ToLower() == normalized &&
                (!excludeId.HasValue || d.Id != excludeId.Value));
        }

        public Task<int> CountSanPhamActiveAsync(int danhMucId)
        {
            return Context.SanPhams.CountAsync(sp => sp.DanhMucId == danhMucId && !sp.IsDeleted);
        }
    }
}
