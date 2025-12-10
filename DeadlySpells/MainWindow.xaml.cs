using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeadlySpells
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        

        public MainWindow()
        {
            InitializeComponent();
            AfficheDemarrage();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void AfficheDemarrage()
        {
            // crée et charge l'écran de démarrage
            UCDemarrage uc = new UCDemarrage();
            // associe l'écran au conteneur
            ZoneJeu.Content = uc;
            
            uc.butRegles.Click += AfficherReglesJeu;
            uc.butDemarrer.Click += AfficherChoixPerso;
        }

        private void AfficherChoixPerso(object sender, RoutedEventArgs e)
        {
            UCChoixPerso uc = new UCChoixPerso();
            ZoneJeu.Content = uc;
        }

        private void AfficherReglesJeu(object sender, RoutedEventArgs e)
        {
            UCReglesJeu uc = new UCReglesJeu();
            ZoneJeu.Content = uc;

        }




        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}