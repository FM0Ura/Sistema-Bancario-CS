using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace Banco
{
    public partial class TransferenciasPage : Page
    {
        public TransferenciasPage()
        {
            InitializeComponent();
        }

        private void OnTransferenciaTipoChanged(object sender, RoutedEventArgs e)
        {
            // Alternar entre os campos de PIX e TED
            if (rbPix.IsChecked == true)
            {
                pixPanel.Visibility = Visibility.Visible;
                tedPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                pixPanel.Visibility = Visibility.Collapsed;
                tedPanel.Visibility = Visibility.Visible;
            }
        }

        private void OnTransferirClicked(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=localhost;Database=sistema_bancario;Uid=root;Pwd=root;";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    if (rbPix.IsChecked == true)
                    {
                        // Transferência por PIX
                        string chavePix = txtPixChave.Text.Trim();
                        decimal valorPix;

                        if (string.IsNullOrWhiteSpace(chavePix) || !decimal.TryParse(txtPixValor.Text.Trim(), out valorPix) || valorPix <= 0)
                        {
                            MessageBox.Show("Por favor, insira uma chave PIX válida e um valor maior que zero.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        string queryPix = "SELECT ct.conta_id FROM clientes c " +
                                          "JOIN contas ct ON c.cliente_id = ct.cliente_id " +
                                          "WHERE c.email = @ChavePix OR c.telefone = @ChavePix OR c.cpf = @ChavePix LIMIT 1;";

                        using (MySqlCommand commandPix = new MySqlCommand(queryPix, connection))
                        {
                            commandPix.Parameters.AddWithValue("@ChavePix", chavePix);

                            object result = commandPix.ExecuteScalar();
                            if (result == null)
                            {
                                MessageBox.Show("Chave PIX não encontrada.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            long contaDestinoId = Convert.ToInt64(result);

                            RealizarTransferencia(connection, contaDestinoId, valorPix);
                        }
                    }
                    else if (rbTed.IsChecked == true)
                    {
                        // Transferência por TED
                        string agencia = txtTedAgencia.Text.Trim();
                        string numeroConta = txtTedContaNumero.Text.Trim();
                        string digitoConta = txtTedContaDigito.Text.Trim();
                        decimal valorTed;

                        if (string.IsNullOrWhiteSpace(agencia) || string.IsNullOrWhiteSpace(numeroConta) ||
                            string.IsNullOrWhiteSpace(digitoConta) || !decimal.TryParse(txtTedValor.Text.Trim(), out valorTed) || valorTed <= 0)
                        {
                            MessageBox.Show("Por favor, insira todos os dados corretamente e um valor maior que zero.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        string queryTed = "SELECT conta_id FROM contas WHERE agencia = @Agencia AND numero_conta = @NumeroConta AND digito = @Digito LIMIT 1;";

                        using (MySqlCommand commandTed = new MySqlCommand(queryTed, connection))
                        {
                            commandTed.Parameters.AddWithValue("@Agencia", agencia);
                            commandTed.Parameters.AddWithValue("@NumeroConta", numeroConta);
                            commandTed.Parameters.AddWithValue("@Digito", digitoConta);

                            object result = commandTed.ExecuteScalar();
                            if (result == null)
                            {
                                MessageBox.Show("Conta não encontrada.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            long contaDestinoId = Convert.ToInt64(result);

                            RealizarTransferencia(connection, contaDestinoId, valorTed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao realizar a transferência: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RealizarTransferencia(MySqlConnection connection, long contaDestinoId, decimal valor)
        {
            try
            {
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    // Verificar saldo disponível
                    long contaOrigemId = SessaoCliente.Instance.ContaId; // ID da conta do cliente logado
                    string querySaldo = "SELECT saldo FROM contas WHERE conta_id = @ContaOrigemId FOR UPDATE;";
                    decimal saldoOrigem;

                    using (MySqlCommand commandSaldo = new MySqlCommand(querySaldo, connection, transaction))
                    {
                        commandSaldo.Parameters.AddWithValue("@ContaOrigemId", contaOrigemId);
                        saldoOrigem = Convert.ToDecimal(commandSaldo.ExecuteScalar());
                    }

                    if (saldoOrigem < valor)
                    {
                        MessageBox.Show("Saldo insuficiente para realizar a transferência.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        transaction.Rollback();
                        return;
                    }

                    // Debitar o valor da conta de origem
                    string queryDebitar = "UPDATE contas SET saldo = saldo - @Valor WHERE conta_id = @ContaOrigemId;";
                    using (MySqlCommand commandDebitar = new MySqlCommand(queryDebitar, connection, transaction))
                    {
                        commandDebitar.Parameters.AddWithValue("@Valor", valor);
                        commandDebitar.Parameters.AddWithValue("@ContaOrigemId", contaOrigemId);
                        commandDebitar.ExecuteNonQuery();
                    }

                    // Creditar o valor na conta de destino
                    string queryCreditar = "UPDATE contas SET saldo = saldo + @Valor WHERE conta_id = @ContaDestinoId;";
                    using (MySqlCommand commandCreditar = new MySqlCommand(queryCreditar, connection, transaction))
                    {
                        commandCreditar.Parameters.AddWithValue("@Valor", valor);
                        commandCreditar.Parameters.AddWithValue("@ContaDestinoId", contaDestinoId);
                        commandCreditar.ExecuteNonQuery();
                    }

                    transaction.Commit();

                    MessageBox.Show("Transferência realizada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
