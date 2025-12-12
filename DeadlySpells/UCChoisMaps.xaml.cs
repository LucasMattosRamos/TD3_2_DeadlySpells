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

namespace DeadlySpells
{
    /// <summary>
    /// Logique d'interaction pour UCChoisMaps.xaml
    /// </summary>
    public partial class UCChoisMaps : UserControl
    {
        public UCChoisMaps()
        {
            InitializeComponent();
        }

        // On remplit la variable TEXTE de MainWindow
        private void RadMapFeu_Click(object sender, RoutedEventArgs e)
        {
            RadMapFeu.IsEnabled = true;
            MainWindow.ChoixMap = "Feu";
        }

        private void RadMapGlace_Click(object sender, RoutedEventArgs e)
        {
            RadMapGlace.IsEnabled = true;
            MainWindow.ChoixMap = "Glace";
        }

        private void RadMapTombe_Click(object sender, RoutedEventArgs e)
        {
            RadMapTombe.IsEnabled = true;
            MainWindow.ChoixMap = "Tombe";
        }

        private void butJouer_Click(object sender, RoutedEventArgs e)
        {
            // Vide, car c'est MainWindow qui gère le clic maintenant via le +=
        }
    }
}
