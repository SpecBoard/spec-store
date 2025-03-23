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

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.HasDefaultSchema("spec-store");

			modelBuilder.Entity<ProjectEntity>().Configure();
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
	}
}
