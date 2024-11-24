using System;
using System.Windows;
using MySql.Data.MySqlClient;

namespace Banco
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            pwdPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSenha.Password))
            {
                pwdPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void AbrirPaginaRegistro(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RegistroClienteWindow registroWindow = new RegistroClienteWindow();
            registroWindow.Show();
            this.Close(); // Fecha a janela de login atual
        }

        private void AutenticarUsuario(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=localhost;Database=sistema_bancario;Uid=root;Pwd=root;";
            string email = txtEmail.Text.Trim();
            string senha = txtSenha.Password;

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Verificar login
                    string queryLogin = "SELECT cliente_id FROM clientes WHERE email = @Email AND senha = @Senha LIMIT 1;";
                    using (MySqlCommand command = new MySqlCommand(queryLogin, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Senha", senha);

                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            int clienteId = Convert.ToInt32(result);
                            SessaoCliente.Instance.ClienteId = clienteId;

                            // Obter o ID da conta vinculada
                            string queryConta = "SELECT conta_id FROM contas WHERE cliente_id = @ClienteId LIMIT 1;";
                            using (MySqlCommand commandConta = new MySqlCommand(queryConta, connection))
                            {
                                commandConta.Parameters.AddWithValue("@ClienteId", clienteId);
                                SessaoCliente.Instance.ContaId = Convert.ToInt32(commandConta.ExecuteScalar());
                            }

                            // Abrir dashboard
                            DashboardWindow dashboard = new DashboardWindow();
                            dashboard.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Email ou senha inválidos.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao autenticar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AbrirPaginaResgateSenha(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ResgateSenhaWindow resgateSenhaWindow = new ResgateSenhaWindow();
            resgateSenhaWindow.Show();
            this.Close(); // Fecha a janela de login
        }
    }
}
