using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data.Configurations
{
    public class MaGiamGiaConfiguration : EntityTypeConfiguration<MaGiamGia>
    {
        public MaGiamGiaConfiguration()
        {
            ToTable("MaGiamGia", "dbo");
            HasKey(x => x.Id);
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.Ma).IsRequired().HasMaxLength(50).IsUnicode(false);
            Property(x => x.MoTa).IsOptional().HasMaxLength(500).IsUnicode(true);
            Property(x => x.Loai).IsRequired().HasMaxLength(20).IsUnicode(false);
            Property(x => x.GiaTri).IsRequired().HasPrecision(18, 2);
            Property(x => x.GiamToiDa).IsOptional().HasPrecision(18, 2);
            Property(x => x.DonToiThieu).IsRequired().HasPrecision(18, 2);
            Property(x => x.NgayBatDau).IsRequired().HasColumnType("datetimeoffset").HasPrecision(0);
            Property(x => x.NgayKetThuc).IsRequired().HasColumnType("datetimeoffset").HasPrecision(0);
            Property(x => x.SoLuotToiDa).IsRequired();
            Property(x => x.SoLuotDaDung).IsRequired();
            Property(x => x.HienThi).IsRequired();
            Property(x => x.IsDeleted).IsRequired();
        }
    }
}
