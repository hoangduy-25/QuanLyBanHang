using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data.Configurations
{
    public class GioHangConfiguration : EntityTypeConfiguration<GioHang>
    {
        public GioHangConfiguration()
        {
            ToTable("GioHang", "dbo");
            HasKey(x => x.Id);
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.TaiKhoanId).IsRequired()
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("UQ_GioHang_TaiKhoan") { IsUnique = true }));

            HasRequired(x => x.TaiKhoan)
                .WithMany(t => t.GioHangs)
                .HasForeignKey(x => x.TaiKhoanId)
                .WillCascadeOnDelete(false);
        }
    }
}
