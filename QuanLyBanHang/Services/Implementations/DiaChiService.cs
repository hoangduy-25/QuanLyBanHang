using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBanHang.Models.DTOs.DiaChi;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Repositories.Interfaces;
using QuanLyBanHang.Services.Exceptions;
using QuanLyBanHang.Services.Interfaces;
using QuanLyBanHang.Services.Helpers;

namespace QuanLyBanHang.Services.Implementations
{
    public class DiaChiService : IDiaChiService
    {
        private const int MaxAddressesPerUser = 10;
        private readonly IUnitOfWork _uow;

        public DiaChiService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<DiaChiResponse>> XemSoDiaChiAsync(int taiKhoanId)
        {
            var list = await _uow.DiaChis.GetByTaiKhoanIdAsync(taiKhoanId);
            return list.Select(Map).ToList();
        }

        public async Task<DiaChiResponse> ThemDiaChiAsync(int taiKhoanId, TaoSuaDiaChiRequest request)
        {
            if (request == null)
                throw new BadRequestException("Dữ liệu địa chỉ không hợp lệ.");
            var count = await _uow.DiaChis.CountByTaiKhoanAsync(taiKhoanId);
            if (count >= MaxAddressesPerUser)
                throw new BadRequestException(string.Format("Bạn chỉ được lưu tối đa {0} địa chỉ nhận hàng.", MaxAddressesPerUser));

            // Địa chỉ đầu tiên luôn tự động là mặc định
            bool setDefault = request.MacDinh || count == 0;

            _uow.BeginTransaction();
            try
            {
                if (setDefault)
                {
                    var currentDefault = await _uow.DiaChis.GetMacDinhAsync(taiKhoanId);
                    if (currentDefault != null)
                    {
                        currentDefault.MacDinh = false;
                        // Lưu trước để không vi phạm Filtered Unique Index UQ_DiaChiNhanHang_MacDinh
                        await _uow.SaveChangesAsync();
                    }
                }

                var entity = new DiaChiNhanHang
                {
                    TaiKhoanId = taiKhoanId,
                    TenNguoiNhan = request.TenNguoiNhan.Trim(),
                    SoDienThoai = PasswordHasher.NormalizePhone(request.SoDienThoai),
                    TinhThanh = request.TinhThanh.Trim(),
                    QuanHuyen = request.QuanHuyen.Trim(),
                    PhuongXa = request.PhuongXa.Trim(),
                    DiaChiCuThe = request.DiaChiCuThe.Trim(),
                    MacDinh = setDefault
                };

                _uow.DiaChis.Add(entity);
                await _uow.SaveChangesAsync();
                _uow.CommitTransaction();

                return Map(entity);
            }
            catch
            {
                _uow.RollbackTransaction();
                throw;
            }
        }

        public async Task<DiaChiResponse> SuaDiaChiAsync(int taiKhoanId, int diaChiId, TaoSuaDiaChiRequest request)
        {
            if (request == null)
                throw new BadRequestException("Dữ liệu địa chỉ không hợp lệ.");
            var entity = await _uow.DiaChis.GetByIdAndOwnerAsync(diaChiId, taiKhoanId);
            if (entity == null)
                throw new NotFoundException("Không tìm thấy địa chỉ nhận hàng.");

            _uow.BeginTransaction();
            try
            {
                if (request.MacDinh && !entity.MacDinh)
                {
                    var currentDefault = await _uow.DiaChis.GetMacDinhAsync(taiKhoanId);
                    if (currentDefault != null && currentDefault.Id != entity.Id)
                    {
                        currentDefault.MacDinh = false;
                        await _uow.SaveChangesAsync();
                    }
                    entity.MacDinh = true;
                }

                entity.TenNguoiNhan = request.TenNguoiNhan.Trim();
                entity.SoDienThoai = PasswordHasher.NormalizePhone(request.SoDienThoai);
                entity.TinhThanh = request.TinhThanh.Trim();
                entity.QuanHuyen = request.QuanHuyen.Trim();
                entity.PhuongXa = request.PhuongXa.Trim();
                entity.DiaChiCuThe = request.DiaChiCuThe.Trim();

                await _uow.SaveChangesAsync();
                _uow.CommitTransaction();
                return Map(entity);
            }
            catch
            {
                _uow.RollbackTransaction();
                throw;
            }
        }

        public async Task XoaDiaChiAsync(int taiKhoanId, int diaChiId)
        {
            var entity = await _uow.DiaChis.GetByIdAndOwnerAsync(diaChiId, taiKhoanId);
            if (entity == null)
                throw new NotFoundException("Không tìm thấy địa chỉ nhận hàng.");

            bool wasDefault = entity.MacDinh;
            _uow.BeginTransaction();
            try
            {
                _uow.DiaChis.Remove(entity);
                await _uow.SaveChangesAsync();

                // UC-07 5b: Nếu xóa địa chỉ mặc định, tự động đặt địa chỉ còn lại làm mặc định
                if (wasDefault)
                {
                    var remaining = await _uow.DiaChis.GetByTaiKhoanIdAsync(taiKhoanId);
                    var nextDefault = remaining.FirstOrDefault();
                    if (nextDefault != null)
                    {
                        nextDefault.MacDinh = true;
                        await _uow.SaveChangesAsync();
                    }
                }

                _uow.CommitTransaction();
            }
            catch
            {
                _uow.RollbackTransaction();
                throw;
            }
        }

        public async Task<DiaChiResponse> DatMacDinhAsync(int taiKhoanId, int diaChiId)
        {
            var entity = await _uow.DiaChis.GetByIdAndOwnerAsync(diaChiId, taiKhoanId);
            if (entity == null)
                throw new NotFoundException("Không tìm thấy địa chỉ nhận hàng.");

            if (entity.MacDinh)
                return Map(entity);

            _uow.BeginTransaction();
            try
            {
                var currentDefault = await _uow.DiaChis.GetMacDinhAsync(taiKhoanId);
                if (currentDefault != null)
                {
                    currentDefault.MacDinh = false;
                    await _uow.SaveChangesAsync();
                }

                entity.MacDinh = true;
                await _uow.SaveChangesAsync();
                _uow.CommitTransaction();
                return Map(entity);
            }
            catch
            {
                _uow.RollbackTransaction();
                throw;
            }
        }

        private static DiaChiResponse Map(DiaChiNhanHang d)
        {
            return new DiaChiResponse
            {
                Id = d.Id,
                TenNguoiNhan = d.TenNguoiNhan,
                SoDienThoai = d.SoDienThoai,
                TinhThanh = d.TinhThanh,
                QuanHuyen = d.QuanHuyen,
                PhuongXa = d.PhuongXa,
                DiaChiCuThe = d.DiaChiCuThe,
                MacDinh = d.MacDinh
            };
        }
    }
}
