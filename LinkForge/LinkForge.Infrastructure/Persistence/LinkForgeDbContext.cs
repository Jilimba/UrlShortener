using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UrlShortener.LinkForge.LinkForge.Domain.Entities;

namespace UrlShortener.LinkForge.LinkForge.Infrastructure.Persistence;

public class LinkForgeDbContext : DbContext
{
    public LinkForgeDbContext(
        DbContextOptions<LinkForgeDbContext> options)
        : base(options)
    {
    }
    public DbSet<Shortlink> Shortlinks => Set<Shortlink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shortlink>(entity =>
        {
            entity.HasKey(link => link.Id);

            entity.Property(link => link.OriginalUrl)
                .HasMaxLength(2048)
                .IsRequired();

            entity.Property(link => link.Code)
                .HasMaxLength(16)
                .IsRequired();

            entity.HasIndex(link => link.Code)
                .IsUnique();

            entity.Property(link => link.CreatedAtUtc)
                .IsRequired();
        });
    }
}