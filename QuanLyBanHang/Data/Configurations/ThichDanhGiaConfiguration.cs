using System.Data.Entity.ModelConfiguration;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data.Configurations
{
    public class ThichDanhGiaConfiguration : EntityTypeConfiguration<ThichDanhGia>
    {
        public ThichDanhGiaConfiguration()
        {
            ToTable("ThichDanhGia", "dbo");
            HasKey(x => new { x.TaiKhoanId, x.DanhGiaId });

            Property(x => x.NgayTao).IsRequired().HasColumnType("datetimeoffset").HasPrecision(0);

            HasRequired(x => x.TaiKhoan)
                .WithMany(t => t.ThichDanhGias)
                .HasForeignKey(x => x.TaiKhoanId)
                .WillCascadeOnDelete(false);

            HasRequired(x => x.DanhGia)
                .WithMany(dg => dg.ThichDanhGias)
                .HasForeignKey(x => x.DanhGiaId)
                .WillCascadeOnDelete(false);
        }
    }
}
