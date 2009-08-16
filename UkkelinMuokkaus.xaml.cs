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
using System.Windows.Shapes;

namespace Net.Brotherus.SeikkailuLaakso
{
    /// <summary>
    /// Interaction logic for UkkelinMuokkaus.xaml
    /// </summary>
    public partial class UkkelinMuokkaus : Window
    {
        public UkkelinMuokkaus()
        {
            InitializeComponent();
        }

        private void VaatteetAla_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetPic(AlaosaPic, VaatteetAla.SelectedItem as ComboBoxItem);
        }

        private void VaatteetPaa_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetPic(PaaPic, VaatteetPaa.SelectedItem as ComboBoxItem);
        }

        private void VaatteetYla_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetPic(YlaosaPic, VaatteetYla.SelectedItem as ComboBoxItem);
        }

        private void Jalassa_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetPic(KengatPic, Jalassa.SelectedItem as ComboBoxItem);            
        }

        private void Takana_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetPic(TakanaPic, Takana.SelectedItem as ComboBoxItem);            
        }

        private void Kasvot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetPic(KasvotPic, Kasvot.SelectedItem as ComboBoxItem);            
        }

        private static void SetPic(Image img, ComboBoxItem selectedItem)
        {
            if (selectedItem.Content == null) return;
            string s = selectedItem.Content.ToString().ToLower();
            if (s == "ei mitään")
            {
                img.Source = null;
            }
            else
            {
                var picUri = string.Format("pack://application:,,,/ukkelit/{0}.png", s);
                img.Source = new BitmapImage(new Uri(picUri));
            }

        }


    }
}
