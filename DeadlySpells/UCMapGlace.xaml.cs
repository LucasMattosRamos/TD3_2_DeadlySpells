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
    /// <summary>
    /// Logique d'interaction pour UCMapGlace.xaml
    /// </summary>
    public partial class UCMapGlace : UserControl
    {
        // --- Constantes du terrain / physique ---

        // Position Y du joueur lorsqu'il est posé sur le sol (Canvas.Top au repos)
        private const double HauteurSol = 494;

        // Hauteur à partir de laquelle on considère que le joueur touche les épines
        private const double HauteurEpines = 750;

        // Limites horizontales de la plateforme de glace
        private const double LimiteGauchePlateforme = 150;
        private const double LimiteDroitePlateforme = 150 + 1620 - 200; // largeur sol - largeur perso

        // Paramètres de mouvement
        private const double VitesseDeplacement = 15;   // vitesse horizontale
        private const double Gravite = 1.2;             // gravité appliquée à chaque tick
        private const double VitesseSaut = -22;         // vitesse verticale initiale pour le saut

        // État des joueurs (vertical)
        private double vitesseVerticaleJ1 = 0;
        private double vitesseVerticaleJ2 = 0;
        private bool estAuSolJ1 = true;
        private bool estAuSolJ2 = true;

        private int viesJ1 = 3;
        private int viesJ2 = 3;

        private DispatcherTimer timer;

        public UCMapGlace()
        {
            InitializeComponent();

            // --- Initialisation de l'image du Joueur 1 ---
            if (MainWindow.Joueur1Choix == "Feu")
                imgJoueur1.Source = new BitmapImage(new Uri("/ImagesSorcier/PNG/wizard_fire/1_IDLE_000.png", UriKind.Relative));
            else if (MainWindow.Joueur1Choix == "Glace")
                imgJoueur1.Source = new BitmapImage(new Uri("/ImagesSorcier/PNG/wizard_ice/1_IDLE_000.png", UriKind.Relative));
            else if (MainWindow.Joueur1Choix == "Orage")
                imgJoueur1.Source = new BitmapImage(new Uri("/ImagesSorcier/PNG/wizard/1_IDLE_000.png", UriKind.Relative));

            // --- Initialisation de l'image du Joueur 2 ---
            if (MainWindow.Joueur2Choix == "Feu")
                imgJoueur2.Source = new BitmapImage(new Uri("/ImagesSorcier/PNG/wizard_fire/1_IDLE_000.png", UriKind.Relative));
            else if (MainWindow.Joueur2Choix == "Glace")
                imgJoueur2.Source = new BitmapImage(new Uri("/ImagesSorcier/PNG/wizard_ice/1_IDLE_000.png", UriKind.Relative));
            else if (MainWindow.Joueur2Choix == "Orage")
                imgJoueur2.Source = new BitmapImage(new Uri("/ImagesSorcier/PNG/wizard/1_IDLE_000.png", UriKind.Relative));

            // Focus clavier
            this.Focusable = true;
            this.Loaded += (s, e) => this.Focus();

            // Timer pour la gravité / les chutes (environ 60 FPS)
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        // Gestion des touches clavier (déplacement + orientation)
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            double positionJ1 = Canvas.GetLeft(imgJoueur1);
            double positionJ2 = Canvas.GetLeft(imgJoueur2);

            // Sécurité : si jamais la position n'est pas définie (NaN)
            if (double.IsNaN(positionJ1))
                positionJ1 = 299;
            if (double.IsNaN(positionJ2))
                positionJ2 = 1416;

            switch (e.Key)
            {
                // --- Joueur 1 : Q / D / Z ---
                case Key.D:
                    Canvas.SetLeft(imgJoueur1, positionJ1 + VitesseDeplacement);
                    scaleJoueur1.ScaleX = 1;   // regarde à droite
                    break;

                case Key.Q:
                    Canvas.SetLeft(imgJoueur1, positionJ1 - VitesseDeplacement);
                    scaleJoueur1.ScaleX = -1;  // regarde à gauche
                    break;

                case Key.Z:
                    if (estAuSolJ1)
                    {
                        vitesseVerticaleJ1 = VitesseSaut;
                        estAuSolJ1 = false;
                    }
                    break;

                // --- Joueur 2 : flèches gauche / droite / haut ---
                case Key.Right:
                    Canvas.SetLeft(imgJoueur2, positionJ2 + VitesseDeplacement);
                    scaleJoueur2.ScaleX = 1;   // regarde à droite
                    break;

                case Key.Left:
                    Canvas.SetLeft(imgJoueur2, positionJ2 - VitesseDeplacement);
                    scaleJoueur2.ScaleX = -1;  // regarde à gauche
                    break;

                case Key.Up:
                    if (estAuSolJ2)
                    {
                        vitesseVerticaleJ2 = VitesseSaut;
                        estAuSolJ2 = false;
                    }
                    break;
            }
        }

        // Tick du timer : gravité / sol / épines
        private void Timer_Tick(object? sender, EventArgs e)
        {
            // 299 et 1416 = positions de respawn X des joueurs sur la map glace
            AppliquerPhysique(imgJoueur1, ref vitesseVerticaleJ1, ref estAuSolJ1, ref viesJ1, 299);
            AppliquerPhysique(imgJoueur2, ref vitesseVerticaleJ2, ref estAuSolJ2, ref viesJ2, 1416);
        }

        /// <summary>
        /// Applique la gravité, gère le sol, les épines et le respawn pour un joueur.
        /// </summary>
        private void AppliquerPhysique(Image imageJoueur,
                                       ref double vitesseVerticale,
                                       ref bool estAuSol,
                                       ref int viesJoueur,
                                       double positionRespawnX)
        {
            double positionY = Canvas.GetTop(imageJoueur);
            double positionX = Canvas.GetLeft(imageJoueur);

            bool estHorsPlateforme = positionX < LimiteGauchePlateforme || positionX > LimiteDroitePlateforme;

            // Si le joueur saute ou est au-dessus du vide, on applique la gravité
            if (!estAuSol || estHorsPlateforme)
            {
                vitesseVerticale += Gravite;     // accélération vers le bas
                positionY += vitesseVerticale;   // nouvelle position verticale
                Canvas.SetTop(imageJoueur, positionY);
                estAuSol = false;
            }

            // Si le joueur retombe sur la plateforme
            if (!estHorsPlateforme && positionY >= HauteurSol && vitesseVerticale >= 0)
            {
                positionY = HauteurSol;
                Canvas.SetTop(imageJoueur, positionY);
                vitesseVerticale = 0;
                estAuSol = true;
            }

            // Si le joueur tombe sur les épines (trop bas)
            if (positionY >= HauteurEpines)
            {
                viesJoueur--;

                if (viesJoueur <= 0)
                {
                    MessageBox.Show("Le joueur a perdu ses 3 vies !");
                    viesJoueur = 3; // pour continuer à tester
                }

                // Respawn sur la plateforme
                Canvas.SetLeft(imageJoueur, positionRespawnX);
                Canvas.SetTop(imageJoueur, HauteurSol);
                vitesseVerticale = 0;
                estAuSol = true;
            }
        }
    }
}

