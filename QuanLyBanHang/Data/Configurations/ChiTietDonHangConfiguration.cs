using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data.Configurations
{
    public class ChiTietDonHangConfiguration : EntityTypeConfiguration<ChiTietDonHang>
    {
        public ChiTietDonHangConfiguration()
        {
            ToTable("ChiTietDonHang", "dbo");
            HasKey(x => x.Id);
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.DonHangId).IsRequired()
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("UQ_ChiTietDonHang_DonHang_SanPham", 1) { IsUnique = true }));
            Property(x => x.SanPhamId).IsRequired()
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("UQ_ChiTietDonHang_DonHang_SanPham", 2) { IsUnique = true }));

            Property(x => x.DonGia).IsRequired().HasPrecision(18, 2);
            Property(x => x.SoLuong).IsRequired();
            Property(x => x.ThanhTien)
                .HasPrecision(18, 2)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            HasRequired(x => x.DonHang)
                .WithMany(d => d.ChiTietDonHangs)
                .HasForeignKey(x => x.DonHangId)
                .WillCascadeOnDelete(false);

            HasRequired(x => x.SanPham)
                .WithMany(s => s.ChiTietDonHangs)
                .HasForeignKey(x => x.SanPhamId)
                .WillCascadeOnDelete(false);
        }
    }
}
