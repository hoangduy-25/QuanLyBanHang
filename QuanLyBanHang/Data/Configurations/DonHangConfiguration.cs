using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data.Configurations
{
    public class DonHangConfiguration : EntityTypeConfiguration<DonHang>
    {
        public DonHangConfiguration()
        {
            ToTable("DonHang", "dbo");
            HasKey(x => x.Id);
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.MaDon).IsRequired().HasMaxLength(50).IsUnicode(false)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("UQ_DonHang_MaDon") { IsUnique = true }));
            Property(x => x.NgayDat).IsRequired().HasColumnType("datetimeoffset").HasPrecision(0);
            Property(x => x.TrangThai).IsRequired().HasMaxLength(30).IsUnicode(false);
            Property(x => x.PhuongThucThanhToan).IsRequired().HasMaxLength(30).IsUnicode(false);
            Property(x => x.DaThanhToan).IsRequired();
            Property(x => x.TenNguoiNhan).IsRequired().HasMaxLength(100).IsUnicode(true);
            Property(x => x.SoDienThoaiNhan).IsRequired().HasMaxLength(20).IsUnicode(false);
            Property(x => x.DiaChiGiaoHang).IsRequired().HasMaxLength(500).IsUnicode(true);
            Property(x => x.TamTinh).IsRequired().HasPrecision(18, 2);
            Property(x => x.TienGiam).IsRequired().HasPrecision(18, 2);
            Property(x => x.TongThanhToan)
                .HasPrecision(18, 2)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            Property(x => x.LyDoHuy).IsOptional().HasMaxLength(500).IsUnicode(true);

            HasRequired(x => x.TaiKhoan)
                .WithMany(t => t.DonHangs)
                .HasForeignKey(x => x.TaiKhoanId)
                .WillCascadeOnDelete(false);

            HasOptional(x => x.MaGiamGia)
                .WithMany(m => m.DonHangs)
                .HasForeignKey(x => x.MaGiamGiaId)
                .WillCascadeOnDelete(false);
        }
    }
}
