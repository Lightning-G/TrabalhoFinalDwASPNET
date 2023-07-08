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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the relationships
            modelBuilder.Entity<Participants>()
                .HasOne(p => p.identityUser)
                .WithMany()
                .HasForeignKey(p => p.UserFK);

            modelBuilder.Entity<Participants>()
                .HasOne(p => p.Event)
                .WithMany(e => e.listaParticipants)
                .HasForeignKey(p => p.EventFK);

            base.OnModelCreating(modelBuilder);
        }
    }
}