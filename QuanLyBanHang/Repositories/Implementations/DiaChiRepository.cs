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
    public class DiaChiRepository : GenericRepository<DiaChiNhanHang>, IDiaChiRepository
    {
        public DiaChiRepository(QuanLyBanHangDbContext context) : base(context) { }

        public Task<List<DiaChiNhanHang>> GetByTaiKhoanIdAsync(int taiKhoanId)
        {
            return DbSet
                .Where(d => d.TaiKhoanId == taiKhoanId)
                .OrderByDescending(d => d.MacDinh)
                .ThenBy(d => d.TenNguoiNhan)
                .ToListAsync();
        }

        public Task<DiaChiNhanHang> GetByIdAndOwnerAsync(int id, int taiKhoanId)
        {
            return DbSet.FirstOrDefaultAsync(d => d.Id == id && d.TaiKhoanId == taiKhoanId);
        }

        public Task<DiaChiNhanHang> GetMacDinhAsync(int taiKhoanId)
        {
            return DbSet.FirstOrDefaultAsync(d => d.TaiKhoanId == taiKhoanId && d.MacDinh);
        }

        public Task<int> CountByTaiKhoanAsync(int taiKhoanId)
        {
            return DbSet.CountAsync(d => d.TaiKhoanId == taiKhoanId);
        }
    }
}
