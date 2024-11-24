using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace Banco
{
    public partial class CartoesPage : Page
    {
        public CartoesPage()
        {
            InitializeComponent();
            CarregarCartoes();
        }

        private void CarregarCartoes()
        {
            string connectionString = "Server=localhost;Database=sistema_bancario;Uid=root;Pwd=root;";
            int clienteId = SessaoCliente.Instance.ClienteId; // Obtendo o ID do cliente autenticado

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string queryCartoes = @"
                SELECT c.tipo_cartao, c.numero_cartao, c.data_validade, c.cvv
                FROM cartoes c
                JOIN contas ct ON c.conta_id = ct.conta_id
                JOIN clientes cli ON ct.cliente_id = cli.cliente_id
                WHERE cli.cliente_id = @ClienteId;";

                    using (MySqlCommand command = new MySqlCommand(queryCartoes, connection))
                    {
                        command.Parameters.AddWithValue("@ClienteId", clienteId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            // Resetando os campos antes de carregar os dados
                            txtDebitoNumero.Text = "Número: ---";
                            txtDebitoValidade.Text = "---";
                            txtDebitoCVV.Text = "---";

                            txtCreditoNumero.Text = "Número: ---";
                            txtCreditoValidade.Text = "---";
                            txtCreditoCVV.Text = "---";

                            // Log: Verificar se os dados estão sendo retornados
                            Console.WriteLine("Lendo os dados dos cartões...");

                            while (reader.Read())
                            {
                                string tipoCartao = reader.GetString("tipo_cartao");
                                string numeroCartao = reader.GetString("numero_cartao");
                                string validade = reader.GetDateTime("data_validade").ToString("MM/yy");
                                string cvv = reader.GetString("cvv");

                                Console.WriteLine($"Cartão: {tipoCartao}, Número: {numeroCartao}, Validade: {validade}, CVV: {cvv}");

                                if (tipoCartao == "debito")
                                {
                                    txtDebitoNumero.Text = $"Número: {numeroCartao}";
                                    txtDebitoValidade.Text = validade;
                                    txtDebitoCVV.Text = cvv;
                                }
                                else if (tipoCartao == "credito")
                                {
                                    txtCreditoNumero.Text = $"Número: {numeroCartao}";
                                    txtCreditoValidade.Text = validade;
                                    txtCreditoCVV.Text = cvv;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar cartões: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SolicitarCartaoCredito(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=localhost;Database=sistema_bancario;Uid=root;Pwd=root;";
            int clienteId = SessaoCliente.Instance.ClienteId;

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Obter o ID da conta do cliente
                    string queryContaId = "SELECT conta_id FROM contas WHERE cliente_id = @ClienteId LIMIT 1;";
                    int contaId;

                    using (MySqlCommand commandContaId = new MySqlCommand(queryContaId, connection))
                    {
                        commandContaId.Parameters.AddWithValue("@ClienteId", clienteId);
                        object result = commandContaId.ExecuteScalar();

                        if (result == null)
                        {
                            MessageBox.Show("Conta não encontrada para este cliente.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        contaId = Convert.ToInt32(result);
                    }

                    // Inserir o cartão de crédito se ainda não existir
                    string queryCartaoCredito = @"
                        INSERT INTO cartoes (conta_id, numero_cartao, tipo_cartao, data_validade, cvv, limite)
                        SELECT @ContaId, @NumeroCartao, 'crédito', @DataValidade, @CVV, 2000.00
                        WHERE NOT EXISTS (
                            SELECT 1 FROM cartoes WHERE conta_id = @ContaId AND tipo_cartao = 'crédito'
                        );";

                    using (MySqlCommand commandCartao = new MySqlCommand(queryCartaoCredito, connection))
                    {
                        commandCartao.Parameters.AddWithValue("@ContaId", contaId);
                        commandCartao.Parameters.AddWithValue("@NumeroCartao", GerarNumeroCartao());
                        commandCartao.Parameters.AddWithValue("@DataValidade", DateTime.Now.AddYears(5).ToString("yyyy-MM-dd"));
                        commandCartao.Parameters.AddWithValue("@CVV", GerarCVV());

                        int rowsAffected = commandCartao.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Cartão de crédito emitido com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                            CarregarCartoes(); // Atualizar a interface
                        }
                        else
                        {
                            MessageBox.Show("A conta já possui um cartão de crédito.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao solicitar cartão de crédito: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GerarNumeroCartao()
        {
            Random random = new Random();
            return $"{random.Next(4000, 4999)} {random.Next(1000, 9999)} {random.Next(1000, 9999)} {random.Next(1000, 9999)}";
        }

        private string GerarCVV()
        {
            Random random = new Random();
            return random.Next(100, 999).ToString();
        }
    }
}
