using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data.Configurations
{
    public class LichSuKhoaTaiKhoanConfiguration : EntityTypeConfiguration<LichSuKhoaTaiKhoan>
    {
        public LichSuKhoaTaiKhoanConfiguration()
        {
            ToTable("LichSuKhoaTaiKhoan", "dbo");
            HasKey(x => x.Id);
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.TrangThaiMoi).IsRequired().HasMaxLength(20).IsUnicode(false);
            Property(x => x.LyDo).IsOptional().HasMaxLength(500).IsUnicode(true);
            Property(x => x.ThoiDiem).IsRequired().HasColumnType("datetimeoffset").HasPrecision(0);

            HasRequired(x => x.TaiKhoan)
                .WithMany(t => t.LichSuBiKhoas)
                .HasForeignKey(x => x.TaiKhoanId)
                .WillCascadeOnDelete(false);

            HasRequired(x => x.QuanLy)
                .WithMany(t => t.LichSuThucHienKhoas)
                .HasForeignKey(x => x.QuanLyId)
                .WillCascadeOnDelete(false);
        }
    }
}
