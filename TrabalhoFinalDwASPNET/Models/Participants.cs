using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrabalhoFinalDwASPNET.Models
{
    // Classe representando os Participantes
    public class Participants
    {
        // Propriedade Id, chave primária
        public int Id { get; set; }

        /* ++++++++++++++++++++++++++++++++++++++++++ 
        Criação das chaves estrangeiras
        ++++++++++++++++++++++++++++++++++++++++++ */

        /// <summary>
        /// FK para o User_ID
        /// </summary>
        [ForeignKey(nameof(identityUser))]
        // Propriedade UserFK, chave estrangeira para o usuário
        public string UserFK { get; set; }
        // Propriedade de navegação para acessar o usuário associado
        public IdentityUser identityUser { get; set; }

        /// <summary>
        /// FK para o Event_ID
        /// </summary>
        [ForeignKey(nameof(Event))]
        // Propriedade EventFK, chave estrangeira para o evento
        public int EventFK { get; set; }
        // Propriedade de navegação para acessar o evento associado
        public Events Event { get; set; }
    }
}
