using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PizzaStore.Data.Interfaces;

namespace PizzaStore.Data.Models
{
    public partial class PizzaStoreDBContext : DbContext, IPizzaStoreDBContext
    {
        private static Action<DbContextOptionsBuilder> s_configureConnection = options => {};

        public static Action<DbContextOptionsBuilder> ConfigureConnection
        {
            get => s_configureConnection;
            set => s_configureConnection = value ?? throw new ArgumentNullException(nameof(value));
        }
        public virtual DbSet<Pslocation> Pslocation { get; set; }
        public virtual DbSet<Psorder> Psorder { get; set; }
        public virtual DbSet<PsorderPart> PsorderPart { get; set; }
        public virtual DbSet<Psuser> Psuser { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ConfigureConnection(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pslocation>(entity =>
            {
                entity.HasKey(e => e.LocationId);

                entity.ToTable("PSLocation", "PizzaStore");

                entity.Property(e => e.LocationId).HasColumnName("LocationID");
            });

            modelBuilder.Entity<Psorder>(entity =>
            {
                entity.HasKey(e => e.OrderId);

                entity.ToTable("PSOrder", "PizzaStore");

                entity.Property(e => e.OrderId).HasColumnName("OrderID");

                entity.Property(e => e.LocationId).HasColumnName("LocationID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.Psorder)
                    .HasForeignKey(d => d.LocationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PSOrder_PSLocation");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Psorder)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PSOrder_PSUser");
            });

            modelBuilder.Entity<PsorderPart>(entity =>
            {
                entity.HasKey(e => new { e.OrderId, e.Price });

                entity.ToTable("PSOrderPart", "PizzaStore");

                entity.Property(e => e.OrderId).HasColumnName("OrderID");

                entity.Property(e => e.Price).HasColumnType("money");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.PsorderPart)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PSOrderPart_PSOrder");
            });

            modelBuilder.Entity<Psuser>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("PSUser", "PizzaStore");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.DefaultLocationId).HasColumnName("DefaultLocationID");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.DefaultLocation)
                    .WithMany(p => p.Psuser)
                    .HasForeignKey(d => d.DefaultLocationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PSUser_PSLocation");
            });
        }
    }
}
