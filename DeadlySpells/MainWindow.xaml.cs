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
        // C'est LA variable importante : elle stocke le texte "Feu", "Glace" ou "Tombe"
        public static string ChoixMap { get; set; } = "";
        public static string Joueur1Choix { get; set; } = "";
        public static string Joueur2Choix { get; set; } = "";

        public MainWindow()
        {
            InitializeComponent();
            AfficheDemarrage();
        }

        public enum MageType
        {
            Feu,
            Glace,
            Orage
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // rien pour l'instant
        }
        
        private void AfficheDemarrage()
        {
            UCDemarrage uc = new UCDemarrage();
            ZoneJeu.Content = uc;
            uc.butRegles.Click += AfficherReglesJeu;
            uc.butLancer.Click += AfficherChoixPerso;
        }

        private void AfficherChoixPerso(object sender, RoutedEventArgs e)
        {

            


            UCChoixPerso uc = new UCChoixPerso();
            ZoneJeu.Content = uc;
            uc.butSuivant.Click += AfficherChoisMaps;
            uc.butRetour.Click += AfficherDemarrageRegle;
        }

        private void AfficherReglesJeu(object sender, RoutedEventArgs e)
        {
            UCReglesJeu uc = new UCReglesJeu();
            ZoneJeu.Content = uc;
            uc.butSuivant.Click += AfficherChoixPerso;
            uc.butRetour.Click += AfficherDemarrageRegle;
        }

        private void AfficherDemarrageRegle(object sender, RoutedEventArgs e)
        {
            AfficheDemarrage();
        }

        private void AfficherChoisMaps(object sender, RoutedEventArgs e)
        {
           

            UCChoisMaps uc = new UCChoisMaps();
            ZoneJeu.Content = uc;

            // Important : On remet le choix à zéro quand on arrive sur la page
            ChoixMap = "";

            uc.butRetour.Click += AfficherChoixPerso;

            // Ligne CRUCIALE : Relie le clic bouton à la fonction de chargement
            uc.butJouer.Click += AfficheMap;
        }

        private void AfficherJeu(object sender, RoutedEventArgs e)
        {
            UCJeu uc = new UCJeu();
            ZoneJeu.Content = uc;
        }

        public void AfficheMap(object sender, RoutedEventArgs e)
        {
            // Sécurité : Si le joueur n'a rien sélectionné
            if (string.IsNullOrEmpty(ChoixMap))
            {
                MessageBox.Show("Veuillez sélectionner une Map");
                return;
            }

            if (ChoixMap == "Feu")
            {
                UCMapFeu uc = new UCMapFeu();
                ZoneJeu.Content = uc;
                uc.butMenu.Click += AfficheMenu;
            }
            else if (ChoixMap == "Glace")
            {
                UCMapGlace uc = new UCMapGlace();
                ZoneJeu.Content = uc;
                uc.butMenu.Click += AfficheMenu;
            }
            else if (ChoixMap == "Tombe")
            {
                UCMapTombe uc = new UCMapTombe();
                ZoneJeu.Content = uc;
                uc.butMenu.Click += AfficheMenu;
            }

        }

        private void AfficheMenu(object sender, RoutedEventArgs e)
        { 
            UCMenu uc = new UCMenu();
            ZoneJeu.Content = uc;

            uc.butRetourPartie.Click += AfficheMap;

            uc.butAccueil.Click += AfficherDemarrageRegle;
        }

    }
}
