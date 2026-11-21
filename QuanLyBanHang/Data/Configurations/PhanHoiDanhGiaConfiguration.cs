using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data.Configurations
{
    public class PhanHoiDanhGiaConfiguration : EntityTypeConfiguration<PhanHoiDanhGia>
    {
        public PhanHoiDanhGiaConfiguration()
        {
            ToTable("PhanHoiDanhGia", "dbo");
            HasKey(x => x.Id);
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.DanhGiaId).IsRequired()
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("UQ_PhanHoiDanhGia_DanhGia") { IsUnique = true }));
            Property(x => x.NoiDung).IsRequired().HasMaxLength(2000).IsUnicode(true);
            Property(x => x.NgayTao).IsRequired().HasColumnType("datetimeoffset").HasPrecision(0);
            Property(x => x.NgayCapNhat).IsOptional().HasColumnType("datetimeoffset").HasPrecision(0);

            HasRequired(x => x.DanhGia)
                .WithMany(dg => dg.PhanHoiDanhGias)
                .HasForeignKey(x => x.DanhGiaId)
                .WillCascadeOnDelete(false);

            HasRequired(x => x.TaiKhoan)
                .WithMany(t => t.PhanHoiDanhGias)
                .HasForeignKey(x => x.TaiKhoanId)
                .WillCascadeOnDelete(false);
        }
    }
}
