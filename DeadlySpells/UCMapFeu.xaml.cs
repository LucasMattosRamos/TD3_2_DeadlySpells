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

namespace DeadlySpells
{
    /// <summary>
    /// Logique d'interaction pour UCMapFeu.xaml
    /// </summary>
    public partial class UCMapFeu : UserControl
    {
        public UCMapFeu()
        {
            InitializeComponent();

            if (MainWindow.Joueur1Choix == "Feu")
                imgJoueur1.Source = new BitmapImage(new Uri("/ImagesSorcier/PNG/wizard_fire/1_IDLE_000.png", UriKind.Relative));
            else if (MainWindow.Joueur1Choix == "Glace")
                imgJoueur1.Source = new BitmapImage(new Uri("/ImagesSorcier/PNG/wizard_ice/1_IDLE_000.png", UriKind.Relative));
            else if (MainWindow.Joueur1Choix == "Orage")
                imgJoueur1.Source = new BitmapImage(new Uri("/ImagesSorcier/PNG/wizard/1_IDLE_000.png", UriKind.Relative));

            // Joueur 2
            if (MainWindow.Joueur2Choix == "Feu")
                imgJoueur2.Source = new BitmapImage(new Uri("/ImagesSorcier/PNG/wizard_fire/1_IDLE_000.png", UriKind.Relative));
            else if (MainWindow.Joueur2Choix == "Glace")
                imgJoueur2.Source = new BitmapImage(new Uri("/ImagesSorcier/PNG/wizard_ice/1_IDLE_000.png", UriKind.Relative));
            else if (MainWindow.Joueur2Choix == "Orage")
                imgJoueur2.Source = new BitmapImage(new Uri("/ImagesSorcier/PNG/wizard/1_IDLE_000.png", UriKind.Relative));
        }


    }
 }

