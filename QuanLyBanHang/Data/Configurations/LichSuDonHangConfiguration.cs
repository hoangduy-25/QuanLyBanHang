using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data.Configurations
{
    public class LichSuDonHangConfiguration : EntityTypeConfiguration<LichSuDonHang>
    {
        public LichSuDonHangConfiguration()
        {
            ToTable("LichSuDonHang", "dbo");
            HasKey(x => x.Id);
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.TrangThai).IsRequired().HasMaxLength(30).IsUnicode(false);
            Property(x => x.ThoiDiem).IsRequired().HasColumnType("datetimeoffset").HasPrecision(0);
            Property(x => x.GhiChu).IsOptional().HasMaxLength(500).IsUnicode(true);

            HasRequired(x => x.DonHang)
                .WithMany(d => d.LichSuDonHangs)
                .HasForeignKey(x => x.DonHangId)
                .WillCascadeOnDelete(false);

            HasOptional(x => x.TaiKhoan)
                .WithMany(t => t.LichSuDonHangs)
                .HasForeignKey(x => x.TaiKhoanId)
                .WillCascadeOnDelete(false);
        }
    }
}
