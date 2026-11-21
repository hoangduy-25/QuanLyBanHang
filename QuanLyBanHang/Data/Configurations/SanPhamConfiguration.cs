using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data.Configurations
{
    public class SanPhamConfiguration : EntityTypeConfiguration<SanPham>
    {
        public SanPhamConfiguration()
        {
            ToTable("SanPham", "dbo");
            HasKey(x => x.Id);
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.MaSanPham).IsRequired().HasMaxLength(50).IsUnicode(false);
            Property(x => x.Ten).IsRequired().HasMaxLength(200).IsUnicode(true);
            Property(x => x.MoTa).IsRequired().HasMaxLength(2000).IsUnicode(true);
            Property(x => x.HinhAnh).IsOptional().HasMaxLength(500).IsUnicode(true);
            Property(x => x.GiaBan).IsRequired().HasPrecision(18, 2);
            Property(x => x.HienThi).IsRequired();
            Property(x => x.IsDeleted).IsRequired();
            Property(x => x.NgayTao).IsRequired().HasColumnType("datetimeoffset").HasPrecision(0);

            HasRequired(x => x.DanhMuc)
                .WithMany(d => d.SanPhams)
                .HasForeignKey(x => x.DanhMucId)
                .WillCascadeOnDelete(false);
        }
    }
}
