using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data.Configurations
{
    public class DanhGiaConfiguration : EntityTypeConfiguration<DanhGia>
    {
        public DanhGiaConfiguration()
        {
            ToTable("DanhGia", "dbo");
            HasKey(x => x.Id);
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.SoSao).IsRequired();
            Property(x => x.NoiDung).IsRequired().HasMaxLength(2000).IsUnicode(true);
            Property(x => x.HinhAnh).IsOptional().HasMaxLength(500).IsUnicode(true);
            Property(x => x.DaChinhSua).IsRequired();
            Property(x => x.NgayTao).IsRequired().HasColumnType("datetimeoffset").HasPrecision(0);
            Property(x => x.NgayCapNhat).IsOptional().HasColumnType("datetimeoffset").HasPrecision(0);
            Property(x => x.LyDoXoa).IsOptional().HasMaxLength(500).IsUnicode(true);
            Property(x => x.IsDeleted).IsRequired();

            HasRequired(x => x.TaiKhoan)
                .WithMany(t => t.DanhGias)
                .HasForeignKey(x => x.TaiKhoanId)
                .WillCascadeOnDelete(false);

            HasRequired(x => x.SanPham)
                .WithMany(s => s.DanhGias)
                .HasForeignKey(x => x.SanPhamId)
                .WillCascadeOnDelete(false);

            HasRequired(x => x.DonHang)
                .WithMany(d => d.DanhGias)
                .HasForeignKey(x => x.DonHangId)
                .WillCascadeOnDelete(false);
        }
    }
}
