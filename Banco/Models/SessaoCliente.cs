public class SessaoCliente
{
    private static SessaoCliente _instance;

    public static SessaoCliente Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SessaoCliente();
            return _instance;
        }
    }

    // Propriedades da sessão
    public int ClienteId { get; set; } // Propriedade para armazenar o ID do cliente logado
    public int ContaId { get; set; }   // Propriedade para armazenar o ID da conta vinculada ao cliente logado

    // Construtor privado para impedir instâncias externas
    private SessaoCliente() { }
}
