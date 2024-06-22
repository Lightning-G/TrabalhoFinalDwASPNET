namespace TrabalhoFinalDwASPNET.Models
{
    // Classe representando as Tags
    public class Tags
    {
        // Propriedade Id, chave primária
        public int Id { get; set; }

        // Propriedade Name para armazenar o nome da tag
        public string Name { get; set; }

        // Propriedade de navegação para relacionamento muitos-para-muitos com EventTag
        public ICollection<EventTag> EventTags { get; set; }
    }

    // Classe representando a relação muitos-para-muitos entre Events e Tags
    public class EventTag
    {
        // Propriedade EventId, chave estrangeira referenciando a tabela Events
        public int EventId { get; set; }

        // Propriedade de navegação para o evento associado
        public Events Event { get; set; }

        // Propriedade TagId, chave estrangeira referenciando a tabela Tags
        public int TagId { get; set; }

        // Propriedade de navegação para a tag associada
        public Tags Tag { get; set; }
    }
}
