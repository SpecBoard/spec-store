using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Internal;
using SpecStore.Application.Entities;
using SpecStore.Application.Interceptors;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SpecStore.Test.Unit")]
namespace SpecStore.Application.Contexts
{
	public class ReportContext(DbContextOptions<ReportContext> options, ISystemClock clock) : DbContext(options)
	{
		public DbSet<ProjectEntity> Projects { get; set; }
		public DbSet<VersionEntity> Versions { get; set; }
		public DbSet<ReportEntity> Reports { get; set; }
		public DbSet<MetadataEntity> Metadata { get; set; }
		public DbSet<TagEntity> Tags { get; set; }
		public DbSet<FeatureEntity> Features { get; set; }
		public DbSet<RuleEntity> Rules { get; set; }
		public DbSet<ScenarioEntity> Scenarios { get; set; }
		public DbSet<StepEntity> Steps { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.AddInterceptors(new TrackInterceptor(clock));
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.HasDefaultSchema("spec-store");

			modelBuilder.Entity<ProjectEntity>().Configure();
			modelBuilder.Entity<VersionEntity>().Configure();
			modelBuilder.Entity<ReportEntity>().Configure();
			modelBuilder.Entity<MetadataEntity>().Configure();
			modelBuilder.Entity<TagEntity>().Configure();
			modelBuilder.Entity<FeatureEntity>().Configure();
			modelBuilder.Entity<RuleEntity>().Configure();
			modelBuilder.Entity<ScenarioEntity>().Configure();
			modelBuilder.Entity<StepEntity>().Configure();
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

			builder.Property(e => e.UploadedAt)
				.HasColumnName("uploaded_at")
				.IsRequired()
				.HasConversion<long>();
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
			builder.Property(e => e.Version)
				.HasColumnName("version")
				.IsRequired();
			builder.HasMany(e => e.Reports)
				.WithOne(e => e.Version)
				.HasConstraintName("FK_version_reports")
				.HasForeignKey("id_version")
				.OnDelete(DeleteBehavior.Cascade);

			builder.Property(e => e.UploadedAt)
				.HasColumnName("uploaded_at")
				.IsRequired()
				.HasConversion<long>();
		}

		public static void Configure(this EntityTypeBuilder<ReportEntity> builder)
		{
			builder.ToTable("reports");
			builder.HasKey(e => e.Id);

			builder.Property(e => e.Id)
				.HasColumnName("id")
				.ValueGeneratedOnAdd();
			builder.HasMany(e => e.Metadata)
				.WithOne()
				.HasConstraintName("FK_report_metadata")
				.HasForeignKey("id_report")
				.OnDelete(DeleteBehavior.Cascade);

			builder.Property(e => e.UploadedAt)
				.HasColumnName("uploaded_at")
				.IsRequired()
				.HasConversion<long>();
		}

		public static void Configure(this EntityTypeBuilder<MetadataEntity> builder)
		{
			builder.ToTable("metadata");
			builder.HasKey(e => e.Id);

			builder.Property(e => e.Id)
				.HasColumnName("id")
				.ValueGeneratedOnAdd();

			builder.Property(e => e.Key)
				.HasColumnName("key")
				.IsRequired();
			builder.Property(e => e.Value)
				.HasColumnName("value")
				.IsRequired();
		}

		public static void Configure(this EntityTypeBuilder<TagEntity> builder)
		{
			builder.ToTable("tags");
			builder.HasKey(e => e.Id);

			builder.Property(e => e.Id)
				.HasColumnName("id")
				.ValueGeneratedOnAdd();

			builder.Property(e => e.Tag)
				.HasColumnName("tag")
				.IsRequired();
		}

		public static void Configure(this EntityTypeBuilder<FeatureEntity> builder)
		{
			builder.ToTable("features");
			builder.HasKey(e => e.Id);

			builder.Property(e => e.Id)
				.HasColumnName("id")
				.ValueGeneratedOnAdd();

			builder.HasMany(e => e.Tags)
				.WithOne()
				.HasConstraintName("FK_feature_tag")
				.HasForeignKey("id_feature")
				.OnDelete(DeleteBehavior.Cascade);
			builder.Property(e => e.Title)
				.HasColumnName("title")
				.IsRequired();
			builder.HasMany(builder => builder.Rules)
				.WithOne()
				.HasConstraintName("FK_feature_rule")
				.HasForeignKey("id_feature")
				.OnDelete(DeleteBehavior.Cascade);
			builder.HasMany(builder => builder.Scenarios)
				.WithOne()
				.HasConstraintName("FK_feature_scenario")
				.HasForeignKey("id_feature")
				.OnDelete(DeleteBehavior.Cascade);
		}

		public static void Configure(this EntityTypeBuilder<RuleEntity> builder)
		{
			builder.ToTable("rules");
			builder.HasKey(e => e.Id);

			builder.Property(e => e.Id)
				.HasColumnName("id")
				.ValueGeneratedOnAdd();

			builder.Property(e => e.Title)
				.HasColumnName("title")
				.IsRequired();
			builder.Property(e => e.Description)
				.HasColumnName("description");
			builder.HasMany(e => e.Scenarios)
				.WithOne()
				.HasConstraintName("FK_rule_scenario")
				.HasForeignKey("id_rule")
				.OnDelete(DeleteBehavior.Cascade);
		}

		public static void Configure(this EntityTypeBuilder<ScenarioEntity> builder)
		{
			builder.ToTable("scenarios");
			builder.HasKey(e => e.Id);

			builder.Property(e => e.Id)
				.HasColumnName("id")
				.ValueGeneratedOnAdd();

			builder.HasMany(e => e.Tags)
				.WithOne()
				.HasConstraintName("FK_scenario_tag")
				.HasForeignKey("id_scenario")
				.OnDelete(DeleteBehavior.Cascade);
			builder.Property(e => e.Title)
				.HasColumnName("title")
				.IsRequired();
			builder.HasMany(e => e.Steps)
				.WithOne()
				.HasConstraintName("FK_scenario_step")
				.HasForeignKey("id_scenario")
				.OnDelete(DeleteBehavior.Cascade);
		}

		public static void Configure(this EntityTypeBuilder<StepEntity> builder)
		{
			builder.ToTable("steps");
			builder.HasKey(e => e.Id);

			builder.Property(e => e.Id)
				.HasColumnName("id")
				.ValueGeneratedOnAdd();

			builder.Property(e => e.Type)
				.HasColumnName("type")
				.IsRequired()
				.HasConversion<string>();
			builder.Property(e => e.Text)
				.HasColumnName("text")
				.IsRequired();
			builder.Property(e => e.Status)
				.HasColumnName("status")
				.IsRequired()
				.HasConversion<string>();
			builder.Property(e => e.Duration)
				.HasColumnName("duration")
				.IsRequired()
				.HasConversion<long>();
		}
	}
}
