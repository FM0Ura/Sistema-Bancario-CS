using System;
using System.Windows;
using MySql.Data.MySqlClient;

namespace Banco
{
    public partial class ResgateSenhaWindow : Window
    {
        public ResgateSenhaWindow()
        {
            InitializeComponent();
        }

        private void AbrirPaginaLogin(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close(); // Fecha a janela de resgate de senha
        }

        private void EnviarCodigo(object sender, RoutedEventArgs e)
        {
            // String de conexão com o banco
            string connectionString = "Server=localhost;Database=sistema_bancario;Uid=root;Pwd=root;";
            string email = txtEmail.Text.Trim();

            // Verificar se o campo de email está preenchido
            if (string.IsNullOrWhiteSpace(email))
            {
                lblMensagem.Text = "Por favor, insira o email.";
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Verificar se o email existe no banco
                    string query = "SELECT COUNT(*) FROM clientes WHERE email = @Email;";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        int emailExiste = Convert.ToInt32(command.ExecuteScalar());

                        if (emailExiste > 0)
                        {
                            // Simulação de envio de código (substitua com integração real de envio de email)
                            string codigoRecuperacao = new Random().Next(100000, 999999).ToString();

                            // Exemplo de exibição do código no log (não faça isso em produção!)
                            Console.WriteLine($"Código de recuperação enviado: {codigoRecuperacao}");

                            lblMensagem.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
                            lblMensagem.Text = "Código de recuperação enviado! Verifique seu email.";

                            // Você pode armazenar o código no banco, caso necessário, para validação posterior.
                            string updateQuery = "UPDATE clientes SET codigo_recuperacao = @Codigo WHERE email = @Email;";
                            using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@Codigo", codigoRecuperacao);
                                updateCommand.Parameters.AddWithValue("@Email", email);
                                updateCommand.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            lblMensagem.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                            lblMensagem.Text = "Email não encontrado.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao conectar ao banco de dados: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
