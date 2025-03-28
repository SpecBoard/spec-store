using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpecStore.Application.Entities;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SpecStore.Test.Unit")]
namespace SpecStore.Application.Contexts
{
	public class ReportContext(DbContextOptions<ReportContext> options) : DbContext(options)
	{
		public DbSet<ProjectEntity> Projects { get; set; }
		public DbSet<VersionEntity> Versions { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.HasDefaultSchema("spec-store");

			modelBuilder.Entity<ProjectEntity>().Configure();
			modelBuilder.Entity<VersionEntity>().Configure();
		}
	}

	file static class ReportContextExtensions
	{
		public static void Configure(this EntityTypeBuilder<ProjectEntity> builder)
		{
			builder.ToTable("projects");
			builder.HasKey(e => e.Key);

			builder.Property(e => e.Key)
				.HasColumnName("key")
				.IsRequired();
		}

		public static void Configure(this EntityTypeBuilder<VersionEntity> builder)
		{
			builder.ToTable("versions");
			builder.HasKey(e => e.Id);

			builder.Property(e => e.Id)
				.HasColumnName("id")
				.ValueGeneratedOnAdd();

			builder.HasOne(e => e.Project)
				.WithMany(e => e.Versions)
				.HasConstraintName("FK_project_versions")
				.HasForeignKey("id_project")
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);

			builder.Property(b => b.Version)
				.HasColumnName("version")
				.IsRequired();
		}
	}
}
