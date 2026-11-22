using QuanLyBanHang.Data;
using QuanLyBanHang.Models.DTOs.Common;
using QuanLyBanHang.Models.DTOs.TaiKhoan;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Repositories.Interfaces;
using QuanLyBanHang.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyBanHang.Repositories.Implementations
{
    public class TaiKhoanRepository : GenericRepository<TaiKhoan>, ITaiKhoanRepository
    {
        public TaiKhoanRepository(QuanLyBanHangDbContext context) : base(context) { }

        public Task<TaiKhoan> GetByEmailOrPhoneAsync(string emailOrPhone)
        {
            var raw = (emailOrPhone ?? string.Empty).Trim();
            var normalizedEmail = raw.ToLower();
            var normalizedPhone = PasswordHasher.NormalizePhone(raw);
            return DbSet.FirstOrDefaultAsync(t =>
                t.Email.ToLower() == normalizedEmail ||
                t.SoDienThoai == normalizedPhone ||
                t.SoDienThoai == raw);
        }

        public Task<bool> ExistsEmailAsync(string email, int? excludeId = null)
        {
            var normalized = (email ?? string.Empty).Trim().ToLower();
            return DbSet.AnyAsync(t =>
                t.Email.ToLower() == normalized && (!excludeId.HasValue || t.Id != excludeId.Value));
        }

        public Task<bool> ExistsPhoneAsync(string phone, int? excludeId = null)
        {
            var normalized = PasswordHasher.NormalizePhone(phone);
            return DbSet.AnyAsync(t =>
                t.SoDienThoai == normalized && (!excludeId.HasValue || t.Id != excludeId.Value));
        }

        public async Task<PagedResult<TaiKhoan>> GetPagedAsync(TaiKhoanFilterQuery query)
        {
            var q = DbSet.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.TuKhoa))
            {
                var kw = query.TuKhoa.Trim().ToLower();
                q = q.Where(t =>
                    t.HoTen.ToLower().Contains(kw) ||
                    t.Email.ToLower().Contains(kw) ||
                    t.SoDienThoai.Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(query.VaiTro))
                q = q.Where(t => t.VaiTro == query.VaiTro);

            if (!string.IsNullOrWhiteSpace(query.TrangThai))
                q = q.Where(t => t.TrangThai == query.TrangThai);

            var total = await q.CountAsync();

            switch (query.SapXep)
            {
                case "HoTenAsc": q = q.OrderBy(t => t.HoTen); break;
                case "HoTenDesc": q = q.OrderByDescending(t => t.HoTen); break;
                case "NgayTaoAsc": q = q.OrderBy(t => t.NgayTao); break;
                default: q = q.OrderByDescending(t => t.NgayTao); break;
            }

            var items = await q
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<TaiKhoan>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = total
            };
        }

        public void AddLichSuKhoa(LichSuKhoaTaiKhoan lichSu)
        {
            Context.LichSuKhoaTaiKhoans.Add(lichSu);
        }

        public Task<List<LichSuKhoaTaiKhoan>> GetLichSuKhoaAsync(int taiKhoanId)
        {
            return Context.LichSuKhoaTaiKhoans
                .AsNoTracking()
                .Include(x => x.QuanLy)
                .Where(x => x.TaiKhoanId == taiKhoanId)
                .OrderByDescending(x => x.ThoiDiem)
                .ToListAsync();
        }
    }
}
