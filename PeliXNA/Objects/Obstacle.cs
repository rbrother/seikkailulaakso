using System;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.GettingStarted.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2Fs = FarseerGames.FarseerPhysics.Mathematics.Vector2;
using Vector2Xna = Microsoft.Xna.Framework.Vector2;

namespace Net.Brotherus.SeikkailuLaakso
{
    public class PolygonObstacle
    {
        private Geom _geom;
        private Body _body;
        private Texture2D _texture;
        private int _width;
        private int _rotate;
        private bool _flip;
        private bool _scroll; // Wether obstacle should participate in scrolling or be fixed one (to screen)
        private System.Drawing.Color _color;

        public PolygonObstacle(Vector2Fs position, int width, int rotate, bool flip, PhysicsSimulator physicsSimulator, TextureGenerator textureCache, bool scroll) :
            this(position, width, rotate, flip, physicsSimulator, textureCache, System.Drawing.Color.FromArgb(200,80,20), scroll) {
        }

        public PolygonObstacle(Vector2Fs position, int width, int rotate, bool flip, PhysicsSimulator physicsSimulator, TextureGenerator textureCache, System.Drawing.Color color, bool scroll) :
            this(position, width, rotate, flip, textureCache.TriangleTile(width, rotate, flip, color), physicsSimulator, scroll) {
            _color = color;
        }

        private PolygonObstacle(Vector2Fs position, int width, int rotate, bool flip, Texture2D texture, PhysicsSimulator physicsSimulator, bool scroll) {
            _scroll = scroll;
            _width = width;
            _rotate = rotate;
            _flip = flip;
            _texture = texture;
            _body = CreateBody(position, physicsSimulator);
            _geom = CreateGeom(_body, PolygonVertices);
            physicsSimulator.Add(_geom);
        }

        public PolygonObstacle(XElement data, PhysicsSimulator physicsSimulator, TextureGenerator textureCache) :
            this(
                new Vector2Fs(
                    Convert.ToSingle(data.Attribute("x").Value), 
                    Convert.ToSingle(data.Attribute("y").Value)
                ),
                Convert.ToInt32(data.Attribute("width").Value),
                Convert.ToInt32(data.Attribute("rotate").Value),
                Convert.ToBoolean(data.Attribute("flip").Value),
                physicsSimulator, textureCache, XmlToColor(data.Element("Color")), true
            ) {
        }

        public PolygonObstacle Clone( Vector2Fs newPos, System.Drawing.Color color, PhysicsSimulator physicsSimulator, TextureGenerator textureGenerator ) {
            return new PolygonObstacle(newPos, _width, _rotate, _flip, physicsSimulator, textureGenerator, color, true);
        }

        public int Width { get { return _texture.Width; } }

        public int Height { get { return _texture.Height; } }

        public bool Scroll { get { return _scroll; } }

        public System.Drawing.Color Color { get { return _color; } }

        private static System.Drawing.Color XmlToColor(XElement color) {
            if (color == null) return System.Drawing.Color.Brown;
            return System.Drawing.Color.FromArgb(
                Convert.ToInt32( color.Attribute("red").Value ),
                Convert.ToInt32( color.Attribute("green").Value ),
                Convert.ToInt32( color.Attribute("blue").Value )
                );
        }

        public XElement Xml {
            get {
                return new XElement("Obstacle",
                    new XAttribute("x", _body.Position.X),
                    new XAttribute("y", _body.Position.Y),
                    new XAttribute("width", _width),
                    new XAttribute("rotate", _rotate),
                    new XAttribute("flip", _flip),
                    new XElement("Color", 
                        new XAttribute("red", Color.R), 
                        new XAttribute("green", Color.G), 
                        new XAttribute("blue", Color.B)
                    )
                );
            }
        }

        private Texture2D Texture {
            get {
                return _texture; 
            }
        }

        private Vertices PolygonVertices {
            get {
                uint[] textureData = new uint[Texture.Width * Texture.Height];
                Texture.GetData<uint>(textureData);
                return Vertices.CreatePolygon(textureData, Texture.Width, Texture.Height);
            }
        }

        private static Body CreateBody(Vector2Fs position, PhysicsSimulator physicsSimulator) {
            var b = BodyFactory.Instance.CreateBody(physicsSimulator, 1, 1); // dummy moment of inertia and mass since static
            b.Position = position;
            b.IsStatic = true;
            return b;
        }

        private static Geom CreateGeom(Body b, Vertices poly) {
            // Don't use Geom factory: it shifts the vertices according to center-of-mass
            var g = new Geom(b, poly, Vector2Fs.Zero, 0, 1.0f);
            g.RestitutionCoefficient = 0.0f;
            return g;
        }

        public Vector2Fs Position {
            get { return _body.Position; }
            set { 
                _body.Position = value;
                _geom.SetBody(_body);
            }
        }

        public Body Body { get { return _body; } }

        public Geom Geom { get { return _geom; } }

        public void Draw(Action<Texture2D, Vector2Fs /*pos*/, float /*rot*/, Vector2Fs /*origin*/> drawer)
        {
            drawer(_texture, _body.Position, 0, Vector2Fs.Zero);
        }
    }
}