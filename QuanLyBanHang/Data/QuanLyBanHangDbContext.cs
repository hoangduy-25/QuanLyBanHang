using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Linq;
using QuanLyBanHang.Data.Configurations;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data
{
    /// <summary>
    /// DbContext kết nối SQL Server bằng Entity Framework 6 (LINQ to Entities).
    /// Ánh xạ 100% với 17 bảng trong database_dong_nhat_v2.sql.
    /// </summary>
    public class QuanLyBanHangDbContext : DbContext
    {
        static QuanLyBanHangDbContext()
        {
            // Dùng trực tiếp CSDL đã tạo bằng script database_dong_nhat_v2.sql,
            // không tự động drop/tạo bảng hay kiểm tra bảng __MigrationHistory.
            Database.SetInitializer<QuanLyBanHangDbContext>(null);
        }

        public QuanLyBanHangDbContext()
            : base("name=QuanLyBanHangConnection")
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        // 17 DbSets tương ứng 17 bảng trong SQL Server
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<LichSuKhoaTaiKhoan> LichSuKhoaTaiKhoans { get; set; }
        public DbSet<DiaChiNhanHang> DiaChiNhanHangs { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<TonKho> TonKhos { get; set; }
        public DbSet<BienDongKho> BienDongKhos { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }
        public DbSet<YeuThichSanPham> YeuThichSanPhams { get; set; }
        public DbSet<MaGiamGia> MaGiamGias { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<LichSuDonHang> LichSuDonHangs { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }
        public DbSet<PhanHoiDanhGia> PhanHoiDanhGias { get; set; }
        public DbSet<ThichDanhGia> ThichDanhGias { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Không tự động chuyển tên bảng sang số nhiều tiếng Anh
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Đăng ký cấu hình Fluent API cho toàn bộ 17 bảng
            modelBuilder.Configurations.Add(new TaiKhoanConfiguration());
            modelBuilder.Configurations.Add(new LichSuKhoaTaiKhoanConfiguration());
            modelBuilder.Configurations.Add(new DiaChiNhanHangConfiguration());
            modelBuilder.Configurations.Add(new DanhMucConfiguration());
            modelBuilder.Configurations.Add(new SanPhamConfiguration());
            modelBuilder.Configurations.Add(new TonKhoConfiguration());
            modelBuilder.Configurations.Add(new BienDongKhoConfiguration());
            modelBuilder.Configurations.Add(new GioHangConfiguration());
            modelBuilder.Configurations.Add(new ChiTietGioHangConfiguration());
            modelBuilder.Configurations.Add(new YeuThichSanPhamConfiguration());
            modelBuilder.Configurations.Add(new MaGiamGiaConfiguration());
            modelBuilder.Configurations.Add(new DonHangConfiguration());
            modelBuilder.Configurations.Add(new ChiTietDonHangConfiguration());
            modelBuilder.Configurations.Add(new LichSuDonHangConfiguration());
            modelBuilder.Configurations.Add(new DanhGiaConfiguration());
            modelBuilder.Configurations.Add(new PhanHoiDanhGiaConfiguration());
            modelBuilder.Configurations.Add(new ThichDanhGiaConfiguration());
        }

        /// <summary>
        /// Gọi Table-Valued Function dbo.fn_BaoCaoTongQuan(@TuNgay, @DenNgay) phục vụ UC-58.
        /// T có thể là BaoCaoTongQuanDTO (sẽ tạo ở Bước 3).
        /// </summary>
        public T LayBaoCaoTongQuan<T>(DateTimeOffset? tuNgay, DateTimeOffset? denNgay) where T : class
        {
            var pTuNgay = new SqlParameter("@TuNgay", (object)tuNgay ?? DBNull.Value);
            var pDenNgay = new SqlParameter("@DenNgay", (object)denNgay ?? DBNull.Value);

            return Database.SqlQuery<T>(
                "SELECT * FROM dbo.fn_BaoCaoTongQuan(@TuNgay, @DenNgay)",
                pTuNgay, pDenNgay
            ).FirstOrDefault();
        }
    }
}
