using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data.Configurations
{
    public class DiaChiNhanHangConfiguration : EntityTypeConfiguration<DiaChiNhanHang>
    {
        public DiaChiNhanHangConfiguration()
        {
            ToTable("DiaChiNhanHang", "dbo");
            HasKey(x => x.Id);
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.TenNguoiNhan).IsRequired().HasMaxLength(100).IsUnicode(true);
            Property(x => x.SoDienThoai).IsRequired().HasMaxLength(20).IsUnicode(false);
            Property(x => x.TinhThanh).IsRequired().HasMaxLength(100).IsUnicode(true);
            Property(x => x.QuanHuyen).IsRequired().HasMaxLength(100).IsUnicode(true);
            Property(x => x.PhuongXa).IsRequired().HasMaxLength(100).IsUnicode(true);
            Property(x => x.DiaChiCuThe).IsRequired().HasMaxLength(300).IsUnicode(true);
            Property(x => x.MacDinh).IsRequired();

            HasRequired(x => x.TaiKhoan)
                .WithMany(t => t.DiaChiNhanHangs)
                .HasForeignKey(x => x.TaiKhoanId)
                .WillCascadeOnDelete(false);
        }
    }
}
