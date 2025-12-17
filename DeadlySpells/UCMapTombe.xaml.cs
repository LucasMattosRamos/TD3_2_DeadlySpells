using DeadlySpells;
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
    /// Logique d'interaction pour UCMapTombe.xaml
    /// </summary>
    public partial class UCMapTombe : UserControl
    {
        // --- Constantes du terrain / physique ---

        // Position Y du joueur lorsqu'il est posé sur le sol
        private const double HauteurSol = 209;

        // Hauteur à partir de laquelle on considère que le joueur touche les épines
        private const double HauteurEpines = 373;

        // Limites horizontales de la plateforme
        private const double LimiteGauchePlateforme = 58;
        private const double LimiteDroitePlateforme = 58 + 684 - 100; // largeur sol - largeur perso

        // Positions de respawn
        private const double RespawnXJ1 = 126;
        private const double RespawnXJ2 = 555;

        // Paramètres de mouvement
        private const double VitesseDeplacement = 15;   // vitesse horizontale
        private const double Gravite = 1.2;             // gravité appliquée à chaque tick
        private const double VitesseSaut = -22;         // vitesse verticale initiale pour le saut

        // Vitesse des projectiles
        private const double VitesseProjectile = 10;

        // --- État des joueurs ---
        private double vitesseVerticaleJ1 = 0;
        private double vitesseVerticaleJ2 = 0;
        private bool estAuSolJ1 = true;
        private bool estAuSolJ2 = true;

        private int viesJ1 = 3;
        private int viesJ2 = 3;

        private bool jeuEstFini = false;

        private DispatcherTimer? timer;

        // --- Projectiles ---
        private class Projectile
        {
            public Image Sprite = null!;
            public double Vx;
            public bool FromJ1;
        }

        private readonly List<Projectile> projectiles = new List<Projectile>();

        public UCMapTombe()
        {
            InitializeComponent();

            // Une fois chargé, on initialise la partie et on met le focus
            this.Loaded += (s, e) =>
            {
                InitialiserPartie();
                this.Focus();
            };
        }

        // Redonne le focus clavier si on clique sur la map
        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
        }

        // ------------------ INITIALISATION PARTIE ------------------

        private void InitialiserPartie()
        {
            // Reset logique
            viesJ1 = 3;
            viesJ2 = 3;
            vitesseVerticaleJ1 = 0;
            vitesseVerticaleJ2 = 0;
            estAuSolJ1 = true;
            estAuSolJ2 = true;
            jeuEstFini = false;

            // Cacher l'écran de fin
            if (GridFinDePartie != null)
                GridFinDePartie.Visibility = Visibility.Collapsed;

            // Vies
            MettreAJourViesUI();

            // Positions de départ
            Canvas.SetLeft(imgJoueur1, RespawnXJ1);
            Canvas.SetTop(imgJoueur1, HauteurSol);
            scaleJoueur1.ScaleX = 1;

            Canvas.SetLeft(imgJoueur2, RespawnXJ2);
            Canvas.SetTop(imgJoueur2, HauteurSol);
            scaleJoueur2.ScaleX = -1;

            // Skins
            ChargerSkinJoueur(imgJoueur1, MainWindow.Joueur1Choix);
            ChargerSkinJoueur(imgJoueur2, MainWindow.Joueur2Choix);

            // Nettoyage des anciens projectiles
            foreach (var p in projectiles)
            {
                MapTombe.Children.Remove(p.Sprite);
            }
            projectiles.Clear();

            // Timer
            if (timer != null)
                timer.Stop();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            timer.Tick += Timer_Tick;
            timer.Start();

            this.Focus();
        }

        /// <summary>
        /// Charge l'image de base (idle) du sorcier selon son type.
        /// </summary>
        private void ChargerSkinJoueur(Image img, string choix)
        {
            string dossier = "wizard"; // Orage par défaut
            if (choix == "Feu") dossier = "wizard_fire";
            else if (choix == "Glace") dossier = "wizard_ice";

            img.Source = new BitmapImage(
                new Uri($"/ImagesSorcier/PNG/{dossier}/1_IDLE_000.png", UriKind.Relative));
        }

        // ------------------ CLAVIER ------------------

        /// <summary>
        /// Gestion des touches (Q/D/Z + espace pour J1, flèches + RightShift pour J2).
        /// </summary>
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (jeuEstFini) return;

            // ----------------- JOUEUR 1 (Q / D / Z / Espace) -----------------

            double xJ1 = Canvas.GetLeft(imgJoueur1);
            if (double.IsNaN(xJ1)) xJ1 = RespawnXJ1;

            // Droite (D)
            if (Keyboard.IsKeyDown(Key.D))
            {
                double prochainX = xJ1 + VitesseDeplacement;
                // Supprime la condition 'if (prochainX <= LimiteDroitePlateforme)'
                Canvas.SetLeft(imgJoueur1, prochainX);
                scaleJoueur1.ScaleX = 1;
                xJ1 = prochainX;
            }

            // Gauche (Q)
            if (Keyboard.IsKeyDown(Key.Q))
            {
                double prochainX = xJ1 - VitesseDeplacement;
                // Supprime la condition 'if (prochainX >= LimiteGauchePlateforme)'
                Canvas.SetLeft(imgJoueur1, prochainX);
                scaleJoueur1.ScaleX = -1;
                xJ1 = prochainX;
            }

            // Saut (Z)
            if (Keyboard.IsKeyDown(Key.Z) && estAuSolJ1)
            {
                vitesseVerticaleJ1 = VitesseSaut;
                estAuSolJ1 = false;
            }

            // Tir (Espace) – déclenché au moment de la touche reçue
            if (e.Key == Key.Space)
            {
                LancerSort(imgJoueur1, MainWindow.Joueur1Choix, true);
            }

            // ----------------- JOUEUR 2 (flèches + Shift) -----------------

            double xJ2 = Canvas.GetLeft(imgJoueur2);
            if (double.IsNaN(xJ2)) xJ2 = RespawnXJ2;

            // Droite (→)
            if (Keyboard.IsKeyDown(Key.Right))
            {
                // On retire la vérification de LimiteDroitePlateforme
                double prochainX = xJ2 + VitesseDeplacement;
                Canvas.SetLeft(imgJoueur2, prochainX);
                scaleJoueur2.ScaleX = 1; // regarde à droite
                xJ2 = prochainX;
            }

            // Gauche (←)
            if (Keyboard.IsKeyDown(Key.Left))
            {
                // On retire la vérification de LimiteGauchePlateforme
                double prochainX = xJ2 - VitesseDeplacement;
                Canvas.SetLeft(imgJoueur2, prochainX);
                scaleJoueur2.ScaleX = -1; // regarde à gauche
                xJ2 = prochainX;
            }

            // Saut (↑)
            if (Keyboard.IsKeyDown(Key.Up) && estAuSolJ2)
            {
                vitesseVerticaleJ2 = VitesseSaut;
                estAuSolJ2 = false;
            }

            // Tir (RightShift)
            if (e.Key == Key.RightShift)
            {
                LancerSort(imgJoueur2, MainWindow.Joueur2Choix, false);
            }
        }

        // ------------------ TIMER ------------------

        /// <summary>
        /// Tick du timer : applique la physique aux deux joueurs et met à jour les projectiles.
        /// </summary>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (jeuEstFini) return;

            bool j1APerdu = AppliquerPhysique(imgJoueur1, ref vitesseVerticaleJ1, ref estAuSolJ1, ref viesJ1, RespawnXJ1);
            bool j2APerdu = AppliquerPhysique(imgJoueur2, ref vitesseVerticaleJ2, ref estAuSolJ2, ref viesJ2, RespawnXJ2);

            if (j1APerdu || j2APerdu)
            {
                MettreAJourViesUI();
                VerifierFinDePartie();
            }

            MettreAJourProjectiles();
        }

        // ------------------ PHYSIQUE JOUEURS ------------------

        /// <summary>
        /// Applique gravité + sol + épines + respawn pour un joueur.
        /// </summary>
        private bool AppliquerPhysique(Image imageJoueur,
                                       ref double vitesseVerticale,
                                       ref bool estAuSol,
                                       ref int viesJoueur,
                                       double positionRespawnX)
        {
            bool aPerduUneVie = false;

            double positionY = Canvas.GetTop(imageJoueur);
            double positionX = Canvas.GetLeft(imageJoueur);

            bool estHorsPlateforme = positionX < LimiteGauchePlateforme || positionX > LimiteDroitePlateforme;

            // Gravité
            if (!estAuSol || estHorsPlateforme)
            {
                vitesseVerticale += Gravite;
                positionY += vitesseVerticale;
                Canvas.SetTop(imageJoueur, positionY);
                estAuSol = false;
            }

            // Retour sur le sol
            if (!estHorsPlateforme && positionY >= HauteurSol && vitesseVerticale >= 0)
            {
                positionY = HauteurSol;
                Canvas.SetTop(imageJoueur, positionY);
                vitesseVerticale = 0;
                estAuSol = true;
            }

            // Chute dans les épines
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
                    // Hors-jeu
                    Canvas.SetTop(imageJoueur, HauteurEpines + 50);
                }
            }

            return aPerduUneVie;
        }

        // ------------------ VIES / FIN DE PARTIE ------------------

        /// <summary>
        /// Met à jour l'affichage des vies (cœurs rouges).
        /// </summary>
        private void MettreAJourViesUI()
        {
            string coeursJ1 = "";
            for (int i = 0; i < viesJ1; i++) coeursJ1 += "❤️";
            txtViesJ1.Text = coeursJ1;

            string coeursJ2 = "";
            for (int i = 0; i < viesJ2; i++) coeursJ2 += "❤️";
            txtViesJ2.Text = coeursJ2;
        }

        /// <summary>
        /// Vérifie si la partie est terminée et affiche l'écran de victoire.
        /// </summary>
        private void VerifierFinDePartie()
        {
            if (viesJ1 <= 0 || viesJ2 <= 0)
            {
                jeuEstFini = true;
                if (timer != null)
                    timer.Stop();

                string gagnant;
                if (viesJ1 <= 0 && viesJ2 > 0) gagnant = "JOUEUR 2";
                else if (viesJ2 <= 0 && viesJ1 > 0) gagnant = "JOUEUR 1";
                else gagnant = "PERSONNE (Égalité)";

                txtGagnant.Text = gagnant + " REMPORTE LA PARTIE !";
                GridFinDePartie.Visibility = Visibility.Visible;
            }
        }

        // ------------------ PROJECTILES ------------------

        /// <summary>
        /// Retourne le chemin de l'image du projectile selon le type de sorcier.
        /// </summary>
        private string ObtenirImageProjectile(string choixSorcier)
        {
            if (choixSorcier == "Feu")
                return "/Images/fire.png";
            else if (choixSorcier == "Glace")
                return "/Images/ice.png";
            else
                return "/Images/lightning.png";
        }

        // Crée le projectile + lance l'animation d'attaque du sorcier
        private void LancerSort(Image joueur, string choixSorcier, bool fromJ1)
        {
            // 1) Création du projectile (ancien code) -----------------------
            Image imgProj = new Image
            {
                Width = 60,
                Height = 60,
                Source = new BitmapImage(new Uri(ObtenirImageProjectile(choixSorcier), UriKind.Relative))
            };

            // Position de départ = centre du joueur
            double xJ = Canvas.GetLeft(joueur);
            double yJ = Canvas.GetTop(joueur);

            if (double.IsNaN(xJ)) xJ = fromJ1 ? RespawnXJ1 : RespawnXJ2;
            if (double.IsNaN(yJ)) yJ = HauteurSol;

            double xProj = xJ + joueur.Width / 2 - imgProj.Width / 2;
            double yProj = yJ + joueur.Height / 2 - imgProj.Height / 2;

            Canvas.SetLeft(imgProj, xProj);
            Canvas.SetTop(imgProj, yProj);

            MapTombe.Children.Add(imgProj);

            // Direction selon l'orientation du joueur
            double direction = 1;
            if (fromJ1)
            {
                if (scaleJoueur1.ScaleX < 0) direction = -1;
            }
            else
            {
                if (scaleJoueur2.ScaleX < 0) direction = -1;
            }

            Projectile p = new Projectile
            {
                Sprite = imgProj,
                Vx = direction * VitesseProjectile,
                FromJ1 = fromJ1
            };

            projectiles.Add(p);

            // 2) Animation du sorcier qui lance le sort --------------------

            // dossier des sprites selon le sorcier
            string dossierPerso = "wizard";
            if (choixSorcier == "Feu") dossierPerso = "wizard_fire";
            else if (choixSorcier == "Glace") dossierPerso = "wizard_ice";

            int frame = 0;
            DispatcherTimer anim = new DispatcherTimer();
            anim.Interval = TimeSpan.FromMilliseconds(80); // vitesse de l'anim

            anim.Tick += (s, e) =>
            {
                // tant qu'on a des frames d'attaque
                if (frame <= 6)
                {
                    string path = $"/ImagesSorcier/PNG/{dossierPerso}/5_ATTACK_{frame:D3}.png";
                    joueur.Source = new BitmapImage(new Uri(path, UriKind.Relative));
                    frame++;
                }
                else
                {
                    // on revient à l'idle à la fin de l'animation
                    string idlePath = $"/ImagesSorcier/PNG/{dossierPerso}/1_IDLE_000.png";
                    joueur.Source = new BitmapImage(new Uri(idlePath, UriKind.Relative));
                    anim.Stop();
                }
            };

            anim.Start();
        }

        /// <summary>
        /// Met à jour la position des projectiles + gère collisions + suppression.
        /// </summary>
        private void MettreAJourProjectiles()
        {
            if (projectiles.Count == 0) return;

            List<Projectile> aSupprimer = new List<Projectile>();

            foreach (var p in projectiles)
            {
                double x = Canvas.GetLeft(p.Sprite);
                double y = Canvas.GetTop(p.Sprite);

                x += p.Vx;
                Canvas.SetLeft(p.Sprite, x);

                // Supprimer s'il sort de l'écran
                if (x < -100 || x > 800 + 100)
                {
                    aSupprimer.Add(p);
                    continue;
                }

                // Collision avec un joueur
                Rect rectProj = new Rect(x, y, p.Sprite.Width, p.Sprite.Height);

                if (p.FromJ1 && viesJ2 > 0)
                {
                    double x2 = Canvas.GetLeft(imgJoueur2);
                    double y2 = Canvas.GetTop(imgJoueur2);
                    Rect rectJ2 = new Rect(x2, y2, imgJoueur2.Width, imgJoueur2.Height);

                    if (rectProj.IntersectsWith(rectJ2))
                    {
                        viesJ2--;
                        MettreAJourViesUI();
                        VerifierFinDePartie();

                        if (!jeuEstFini && viesJ2 > 0)
                        {
                            Canvas.SetLeft(imgJoueur2, RespawnXJ2);
                            Canvas.SetTop(imgJoueur2, HauteurSol);
                            vitesseVerticaleJ2 = 0;
                            estAuSolJ2 = true;
                        }

                        aSupprimer.Add(p);
                        continue;
                    }
                }
                else if (!p.FromJ1 && viesJ1 > 0)
                {
                    double x1 = Canvas.GetLeft(imgJoueur1);
                    double y1 = Canvas.GetTop(imgJoueur1);
                    Rect rectJ1 = new Rect(x1, y1, imgJoueur1.Width, imgJoueur1.Height);

                    if (rectProj.IntersectsWith(rectJ1))
                    {
                        viesJ1--;
                        MettreAJourViesUI();
                        VerifierFinDePartie();

                        if (!jeuEstFini && viesJ1 > 0)
                        {
                            Canvas.SetLeft(imgJoueur1, RespawnXJ1);
                            Canvas.SetTop(imgJoueur1, HauteurSol);
                            vitesseVerticaleJ1 = 0;
                            estAuSolJ1 = true;
                        }

                        aSupprimer.Add(p);
                        continue;
                    }
                }
            }

            // Suppression des projectiles morts
            foreach (var p in aSupprimer)
            {
                MapTombe.Children.Remove(p.Sprite);
                projectiles.Remove(p);
            }
        }

        // ------------------ BOUTONS FIN DE PARTIE ------------------

        /// <summary>
        /// Bouton REJOUER.
        /// </summary>
        private void BtnRejouer_Click(object sender, RoutedEventArgs e)
        {
            InitialiserPartie();
        }

        /// <summary>
        /// Bouton QUITTER.
        /// </summary>
        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}