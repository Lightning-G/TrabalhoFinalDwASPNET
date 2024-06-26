﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Xml.Linq;

namespace TrabalhoFinalDwASPNET.Models
{
    public class Events
    {
        public Events()
        {
            // Inicializar a lista de Participantes do evento
            ListaParticipants = new HashSet<Participants>();
        }

        // Identificador único do evento (chave primária)
        public int Id { get; set; }

        // Identificador do anfitrião do evento
        public string host_id { get; set; }

        // Data de criação do evento
        public DateTime created_at { get; set; }

        /// <summary>
        /// Nome do evento
        /// </summary>
        [Required(ErrorMessage = "O nome do evento é de preenchimento obrigatório")]
        [StringLength(maximumLength: 30)]
        [Display(Name = "Nome do evento")]
        public string title { get; set; }

        /// <summary>
        /// Descrição do evento
        /// </summary>
        [Display(Name = "Descrição do evento")]
        public string Description { get; set; }

        /// <summary>
        /// Imagem do evento
        /// </summary>
        [Column(TypeName = "nvarchar(MAX)")]
        [DefaultValue("('https://www.wolflair.com/wp-content/uploads/2017/01/placeholder.jpg')")]
        public string Image { get; set; }

        /// <summary>
        /// Data de começo do evento
        /// </summary>
        [Required(ErrorMessage = "A data inicial é de preenchimento obrigatório")]
        [Display(Name = "Data Inicial")]
        //[RegularExpression(@"^(\d{2})/(\d{2})/(\d{4}) (\d{2}):(\d{2})$", ErrorMessage = "Invalid date and time format. Use dd/MM/yyyy HH:mm")]
        public DateTime start_time { get; set; }

        /// <summary>
        /// Data do término do evento
        /// </summary>
        [Required(ErrorMessage = "A data final é de preenchimento obrigatório")]
        //[RegularExpression(@"^(0[1-9]|[1-2][0-9]|3[01])/(0[1-9]|1[0-2])/(202[3-9]|2030)$")]
        //[RegularExpression(@"^(\d{2})/(\d{2})/(\d{4}) (\d{2}):(\d{2})$", ErrorMessage = "Invalid date and time format. Use dd/MM/yyyy HH:mm")]
        public DateTime end_time { get; set; }

        /// <summary>
        /// Local do evento
        /// </summary>
        [Display(Name = "Local")]
        public string location { get; set; }

        /// <summary>
        /// Privacidade do evento
        /// </summary>
        [Required(ErrorMessage = "A privacidade do evento é de preenchimento obrigatório")]
        [Display(Name = "Privacidade do evento")]
        public bool is_private { get; set; }

        /// <summary>
        /// Máximo de participantes do evento
        /// </summary>
        [Required(ErrorMessage = "O Máximo de participantes do evento é de preenchimento obrigatório")]
        [Display(Name = "Máximo de Participantes")]
        public int maxParticipants { get; set; }

        /* ++++++++++++++++++++++++++++++++++++++++++++++++
        * Relacionamentos associados aos Eventos
        */

        /// <summary>
        /// Lista dos Participantes associados ao Evento
        /// </summary>
        public ICollection<Participants> ListaParticipants { get; set; }

        // Lista de tags associadas ao evento
        public ICollection<EventTag> EventTags { get; set; } = new List<EventTag>();
    }
}
