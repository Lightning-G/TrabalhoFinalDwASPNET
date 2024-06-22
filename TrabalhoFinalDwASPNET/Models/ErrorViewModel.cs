namespace TrabalhoFinalDwASPNET.Models
{
    // Classe que representa o modelo de visualização de erro
    public class ErrorViewModel
    {
        // Propriedade para armazenar o ID da requisição
        // Pode ser nula, por isso o tipo string?
        public string? RequestId { get; set; }

        // Propriedade que indica se o RequestId deve ser exibido
        // Retorna verdadeiro se RequestId não for nulo ou vazio
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
