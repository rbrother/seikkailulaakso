using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Net.Brotherus.SeikkailuLaakso
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AlkuIkkuna : Window
    {
        [STAThread]
        public static void Main() {
            new Application().Run(new AlkuIkkuna());
        }

        public AlkuIkkuna()
        {
            InitializeComponent();
        }

        private void Aloitetaan(object sender, RoutedEventArgs e)
        {
            var game = new SeikkailuLaaksoGame();
            game.Run();
        }

        private void UkkelinMuokkaus(object sender, RoutedEventArgs e) {
            new UkkelinMuokkaus().Show();
        }
    }
}
