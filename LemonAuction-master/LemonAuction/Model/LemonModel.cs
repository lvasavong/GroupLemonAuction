using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;


namespace LemonAuction.Models
{
    public partial class LemonContext : IdentityDbContext<Identity.LemonUser>
    {
        
        public LemonContext() : base() {}
        public LemonContext(DbContextOptions<LemonContext> options)
            : base(options)
        { }
        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     optionsBuilder.UseNpgsql(Microsoft.Extensions.Configuration..Microsoft.Extensions.Configuration GetConnectionString("LemonAuctionDB"));
        // }
        // public DbSet<LemonUser> LemonUsers { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Product> Products { get; set; }
        // public DbSet<History> LemonUsersStats { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.Entity<LemonUser>();
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Bid>(eb =>
            {
                eb.Property(b => b.BidAmount).HasColumnType("decimal(11,2)");
                eb.Property(b => b.WinningBid).HasColumnType("decimal(11,2)");
                eb.Property(p => p.MadeAt).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            });

            modelBuilder.Entity<Product>(eb =>
            {
                // eb.Property(p => p.name, p.category, p.description).HasColumnType("TEXT");
                eb.Property(p => p.MinimumBid).HasColumnType("decimal(11,2)");
                eb.Property(p => p.Image).HasColumnType("bytea");
                eb.Property(p => p.BiddingStartTime).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
                eb.Property(p => p.BiddingEndTime).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

                // eb.Property(p => p.biddingStartTime).HasColumnType("DATETIME");
                // eb.Property(p => p.stillOnAuction).HasColumnType("BOOLEAN");
            });

            // modelBuilder.Entity<Identity.LemonUser>()
            //     .HasKey(c => c.Id);
            modelBuilder.Entity<Bid>()
                .HasKey(c => c.BidId);
            modelBuilder.Entity<Product>()
                .HasKey(c => c.ProductId);
            // modelBuilder.Entity<History>()
            //     .HasKey(c => c.HistoryId);

            // modelBuilder.Entity<Bid>()
            //     .HasIndex(b => new { b.Bidder, b.OnProduct, b.MadeAt}).IsUnique();
            
          

            modelBuilder.Entity<Identity.LemonUser>()
                .HasMany<Bid>(u => u.Bids)
                .WithOne(b => b.Bidder);
            
            
             modelBuilder.Entity<Identity.LemonUser>()
                .HasMany<Product>(u => u.ProductsPutUp)
                .WithOne(p => p.Seller);

            modelBuilder.Entity<Product>()
                .HasMany<Bid>(p => p.Bids)
                .WithOne(b => b.OnProduct);

            modelBuilder.Entity<Identity.LemonUser>()
                .Property(u => u.DateCreated)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Bid>()
                .Property(b => b.MadeAt)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Product>()
                .Property(p => p.BiddingStartTime)
                .ValueGeneratedOnAdd();
        }
    }


    

    public class Bid
    {
        public int BidId { get; set; }

        // RELATIONAL?
        public Identity.LemonUser Bidder { get; set; }
        public Product OnProduct { get; set; }

        public DateTime MadeAt { get; set; }
        public decimal BidAmount { get; set; }
        public bool WinningBid { get; set; }
    }

    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }

        // RELATIONAL?
        public Identity.LemonUser Seller { get; set; }

        public string Description { get; set; }
        public decimal MinimumBid { get; set; }
        public ICollection<Bid> Bids { get; } = new List<Bid>();
        // public decimal CurrentPrice { get; set; }
        public DateTime BiddingStartTime { get; set; }
        public DateTime BiddingEndTime { get; set; }
        public bool Active { 
            get {
                return DateTime.Compare(DateTime.UtcNow, BiddingEndTime) < 0;
            }
        }
        
        public byte[] Image { get; set; }
    }

    // public class History
    // {
      
    // }
}