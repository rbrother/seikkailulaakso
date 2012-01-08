using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Net.Brotherus {
    
    public class TextureGenerator {

        private readonly int TILE_SIZE = 60;
        private GraphicsDevice _graphicsDevice;
        private Dictionary<string, Texture2D> _cache;
        
        public TextureGenerator(GraphicsDevice graphicsDevice) {
            _graphicsDevice = graphicsDevice;
            _cache = new Dictionary<string, Texture2D>();
        }

        public int CacheCount { get { return _cache.Count; } }

        public Texture2D RectangleTile(int width, int height, int rot) {
            return RectangleTile(width, height, rot, System.Drawing.Color.Red);
        }

        public Texture2D RectangleTile(int width, int height, int rot, System.Drawing.Color color) {
            return MakeTile(
                new PointF[] {
                    new PointF(0.0f, 0.0f),
                    new PointF(TILE_SIZE * width, 0.0f),
                    new PointF(TILE_SIZE * width, TILE_SIZE * height),
                    new PointF(0.0f, TILE_SIZE * height),
                    new PointF(0.0f, 0.0f)
                },
                rot, false, color
            );
        }        

        public Texture2D TriangleTile( int width, int rot, bool flip ) {
            return TriangleTile( width, rot, flip, System.Drawing.Color.Red );            
        }
            
        public Texture2D TriangleTile( int width, int rot, bool flip, System.Drawing.Color color ) {
            return MakeTile(
                new PointF[] {
                    new PointF(0.0f, 0.0f),
                    new PointF(TILE_SIZE * width, 0.0f),
                    new PointF(0.0f, TILE_SIZE),
                    new PointF(0.0f, 0.0f)
                },
                rot, flip, color
            );
        }
          
        private Texture2D MakeTile( IEnumerable<PointF> points, int rotate, bool flip, System.Drawing.Color color ) {
            string key = string.Format("{0}-{1}-{2}", rotate, flip, color);
            foreach (PointF p in points) { key += string.Format("-({0},{1})", p.X, p.Y); }
            if (!_cache.ContainsKey(key)) {
                int width = Convert.ToInt32(points.Max(p => p.X));
                int height = Convert.ToInt32(points.Max(p => p.Y));
                Bitmap dest_bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                Graphics dst_graphics = Graphics.FromImage(dest_bmp);
                dst_graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                dst_graphics.SmoothingMode = SmoothingMode.HighQuality;
                dst_graphics.FillPolygon(new SolidBrush(color), points.ToArray());
                dst_graphics.DrawPolygon(new Pen(System.Drawing.Color.Black, 2), points.ToArray());
                dest_bmp.RotateFlip(RotateType(rotate, flip));
                dest_bmp.Save(TempFile);
                _cache[key] = Texture2D.FromFile(_graphicsDevice, TempFile);
            }
            return _cache[key];
        }

        private RotateFlipType RotateType(int rotate, bool flip) {
            if (rotate == 0 && !flip) {
                return RotateFlipType.RotateNoneFlipNone;
            } else if (rotate == 90 && !flip) {
                return RotateFlipType.Rotate90FlipNone;
            } else if (rotate == 180 && !flip) {
                return RotateFlipType.Rotate180FlipNone;
            } else if (rotate == 270 && !flip) {
                return RotateFlipType.Rotate270FlipNone;
            } else if (rotate == 0 && flip) {
                return RotateFlipType.RotateNoneFlipX;
            } else if (rotate == 90 && flip) {
                return RotateFlipType.Rotate90FlipX;
            } else if (rotate == 180 && flip) {
                return RotateFlipType.Rotate180FlipX;
            } else if (rotate == 270 && flip) {
                return RotateFlipType.Rotate270FlipX;
            } else {
                throw new ArgumentOutOfRangeException("rotate");
            }
        }

        private string TempFile {
            get {
                return Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "texture.bmp");
            }
        }

    } // class

} // namespace
