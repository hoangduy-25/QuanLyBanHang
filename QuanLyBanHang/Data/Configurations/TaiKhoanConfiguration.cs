using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data.Configurations
{
    public class TaiKhoanConfiguration : EntityTypeConfiguration<TaiKhoan>
    {
        public TaiKhoanConfiguration()
        {
            ToTable("TaiKhoan", "dbo");
            HasKey(x => x.Id);
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.HoTen).IsRequired().HasMaxLength(100).IsUnicode(true);
            Property(x => x.Email).IsRequired().HasMaxLength(254).IsUnicode(false)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("UQ_TaiKhoan_Email") { IsUnique = true }));
            Property(x => x.SoDienThoai).IsRequired().HasMaxLength(20).IsUnicode(false)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("UQ_TaiKhoan_SoDienThoai") { IsUnique = true }));
            Property(x => x.MatKhauHash).IsRequired().HasMaxLength(255).IsUnicode(true);
            Property(x => x.VaiTro).IsRequired().HasMaxLength(20).IsUnicode(false);
            Property(x => x.TrangThai).IsRequired().HasMaxLength(20).IsUnicode(false);
            Property(x => x.NgaySinh).IsOptional().HasColumnType("date");
            Property(x => x.GioiTinh).IsOptional().HasMaxLength(10).IsUnicode(true);
            Property(x => x.AnhDaiDien).IsOptional().HasMaxLength(500).IsUnicode(true);
            Property(x => x.CauHoiBaoMat).IsOptional().HasMaxLength(255).IsUnicode(true);
            Property(x => x.CauTraLoiHash).IsOptional().HasMaxLength(255).IsUnicode(true);
            Property(x => x.LyDoKhoa).IsOptional().HasMaxLength(500).IsUnicode(true);
            Property(x => x.SoLanDangNhapSai).IsRequired();
            Property(x => x.KhoaTamDen).IsOptional().HasColumnType("datetimeoffset").HasPrecision(0);
            Property(x => x.NgayTao).IsRequired().HasColumnType("datetimeoffset").HasPrecision(0);
        }
    }
}
