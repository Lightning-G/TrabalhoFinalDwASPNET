using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrabalhoFinalDwASPNET.Models;

namespace TrabalhoFinalDwASPNET.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Events> Events { get; set; }
        public DbSet<Participants> Participants { get; set; }

        public DbSet<Tags> Tags { get; set; }
        public DbSet<EventTag> EventTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the relationships
            modelBuilder.Entity<Participants>()
                .HasOne(p => p.identityUser)
                .WithMany()
                .HasForeignKey(p => p.UserFK);

            modelBuilder.Entity<Participants>()
                .HasOne(p => p.Event)
                .WithMany(e => e.ListaParticipants)
                .HasForeignKey(p => p.EventFK);

            modelBuilder.Entity<EventTag>()
            .HasKey(et => new { et.EventId, et.TagId });

            modelBuilder.Entity<EventTag>()
                .HasOne(et => et.Event)
                .WithMany(e => e.EventTags)
                .HasForeignKey(et => et.EventId);

            modelBuilder.Entity<EventTag>()
                .HasOne(et => et.Tag)
                .WithMany(t => t.EventTags)
                .HasForeignKey(et => et.TagId);


            base.OnModelCreating(modelBuilder);
        }
    }
}