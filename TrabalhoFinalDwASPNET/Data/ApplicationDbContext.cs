using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrabalhoFinalDwASPNET.Models;

namespace TrabalhoFinalDwASPNET.Data
{
    // Classe que representa o contexto da base de dados da aplicação
    public class ApplicationDbContext : IdentityDbContext
    {
        // Construtor que recebe as opções de configuração do contexto
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet para a tabela de eventos
        public DbSet<Events> Events { get; set; }
        // DbSet para a tabela de participantes
        public DbSet<Participants> Participants { get; set; }
        // DbSet para a tabela de tags
        public DbSet<Tags> Tags { get; set; }
        // DbSet para a tabela de relação entre eventos e tags
        public DbSet<EventTag> EventTags { get; set; }

        // Método para configurar as relações entre as entidades
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração da relação entre Participants e IdentityUser
            modelBuilder.Entity<Participants>()
                .HasOne(p => p.identityUser) // Um participante tem um usuário associado
                .WithMany() // Um usuário pode ter muitos participantes associados (não configurado explicitamente)
                .HasForeignKey(p => p.UserFK); // Chave estrangeira para o usuário

            // Configuração da relação entre Participants e Events
            modelBuilder.Entity<Participants>()
                .HasOne(p => p.Event) // Um participante está associado a um evento
                .WithMany(e => e.ListaParticipants) // Um evento pode ter muitos participantes
                .HasForeignKey(p => p.EventFK); // Chave estrangeira para o evento

            // Configuração da chave composta para a relação entre Event e Tag (EventTag)
            modelBuilder.Entity<EventTag>()
                .HasKey(et => new { et.EventId, et.TagId }); // Chave composta (EventId e TagId)

            // Configuração da relação entre EventTag e Event
            modelBuilder.Entity<EventTag>()
                .HasOne(et => et.Event) // Um EventTag está associado a um evento
                .WithMany(e => e.EventTags) // Um evento pode ter muitas associações EventTag
                .HasForeignKey(et => et.EventId); // Chave estrangeira para o evento

            // Configuração da relação entre EventTag e Tag
            modelBuilder.Entity<EventTag>()
                .HasOne(et => et.Tag) // Um EventTag está associado a uma tag
                .WithMany(t => t.EventTags) // Uma tag pode ter muitas associações EventTag
                .HasForeignKey(et => et.TagId); // Chave estrangeira para a tag

            // Chama o método base para aplicar outras configurações padrão do IdentityDbContext
            base.OnModelCreating(modelBuilder);
        }
    }
}
