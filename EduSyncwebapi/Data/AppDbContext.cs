using System;
using System.Collections.Generic;
using EduSyncwebapi.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSyncwebapi.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AssessmentModel> AssessmentModels { get; set; }

    public virtual DbSet<Coursemodel> Coursemodels { get; set; }

    public virtual DbSet<ResultModel> ResultModels { get; set; }

    public virtual DbSet<UserModel> UserModels { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer(" Data Source=(localdb)\\ProjectModels;Initial Catalog=edusyncdatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssessmentModel>(entity =>
        {
            entity.HasKey(e => e.AssessmentId);

            entity.ToTable("AssessmentModel");

            entity.Property(e => e.AssessmentId).ValueGeneratedNever();
            entity.Property(e => e.Questions).HasMaxLength(50);
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Course).WithMany(p => p.AssessmentModels)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK_AssessmentModel_Coursemodel");
        });

        modelBuilder.Entity<Coursemodel>(entity =>
        {
            entity.HasKey(e => e.Cousreld);

            entity.ToTable("Coursemodel");

            entity.Property(e => e.Cousreld).ValueGeneratedNever();
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Instructor).WithMany(p => p.Coursemodels)
                .HasForeignKey(d => d.InstructorId)
                .HasConstraintName("FK_Coursemodel_User Model");
        });

        modelBuilder.Entity<ResultModel>(entity =>
        {
            entity.HasKey(e => e.ResultId);

            entity.ToTable("ResultModel");

            entity.Property(e => e.ResultId).ValueGeneratedNever();
            entity.Property(e => e.AttemptDate).HasColumnType("datetime");

            entity.HasOne(d => d.Assessment).WithMany(p => p.ResultModels)
                .HasForeignKey(d => d.AssessmentId)
                .HasConstraintName("FK_ResultModel_AssessmentModel");

            entity.HasOne(d => d.User).WithMany(p => p.ResultModels)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ResultModel_User Model");
        });

        modelBuilder.Entity<UserModel>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("User Model");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
