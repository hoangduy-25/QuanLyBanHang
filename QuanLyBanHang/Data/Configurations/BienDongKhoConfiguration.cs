using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data.Configurations
{
    public class BienDongKhoConfiguration : EntityTypeConfiguration<BienDongKho>
    {
        public BienDongKhoConfiguration()
        {
            ToTable("BienDongKho", "dbo");
            HasKey(x => x.Id);
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.Loai).IsRequired().HasMaxLength(30).IsUnicode(false);
            Property(x => x.SoLuongThayDoi).IsRequired();
            Property(x => x.SoLuongTruoc).IsRequired();
            Property(x => x.SoLuongSau).IsRequired();
            Property(x => x.GiaNhap).IsOptional().HasPrecision(18, 2);
            Property(x => x.NhaCungCap).IsOptional().HasMaxLength(200).IsUnicode(true);
            Property(x => x.LyDo).IsRequired().HasMaxLength(500).IsUnicode(true);
            Property(x => x.ThoiDiem).IsRequired().HasColumnType("datetimeoffset").HasPrecision(0);

            HasRequired(x => x.TonKho)
                .WithMany(tk => tk.BienDongKhos)
                .HasForeignKey(x => x.TonKhoId)
                .WillCascadeOnDelete(false);

            HasOptional(x => x.TaiKhoan)
                .WithMany(t => t.BienDongKhos)
                .HasForeignKey(x => x.TaiKhoanId)
                .WillCascadeOnDelete(false);

            HasOptional(x => x.DonHang)
                .WithMany(d => d.BienDongKhos)
                .HasForeignKey(x => x.DonHangId)
                .WillCascadeOnDelete(false);
        }
    }
}
