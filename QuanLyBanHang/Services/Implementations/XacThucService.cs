using System;
using System.Threading.Tasks;
using QuanLyBanHang.Models.DTOs.XacThuc;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Models.Enums;
using QuanLyBanHang.Repositories.Interfaces;
using QuanLyBanHang.Services.Exceptions;
using QuanLyBanHang.Services.Helpers;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Services.Implementations
{
    public class XacThucService : IXacThucService
    {
        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes = 15;
        private readonly IUnitOfWork _uow;

        public XacThucService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // UC-01: Đăng ký tài khoản khách hàng
        public async Task<int> DangKyAsync(DangKyRequest request)
        {
            if (request == null)
                throw new BadRequestException("Dữ liệu đăng ký không hợp lệ.");

            if (!PasswordHasher.IsStrongPassword(request.MatKhau))
                throw new BadRequestException("Mật khẩu phải dài tối thiểu 8 ký tự, gồm chữ hoa, chữ thường và chữ số.");

            if (await _uow.TaiKhoans.ExistsEmailAsync(request.Email))
                throw new ConflictException("Email đã tồn tại trong hệ thống. Vui lòng đăng nhập hoặc dùng chức năng Quên mật khẩu.");

            if (await _uow.TaiKhoans.ExistsPhoneAsync(request.SoDienThoai))
                throw new ConflictException("Số điện thoại đã tồn tại trong hệ thống. Vui lòng đăng nhập hoặc dùng chức năng Quên mật khẩu.");

            var taiKhoan = new TaiKhoan
            {
                HoTen = request.HoTen.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                SoDienThoai = request.SoDienThoai.Trim(),
                MatKhauHash = PasswordHasher.Hash(request.MatKhau),
                VaiTro = VaiTro.KhachHang.ToString(),
                TrangThai = TrangThaiTaiKhoan.HoatDong.ToString(),
                CauHoiBaoMat = request.CauHoiBaoMat.Trim(),
                CauTraLoiHash = PasswordHasher.HashSecurityAnswer(request.CauTraLoiBaoMat),
                SoLanDangNhapSai = 0,
                NgayTao = DateTimeOffset.Now
            };

            _uow.TaiKhoans.Add(taiKhoan);
            await _uow.SaveChangesAsync();
            return taiKhoan.Id;
        }

        // UC-02: Đăng nhập hệ thống
        public async Task<DangNhapResponse> DangNhapAsync(DangNhapRequest request)
        {
            if (request == null)
                throw new BadRequestException("Vui lòng nhập thông tin đăng nhập.");

            var tk = await _uow.TaiKhoans.GetByEmailOrPhoneAsync(request.TaiKhoan);
            if (tk == null)
                throw new UnauthorizedException("Thông tin đăng nhập không chính xác.");

            // Luồng 4b: Tài khoản bị khóa bởi quản lý
            if (tk.TrangThai == TrangThaiTaiKhoan.BiKhoa.ToString())
                throw new ForbiddenException("Tài khoản của bạn đang bị khóa. Vui lòng liên hệ cửa hàng để được hỗ trợ.");

            // Luồng 4c: Tạm khóa do nhập sai quá 5 lần liên tiếp
            if (tk.KhoaTamDen.HasValue && tk.KhoaTamDen.Value > DateTimeOffset.Now)
            {
                var conLai = (int)Math.Ceiling((tk.KhoaTamDen.Value - DateTimeOffset.Now).TotalMinutes);
                throw new ForbiddenException(string.Format("Tài khoản đang tạm khóa đăng nhập do nhập sai nhiều lần. Vui lòng thử lại sau {0} phút.", Math.Max(1, conLai)));
            }

            if (!PasswordHasher.Verify(request.MatKhau, tk.MatKhauHash))
            {
                tk.SoLanDangNhapSai += 1;
                if (tk.SoLanDangNhapSai >= MaxFailedAttempts)
                {
                    tk.KhoaTamDen = DateTimeOffset.Now.AddMinutes(LockoutMinutes);
                    tk.SoLanDangNhapSai = 0;
                    await _uow.SaveChangesAsync();
                    throw new ForbiddenException(string.Format("Bạn đã nhập sai quá {0} lần liên tiếp. Chức năng đăng nhập bị tạm khóa trong {1} phút.", MaxFailedAttempts, LockoutMinutes));
                }

                await _uow.SaveChangesAsync();
                // Luồng 4a: Thông báo lỗi chung, không nêu rõ trường nào sai
                throw new UnauthorizedException("Thông tin đăng nhập không chính xác.");
            }

            // Đăng nhập thành công: đặt lại bộ đếm sai
            if (tk.SoLanDangNhapSai > 0 || tk.KhoaTamDen.HasValue)
            {
                tk.SoLanDangNhapSai = 0;
                tk.KhoaTamDen = null;
                await _uow.SaveChangesAsync();
            }

            string redirectUrl = "/";
            if (tk.VaiTro == VaiTro.NhanVien.ToString()) redirectUrl = "/DonHang";
            else if (tk.VaiTro == VaiTro.QuanLy.ToString()) redirectUrl = "/Admin";

            return new DangNhapResponse
            {
                TaiKhoanId = tk.Id,
                HoTen = tk.HoTen,
                Email = tk.Email,
                VaiTro = tk.VaiTro,
                RedirectUrl = redirectUrl,
                ExpiresAt = DateTimeOffset.Now.AddHours(8)
            };
        }

        // UC-03 (Bước 3): Lấy câu hỏi bảo mật
        public async Task<CauHoiBaoMatResponse> LayCauHoiBaoMatAsync(string emailOrPhone)
        {
            if (string.IsNullOrWhiteSpace(emailOrPhone))
                throw new BadRequestException("Vui lòng nhập email hoặc số điện thoại.");

            var tk = await _uow.TaiKhoans.GetByEmailOrPhoneAsync(emailOrPhone);
            if (tk == null)
                throw new NotFoundException("Không tìm thấy tài khoản tương ứng.");

            if (string.IsNullOrWhiteSpace(tk.CauHoiBaoMat) || string.IsNullOrWhiteSpace(tk.CauTraLoiHash))
                throw new BadRequestException("Tài khoản chưa thiết lập câu hỏi bảo mật. Vui lòng liên hệ quản lý để được hỗ trợ.");

            return new CauHoiBaoMatResponse
            {
                EmailOrPhone = emailOrPhone.Trim(),
                CauHoiBaoMat = tk.CauHoiBaoMat
            };
        }

        // UC-03 (Bước 5-8): Xác minh câu trả lời và đặt lại mật khẩu
        public async Task QuenMatKhauAsync(QuenMatKhauRequest request)
        {
            if (request == null)
                throw new BadRequestException("Dữ liệu không hợp lệ.");

            var tk = await _uow.TaiKhoans.GetByEmailOrPhoneAsync(request.TaiKhoan);
            if (tk == null)
                throw new NotFoundException("Không tìm thấy tài khoản tương ứng.");

            if (string.IsNullOrWhiteSpace(tk.CauHoiBaoMat) || string.IsNullOrWhiteSpace(tk.CauTraLoiHash))
                throw new BadRequestException("Tài khoản chưa thiết lập câu hỏi bảo mật. Vui lòng liên hệ quản lý để được hỗ trợ.");

            if (tk.KhoaTamDen.HasValue && tk.KhoaTamDen.Value > DateTimeOffset.Now)
                throw new ForbiddenException("Chức năng khôi phục đang tạm khóa do trả lời sai quá nhiều lần. Vui lòng thử lại sau.");

            if (!PasswordHasher.VerifySecurityAnswer(request.CauTraLoiBaoMat, tk.CauTraLoiHash))
            {
                tk.SoLanDangNhapSai += 1;
                if (tk.SoLanDangNhapSai >= MaxFailedAttempts)
                {
                    tk.KhoaTamDen = DateTimeOffset.Now.AddMinutes(LockoutMinutes);
                    tk.SoLanDangNhapSai = 0;
                    await _uow.SaveChangesAsync();
                    throw new ForbiddenException("Bạn đã trả lời sai quá 5 lần. Chức năng tạm khóa trong 15 phút.");
                }
                await _uow.SaveChangesAsync();
                throw new BadRequestException("Câu trả lời bảo mật không chính xác.");
            }

            if (!PasswordHasher.IsStrongPassword(request.MatKhauMoi))
                throw new BadRequestException("Mật khẩu mới phải dài tối thiểu 8 ký tự, gồm chữ hoa, chữ thường và chữ số.");

            tk.MatKhauHash = PasswordHasher.Hash(request.MatKhauMoi);
            tk.SoLanDangNhapSai = 0;
            tk.KhoaTamDen = null;
            await _uow.SaveChangesAsync();
        }

        // UC-04: Đổi mật khẩu
        public async Task DoiMatKhauAsync(int taiKhoanId, DoiMatKhauRequest request)
        {
            var tk = await _uow.TaiKhoans.GetByIdAsync(taiKhoanId);
            if (tk == null)
                throw new NotFoundException("Không tìm thấy tài khoản.");

            if (!PasswordHasher.Verify(request.MatKhauCu, tk.MatKhauHash))
                throw new BadRequestException("Mật khẩu cũ không chính xác.");

            if (request.MatKhauMoi == request.MatKhauCu)
                throw new BadRequestException("Mật khẩu mới không được trùng với mật khẩu cũ.");

            if (!PasswordHasher.IsStrongPassword(request.MatKhauMoi))
                throw new BadRequestException("Mật khẩu mới phải dài tối thiểu 8 ký tự, gồm chữ hoa, chữ thường và chữ số.");

            tk.MatKhauHash = PasswordHasher.Hash(request.MatKhauMoi);
            await _uow.SaveChangesAsync();
        }

        // UC-05: Điền / cập nhật câu hỏi bảo mật
        public async Task CapNhatCauHoiBaoMatAsync(int taiKhoanId, CapNhatCauHoiBaoMatRequest request)
        {
            var tk = await _uow.TaiKhoans.GetByIdAsync(taiKhoanId);
            if (tk == null)
                throw new NotFoundException("Không tìm thấy tài khoản.");

            if (!PasswordHasher.Verify(request.MatKhauHienTai, tk.MatKhauHash))
                throw new BadRequestException("Mật khẩu xác nhận không chính xác.");

            if (string.IsNullOrWhiteSpace(request.CauTraLoiBaoMat) || request.CauTraLoiBaoMat.Trim().Length < 2)
                throw new BadRequestException("Câu trả lời bảo mật quá ngắn.");

            tk.CauHoiBaoMat = request.CauHoiBaoMat.Trim();
            tk.CauTraLoiHash = PasswordHasher.HashSecurityAnswer(request.CauTraLoiBaoMat);
            await _uow.SaveChangesAsync();
        }
    }
}
