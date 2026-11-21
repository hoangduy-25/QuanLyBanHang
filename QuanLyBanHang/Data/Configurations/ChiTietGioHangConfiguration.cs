using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data.Configurations
{
    public class ChiTietGioHangConfiguration : EntityTypeConfiguration<ChiTietGioHang>
    {
        public ChiTietGioHangConfiguration()
        {
            ToTable("ChiTietGioHang", "dbo");
            HasKey(x => x.Id);
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.GioHangId).IsRequired()
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("UQ_ChiTietGioHang_GioHang_SanPham", 1) { IsUnique = true }));
            Property(x => x.SanPhamId).IsRequired()
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("UQ_ChiTietGioHang_GioHang_SanPham", 2) { IsUnique = true }));

            Property(x => x.SoLuong).IsRequired();
            Property(x => x.DonGiaLucThem).IsRequired().HasPrecision(18, 2);

            HasRequired(x => x.GioHang)
                .WithMany(g => g.ChiTietGioHangs)
                .HasForeignKey(x => x.GioHangId)
                .WillCascadeOnDelete(false);

            HasRequired(x => x.SanPham)
                .WithMany(s => s.ChiTietGioHangs)
                .HasForeignKey(x => x.SanPhamId)
                .WillCascadeOnDelete(false);
        }
    }
}
