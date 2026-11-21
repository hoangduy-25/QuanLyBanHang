using System.Data.Entity.ModelConfiguration;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data.Configurations
{
    public class YeuThichSanPhamConfiguration : EntityTypeConfiguration<YeuThichSanPham>
    {
        public YeuThichSanPhamConfiguration()
        {
            ToTable("YeuThichSanPham", "dbo");
            HasKey(x => new { x.TaiKhoanId, x.SanPhamId });

            Property(x => x.NgayTao).IsRequired().HasColumnType("datetimeoffset").HasPrecision(0);

            HasRequired(x => x.TaiKhoan)
                .WithMany(t => t.YeuThichSanPhams)
                .HasForeignKey(x => x.TaiKhoanId)
                .WillCascadeOnDelete(false);

            HasRequired(x => x.SanPham)
                .WithMany(s => s.YeuThichSanPhams)
                .HasForeignKey(x => x.SanPhamId)
                .WillCascadeOnDelete(false);
        }
    }
}
