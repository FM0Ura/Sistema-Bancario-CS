using System;
using System.Windows;
using MySql.Data.MySqlClient;

namespace Banco
{
    public partial class RegistroClienteWindow : Window
    {
        public RegistroClienteWindow()
        {
            InitializeComponent();
        }

        private void AbrirPaginaLogin(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }

        private void RegistrarCliente(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=localhost;Database=sistema_bancario;Uid=root;Pwd=root;";

            string nome = txtNome.Text.Trim();
            string cpf = txtCPF.Text.Trim();
            string endereco = txtEndereco.Text.Trim();
            string telefone = txtTelefone.Text.Trim();
            string email = txtEmail.Text.Trim();
            string senha = txtSenha.Text.Trim();
            string tipoConta = cbTipoConta.Text;
            DateTime? dataNascimento = dpDataNascimento.SelectedDate;

            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(cpf) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha) ||
                dataNascimento == null || string.IsNullOrWhiteSpace(tipoConta))
            {
                MessageBox.Show("Por favor, preencha todos os campos obrigatórios.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string queryCliente = "INSERT INTO clientes (nome, cpf, endereco, telefone, email, senha, data_nascimento) " +
                                                  "VALUES (@Nome, @CPF, @Endereco, @Telefone, @Email, @Senha, @DataNascimento);";

                            long clienteId;
                            using (MySqlCommand commandCliente = new MySqlCommand(queryCliente, connection, transaction))
                            {
                                commandCliente.Parameters.AddWithValue("@Nome", nome);
                                commandCliente.Parameters.AddWithValue("@CPF", cpf);
                                commandCliente.Parameters.AddWithValue("@Endereco", endereco);
                                commandCliente.Parameters.AddWithValue("@Telefone", telefone);
                                commandCliente.Parameters.AddWithValue("@Email", email);
                                commandCliente.Parameters.AddWithValue("@Senha", senha);
                                commandCliente.Parameters.AddWithValue("@DataNascimento", dataNascimento.Value.ToString("yyyy-MM-dd"));
                                commandCliente.ExecuteNonQuery();
                                clienteId = commandCliente.LastInsertedId;
                            }

                            // Gerar valores únicos de agência, número da conta e dígito
                            bool contaInserida = false;
                            long contaId = 0;
                            while (!contaInserida)
                            {
                                try
                                {
                                    string agencia = new Random().Next(1000, 9999).ToString();
                                    string numeroConta = new Random().Next(10000000, 99999999).ToString();
                                    string digito = new Random().Next(0, 9).ToString();

                                    string queryConta = "INSERT INTO contas (cliente_id, tipo_conta, saldo, data_abertura, agencia, numero_conta, digito) " +
                                                        "VALUES (@ClienteId, @TipoConta, @Saldo, @DataAbertura, @Agencia, @NumeroConta, @Digito);";

                                    using (MySqlCommand commandConta = new MySqlCommand(queryConta, connection, transaction))
                                    {
                                        commandConta.Parameters.AddWithValue("@ClienteId", clienteId);
                                        commandConta.Parameters.AddWithValue("@TipoConta", tipoConta);
                                        commandConta.Parameters.AddWithValue("@Saldo", 0.00);
                                        commandConta.Parameters.AddWithValue("@DataAbertura", DateTime.Now.ToString("yyyy-MM-dd"));
                                        commandConta.Parameters.AddWithValue("@Agencia", agencia);
                                        commandConta.Parameters.AddWithValue("@NumeroConta", numeroConta);
                                        commandConta.Parameters.AddWithValue("@Digito", digito);
                                        commandConta.ExecuteNonQuery();
                                        contaId = commandConta.LastInsertedId;
                                        contaInserida = true; // Se não houver exceção, conta foi inserida com sucesso
                                    }
                                }
                                catch (MySqlException ex) when (ex.Number == 1062)
                                {
                                    // Código de erro 1062: Duplicate entry (violação de unicidade)
                                    // Gerar novos valores e tentar novamente
                                }
                            }

                            // Inserir cartão de débito
                            string queryCartaoDebito = "INSERT INTO cartoes (conta_id, numero_cartao, tipo_cartao, data_validade, cvv, limite) " +
                                                       "VALUES (@ContaId, @NumeroCartao, 'débito', @DataValidade, @CVV, NULL);";

                            using (MySqlCommand commandCartao = new MySqlCommand(queryCartaoDebito, connection, transaction))
                            {
                                commandCartao.Parameters.AddWithValue("@ContaId", contaId);
                                commandCartao.Parameters.AddWithValue("@NumeroCartao", GerarNumeroCartao());
                                commandCartao.Parameters.AddWithValue("@DataValidade", DateTime.Now.AddYears(5).ToString("yyyy-MM-dd"));
                                commandCartao.Parameters.AddWithValue("@CVV", GerarCVV());
                                commandCartao.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            MessageBox.Show("Cliente registrado com sucesso, conta criada e cartão de débito emitido!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                            MainWindow loginWindow = new MainWindow();
                            loginWindow.Show();
                            this.Close();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao registrar cliente: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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
