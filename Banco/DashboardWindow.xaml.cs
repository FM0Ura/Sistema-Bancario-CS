using System.Windows;

namespace Banco
{
    public partial class DashboardWindow : Window
    {
        public DashboardWindow()
        {
            InitializeComponent();

            // Carregar a página inicial (Home) ao abrir o Dashboard
            //MainContent.Navigate(new HomePage());
        }

        private void MenuHome_Click(object sender, RoutedEventArgs e)
        {
            //MainContent.Navigate(new HomePage());
        }

        private void MenuCartoes_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Navigate(new CartoesPage());
        }

        private void MenuContas_Click(object sender, RoutedEventArgs e)
        {
            //MainContent.Navigate(new ContasPage());
        }

        private void MenuTransacoes_Click(object sender, RoutedEventArgs e)
        {
            //MainContent.Navigate(new TransacoesPage());
        }

        private void MenuRelatorios_Click(object sender, RoutedEventArgs e)
        {
            //MainContent.Navigate(new RelatoriosPage());
        }
    }
}
