using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using QuanLyBanHang.Models.Entities;

namespace QuanLyBanHang.Data.Configurations
{
    public class TonKhoConfiguration : EntityTypeConfiguration<TonKho>
    {
        public TonKhoConfiguration()
        {
            ToTable("TonKho", "dbo");
            HasKey(x => x.Id);
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.SanPhamId).IsRequired()
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("UQ_TonKho_SanPham") { IsUnique = true }));
            Property(x => x.SoLuongTon).IsRequired();
            Property(x => x.NguongToiThieu).IsRequired();

            HasRequired(x => x.SanPham)
                .WithMany(s => s.TonKhos)
                .HasForeignKey(x => x.SanPhamId)
                .WillCascadeOnDelete(false);
        }
    }
}
