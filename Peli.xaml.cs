using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Polygon = System.Windows.Shapes.Polygon;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Net.Brotherus.SeikkailuLaakso
{
    /// <summary>
    /// Interaction logic for Peli.xaml
    /// </summary>
    public partial class Peli : Window
    {
        private SceneCanvas scene;

        public Peli() {
            InitializeComponent();
            if (File.Exists(this.SaveFileName)) {
                using (var stream = new FileStream(this.SaveFileName, FileMode.Open)) {   
                    view.Content = XamlReader.Load(stream);
                }
            }
            this.scene = (SceneCanvas) view.Content;
            // Create land if no previous content
            if (this.scene.Children.Count == 0)
            {
                for(int x = 0; x < 12; ++x) {
                    this.scene.AddRectPolygon(900, x * 200, 200, 100);
                }
            }
            scene.MouseMove += scene_MouseMove;
            scene.MouseLeftButtonDown += scene_MouseLeftButtonDown;
            scene.MouseLeftButtonUp += scene_MouseLeftButtonUp;
        }

        private void scene_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            scene.StartDrawing(e);
        }

        private void scene_MouseMove(object sender, MouseEventArgs e)
        {
            scene.ContinueDrawing(e);
        }

        private void scene_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            scene.EndDrawing(e);
        }

        private string SaveFileName
        {
            get { return Path.Combine(SaveDir, "SeikkailuMaaRata.xml"); }
        }

        private string SaveDir
        {
            get 
            { 
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SeikkailuMaa"); 
            }
        }

        private void PlayGame_Click(object sender, RoutedEventArgs e)
        {
            using (var game = new SeikkailuLaaksoGame())
            {
                int i = 1;
                foreach (FrameworkElement sceneItem in scene.Children)
                {
                    var encoder = RenderToBitmap(sceneItem);
                    string picFile = Path.Combine(SaveDir, "scene_item_" + i.ToString() + ".png");
                    using (var picStream = new FileStream(picFile, FileMode.Create)) {
                        encoder.Save(picStream);
                    }
                    game.AddPolygonObstacle(picFile, Canvas.GetLeft(sceneItem), Canvas.GetTop(sceneItem));
                    ++i;
                }
                game.Run();
            }
        }

        [STAThread]
        public static void Main()
        {
            //new Application().Run(new AlkuIkkuna());

            new Application().Run(new Peli());

            //new Peli().PlayGame_Click(null, null);
        }

        private static PngBitmapEncoder RenderToBitmap(FrameworkElement sceneItem)
        {
            UIElement dupItem = (UIElement)XamlReader.Parse(XamlWriter.Save(sceneItem));
            Window renderWindow = new Window { Top = 10, Left = 10 };
            renderWindow.SizeToContent = SizeToContent.WidthAndHeight;
            renderWindow.Content = dupItem;
            renderWindow.Show();
            var targetBmp = new RenderTargetBitmap(
                Convert.ToInt32(sceneItem.ActualWidth), Convert.ToInt32(sceneItem.ActualHeight),
                96, 96, PixelFormats.Default);
            targetBmp.Render(dupItem);
            renderWindow.Close();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(targetBmp));
            return encoder;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(SaveDir)) Directory.CreateDirectory(SaveDir);
            using (var stream = new FileStream(this.SaveFileName, FileMode.Create))
            {
                XamlWriter.Save(scene, stream);
            }
        }

    } // class

} // namespace
