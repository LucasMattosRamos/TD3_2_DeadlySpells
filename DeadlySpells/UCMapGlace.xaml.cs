using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DeadlySpells
{
    public partial class UCMapGlace : UserControl
    {
        // --- Constantes du terrain / physique ---
        private const double HauteurSol = 494;
        private const double HauteurEpines = 750;
        private const double LimiteGauchePlateforme = 150;
        private const double LimiteDroitePlateforme = 150 + 1620 - 200;

        // Paramètres de mouvement
        private const double VitesseDeplacement = 15;
        private const double Gravite = 1.2;
        private const double VitesseSaut = -22;

        // État des joueurs
        private double vitesseVerticaleJ1 = 0;
        private double vitesseVerticaleJ2 = 0;
        private bool estAuSolJ1 = true;
        private bool estAuSolJ2 = true;

        private int viesJ1 = 3;
        private int viesJ2 = 3;

        private bool jeuEstFini = false;

        // CORRECTION : Le "?" signifie que timer peut être null
        private DispatcherTimer? timer;

        public UCMapGlace()
        {
            InitializeComponent();

            // On attend que l'écran soit totalement chargé pour lancer la partie et mettre le focus
            this.Loaded += (s, e) =>
            {
                InitialiserPartie();
                this.Focus(); // <-- C'est ça qui réactive le clavier !
            };
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Redonne le focus clavier au UserControl, assurant que les touches Q/D/Z fonctionnent
            this.Focus();
        }

        private void InitialiserPartie()
        {
            // Reset des variables
            viesJ1 = 3;
            viesJ2 = 3;
            vitesseVerticaleJ1 = 0;
            vitesseVerticaleJ2 = 0;
            estAuSolJ1 = true;
            estAuSolJ2 = true;
            jeuEstFini = false;

            // Reset UI
            if (GridFinDePartie != null) GridFinDePartie.Visibility = Visibility.Collapsed;
            MettreAJourViesUI();

            // Positions de départ
            Canvas.SetLeft(imgJoueur1, 299);
            Canvas.SetTop(imgJoueur1, HauteurSol);
            scaleJoueur1.ScaleX = 1;

            Canvas.SetLeft(imgJoueur2, 1416);
            Canvas.SetTop(imgJoueur2, HauteurSol);
            scaleJoueur2.ScaleX = -1;

            // --- Chargement des Images ---
            ChargerSkinJoueur(imgJoueur1, MainWindow.Joueur1Choix);
            ChargerSkinJoueur(imgJoueur2, MainWindow.Joueur2Choix);

            // Timer
            if (timer != null) timer.Stop();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += Timer_Tick;
            timer.Start();

            // Focus
            this.Focus();
        }

        private void ChargerSkinJoueur(Image img, string choix)
        {
            string dossier = "wizard"; // Default Orage
            if (choix == "Feu") dossier = "wizard_fire";
            else if (choix == "Glace") dossier = "wizard_ice";

            img.Source = new BitmapImage(new Uri($"/ImagesSorcier/PNG/{dossier}/1_IDLE_000.png", UriKind.Relative));
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (jeuEstFini) return;

            double positionJ1 = Canvas.GetLeft(imgJoueur1);
            double positionJ2 = Canvas.GetLeft(imgJoueur2);

            if (double.IsNaN(positionJ1)) positionJ1 = 299;
            if (double.IsNaN(positionJ2)) positionJ2 = 1416;

            switch (e.Key)
            {
                // --- Joueur 1 ---
                case Key.D:
                    Canvas.SetLeft(imgJoueur1, positionJ1 + VitesseDeplacement);
                    scaleJoueur1.ScaleX = 1;
                    break;
                case Key.Q:
                    Canvas.SetLeft(imgJoueur1, positionJ1 - VitesseDeplacement);
                    scaleJoueur1.ScaleX = -1;
                    break;
                case Key.Z:
                    if (estAuSolJ1) { vitesseVerticaleJ1 = VitesseSaut; estAuSolJ1 = false; }
                    break;
                case Key.Space:
                    LancerSort(imgJoueur1, MainWindow.Joueur1Choix);
                    break;

                // --- Joueur 2 ---
                case Key.Right:
                    Canvas.SetLeft(imgJoueur2, positionJ2 + VitesseDeplacement);
                    scaleJoueur2.ScaleX = 1;
                    break;
                case Key.Left:
                    Canvas.SetLeft(imgJoueur2, positionJ2 - VitesseDeplacement);
                    scaleJoueur2.ScaleX = -1;
                    break;
                case Key.Up:
                    if (estAuSolJ2) { vitesseVerticaleJ2 = VitesseSaut; estAuSolJ2 = false; }
                    break;
                case Key.RightShift:
                    LancerSort(imgJoueur2, MainWindow.Joueur2Choix);
                    break;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (jeuEstFini) return;

            bool j1EstMort = AppliquerPhysique(imgJoueur1, ref vitesseVerticaleJ1, ref estAuSolJ1, ref viesJ1, 299);
            bool j2EstMort = AppliquerPhysique(imgJoueur2, ref vitesseVerticaleJ2, ref estAuSolJ2, ref viesJ2, 1416);

            if (j1EstMort || j2EstMort)
            {
                MettreAJourViesUI();
                VerifierFinDePartie();
            }
        }

        private bool AppliquerPhysique(Image imageJoueur, ref double vitesseVerticale, ref bool estAuSol, ref int viesJoueur, double positionRespawnX)
        {
            bool aPerduUneVie = false;
            double positionY = Canvas.GetTop(imageJoueur);
            double positionX = Canvas.GetLeft(imageJoueur);

            bool estHorsPlateforme = positionX < LimiteGauchePlateforme || positionX > LimiteDroitePlateforme;

            if (!estAuSol || estHorsPlateforme)
            {
                vitesseVerticale += Gravite;
                positionY += vitesseVerticale;
                Canvas.SetTop(imageJoueur, positionY);
                estAuSol = false;
            }

            if (!estHorsPlateforme && positionY >= HauteurSol && vitesseVerticale >= 0)
            {
                positionY = HauteurSol;
                Canvas.SetTop(imageJoueur, positionY);
                vitesseVerticale = 0;
                estAuSol = true;
            }

            if (positionY >= HauteurEpines)
            {
                viesJoueur--;
                aPerduUneVie = true;

                if (viesJoueur > 0)
                {
                    Canvas.SetLeft(imageJoueur, positionRespawnX);
                    Canvas.SetTop(imageJoueur, HauteurSol);
                    vitesseVerticale = 0;
                    estAuSol = true;
                }
                else
                {
                    Canvas.SetTop(imageJoueur, HauteurEpines + 100);
                }
            }

            return aPerduUneVie;
        }

        private void MettreAJourViesUI()
        {
            string coeursJ1 = "";
            for (int i = 0; i < viesJ1; i++) coeursJ1 += "❤️";
            txtViesJ1.Text = coeursJ1;

            string coeursJ2 = "";
            for (int i = 0; i < viesJ2; i++) coeursJ2 += "❤️";
            txtViesJ2.Text = coeursJ2;
        }

        private void VerifierFinDePartie()
        {
            if (viesJ1 <= 0 || viesJ2 <= 0)
            {
                jeuEstFini = true;
                if (timer != null) timer.Stop();

                string gagnant = "";
                if (viesJ1 <= 0 && viesJ2 > 0) gagnant = "JOUEUR 2";
                else if (viesJ2 <= 0 && viesJ1 > 0) gagnant = "JOUEUR 1";
                else gagnant = "PERSONNE (Egalité)";

                txtGagnant.Text = gagnant + " REMPORTE LA PARTIE !";
                GridFinDePartie.Visibility = Visibility.Visible;
            }
        }

        private void LancerSort(Image joueur, string choixSorcier)
        {
            string dossier = "wizard";
            if (choixSorcier == "Feu") dossier = "wizard_fire";
            else if (choixSorcier == "Glace") dossier = "wizard_ice";

            int frame = 0;
            DispatcherTimer sort = new DispatcherTimer();
            sort.Interval = TimeSpan.FromMilliseconds(100);
            sort.Tick += (s, e) =>
            {
                string path = $"/ImagesSorcier/PNG/{dossier}/5_ATTACK_{frame:D3}.png";
                joueur.Source = new BitmapImage(new Uri(path, UriKind.Relative));
                frame++;
                if (frame > 6)
                {
                    sort.Stop();
                    joueur.Source = new BitmapImage(new Uri($"/ImagesSorcier/PNG/{dossier}/1_IDLE_000.png", UriKind.Relative));
                }
            };
            sort.Start();
        }
        private void LancerHurt(Image joueur, string choixSorcier)
        {
            string dossier;

            if (choixSorcier == "Feu")
                dossier = "wizard_fire";
            else if (choixSorcier == "Glace")
                dossier = "wizard_ice";
            else
                dossier = "wizard"; // par défaut (Orage)

            int frame = 0;
            DispatcherTimer hurt = new DispatcherTimer();
            hurt.Interval = TimeSpan.FromMilliseconds(100);
            hurt.Tick += (s, e) =>
            {
                string path = $"/ImagesSorcier/PNG/{dossier}/6_HURT_{frame:D3}.png";
                joueur.Source = new BitmapImage(new Uri(path, UriKind.Relative));
                frame++;
                if (frame > 4)
                {
                    hurt.Stop();
                    joueur.Source = new BitmapImage(new Uri($"/ImagesSorcier/PNG/{dossier}/1_IDLE_000.png", UriKind.Relative));
                }
            };
            hurt.Start();
        }

        private void LancerDie(Image joueur, string choixSorcier)
        {
            string dossier;

            if (choixSorcier == "Feu")
                dossier = "wizard_fire";
            else if (choixSorcier == "Glace")
                dossier = "wizard_ice";
            else
                dossier = "wizard"; // par défaut (Orage)

            int frame = 0;
            DispatcherTimer die = new DispatcherTimer();
            die.Interval = TimeSpan.FromMilliseconds(100);
            die.Tick += (s, e) =>
            {
                string path = $"/ImagesSorcier/PNG/{dossier}/7_DIE_{frame:D3}.png";
                joueur.Source = new BitmapImage(new Uri(path, UriKind.Relative));
                frame++;
                if (frame > 4)
                {
                    die.Stop();
                    joueur.Source = new BitmapImage(new Uri($"/ImagesSorcier/PNG/{dossier}/1_IDLE_000.png", UriKind.Relative));
                }
            };
            die.Start();
        }
        private void BtnRejouer_Click(object sender, RoutedEventArgs e)
        {
            InitialiserPartie();
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {




            Application.Current.Shutdown();
        }
    }
}

