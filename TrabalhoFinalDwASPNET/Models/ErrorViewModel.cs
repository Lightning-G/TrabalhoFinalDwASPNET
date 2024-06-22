namespace TrabalhoFinalDwASPNET.Models
{
    // Classe que representa o modelo de visualiza��o de erro
    public class ErrorViewModel
    {
        // Propriedade para armazenar o ID da requisi��o
        // Pode ser nula, por isso o tipo string?
        public string? RequestId { get; set; }

        // Propriedade que indica se o RequestId deve ser exibido
        // Retorna verdadeiro se RequestId n�o for nulo ou vazio
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
