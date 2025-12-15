using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static DeadlySpells.MainWindow;

namespace DeadlySpells
{
    /// <summary>
    /// Logique d'interaction pour UCChoixPerso.xaml
    /// </summary>
    public partial class UCChoixPerso : UserControl
    {
        private MageType? _mageJ1;
        private MageType? _mageJ2;

        private void butSuivant_Click(object sender, RoutedEventArgs e)
        {
           

            // Récupérer le texte choisi pour Joueur 1
            ComboBoxItem persoJ1 = (ComboBoxItem)cbJ1.SelectedItem;
            StackPanel stackpanelJ1 = (StackPanel)persoJ1.Content;
            Image imageJ1 = (Image)stackpanelJ1.Children[0];
            TextBlock textblockJ1 = (TextBlock)stackpanelJ1.Children[1];
            MainWindow.Joueur1Choix = textblockJ1.Text;



            // Récupérer le texte choisi pour Joueur 2
            ComboBoxItem persoJ2 = (ComboBoxItem)cbJ2.SelectedItem;
            StackPanel stackpanelJ2 = (StackPanel)persoJ2.Content;
            Image imageJ2 = (Image)stackpanelJ1.Children[0];
            TextBlock textblockJ2 = (TextBlock)stackpanelJ2.Children[1];
            MainWindow.Joueur2Choix = textblockJ2.Text;

            // Ensuite, aller vers la page de choix des maps
            
        }


        public UCChoixPerso()
        {
            InitializeComponent();
            butSuivant.Click += butSuivant_Click;

        }


        private void butRetour_Click(object sender, RoutedEventArgs e)
        {

        }


    }
}
