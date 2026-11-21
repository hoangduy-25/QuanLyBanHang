using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data.Configurations
{
    public class DanhMucConfiguration : EntityTypeConfiguration<DanhMuc>
    {
        public DanhMucConfiguration()
        {
            ToTable("DanhMuc", "dbo");
            HasKey(x => x.Id);
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.Ten).IsRequired().HasMaxLength(150).IsUnicode(true);
            Property(x => x.MoTa).IsOptional().HasMaxLength(1000).IsUnicode(true);
            Property(x => x.HinhAnh).IsOptional().HasMaxLength(500).IsUnicode(true);
            Property(x => x.HienThi).IsRequired();
            Property(x => x.IsDeleted).IsRequired();
            Property(x => x.NgayTao).IsRequired().HasColumnType("datetimeoffset").HasPrecision(0);
        }
    }
}
