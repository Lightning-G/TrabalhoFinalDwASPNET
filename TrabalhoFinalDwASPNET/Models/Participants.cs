using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrabalhoFinalDwASPNET.Models
{
    public class Participants
    {

        public int Id { get; set; }



        /* ++++++++++++++++++++++++++++++++++++++++++ 
        Criação das chaves forasteiras
        ++++++++++++++++++++++++++++++++++++++++++ */

        /// <summary>
        /// FK para o User_ID
        /// </summary>
        [ForeignKey(nameof(identityUser))]
        public string UserFK { get; set; }
        public IdentityUser identityUser { get; set; }

        /// <summary>
        /// FK para o Event_ID
        /// </summary>
        [ForeignKey(nameof(Event))]
        public int EventFK { get; set; }
        public Events Event { get; set; }

    }
}
