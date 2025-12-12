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

    /// Interaction logic for MainWindow.xaml

    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            AfficheDemarrage();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // rien pour l'instant
        }

        /// <summary>
        /// Affiche l'écran de démarrage (UCDemarrage)
        /// </summary>
        private void AfficheDemarrage()
        {
            UCDemarrage uc = new UCDemarrage();
            ZoneJeu.Content = uc;

            // UCDemarrage -> UCReglesJeu
            uc.butRegles.Click += AfficherReglesJeu;

            // UCDemarrage -> UCChoixPerso
            uc.butLancer.Click += AfficherChoixPerso;
        }

        /// <summary>
        /// UCDemarrage ou UCReglesJeu -> UCChoixPerso
        /// </summary>
        private void AfficherChoixPerso(object sender, RoutedEventArgs e)
        {
            UCChoixPerso uc = new UCChoixPerso();
            ZoneJeu.Content = uc;

            // UCChoixPerso -> UCChoisMaps
            uc.butSuivant.Click += AfficherChoisMaps;
            uc.butRetour.Click += AfficherReglesJeu;
        }

        /// <summary>
        /// UCDemarrage -> UCReglesJeu
        /// </summary>
        private void AfficherReglesJeu(object sender, RoutedEventArgs e)
        {
            UCReglesJeu uc = new UCReglesJeu();
            ZoneJeu.Content = uc;

            // UCReglesJeu -> UCChoixPerso
            uc.butSuivant.Click += AfficherChoixPerso;
            uc.butRetour.Click += AfficherDemarrageRegle;

        }

        private void AfficherDemarrageRegle(object sender, RoutedEventArgs e)
        {
            AfficheDemarrage();
        }


        /// <summary>
        /// UCChoisMaps -> UCJeu ++ retour UCChoisMaps -> UCChoixPerso
        /// </summary>
        private void AfficherChoisMaps(object sender, RoutedEventArgs e)
        {
            UCChoisMaps uc = new UCChoisMaps();
            ZoneJeu.Content = uc;

            // UCChoisMaps -> UCChoixPerso
            uc.butJouer.Click += AfficherJeu;
            // UCChoisMaps -> UCChoixPerso
            uc.butRetour.Click += AfficherChoixPerso; 
        }

        private void AfficherJeu(object sender, RoutedEventArgs e)
        {
            UCJeu uc = new UCJeu();
            ZoneJeu.Content = uc;
        }



    }

}
