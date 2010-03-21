using System;
using System.Diagnostics;
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
                CreateNewLand();
            }
            scene.MouseMove += scene_MouseMove;
            scene.MouseLeftButtonDown += scene_MouseLeftButtonDown;
            scene.MouseLeftButtonUp += scene_MouseLeftButtonUp;
        }

        private void CreateNewLand() {
            for (int x = 0; x < 24; ++x) {
                this.scene.AddRectPolygon(1100, x * 200, 200, 100);
            }
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

        private string SaveDir { get { return "Radat"; } }

        public void StartGame() {
            using (var game = new SeikkailuLaaksoGame()) {
                int i = 1;
                var items = new System.Collections.ArrayList(scene.Children);
                foreach (FrameworkElement sceneItem in items) {
                    try {
                        var encoder = RenderToBitmap(sceneItem);
                        string picFile = Path.Combine(SaveDir, "scene_item_" + i.ToString() + ".png");
                        using (var picStream = new FileStream(picFile, FileMode.Create)) {
                            encoder.Save(picStream);
                        }
                        game.AddPolygonObstacle(picFile, Canvas.GetLeft(sceneItem), Canvas.GetTop(sceneItem));
                        ++i;
                    } catch (Exception ex) {
                        Debug.WriteLine(ex);
                        scene.Children.Remove(sceneItem);
                    }
                }
                game.Run();
            }
        }

        private void PlayGame_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        [STAThread]
        public static void Main()
        {
            new Application().Run(new AlkuIkkuna());
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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(SaveDir)) Directory.CreateDirectory(SaveDir);
            using (var stream = new FileStream(this.SaveFileName, FileMode.Create))
            {
                XamlWriter.Save(scene, stream);
            }
        }

    } // class

} // namespace
