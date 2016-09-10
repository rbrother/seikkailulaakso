using System;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Net.Brotherus.SeikkailuLaakso
{
    public class PolygonObstacle
    {
        private Body _body;
        private Texture2D _texture;
        private int _width;
        private int _rotate;
        private bool _flip;
        private bool _scroll; // Wether obstacle should participate in scrolling or be fixed one (to screen)
        private Color _color;

        public PolygonObstacle(Vector2 position, int width, int rotate, bool flip, World World, bool scroll) :
            this(position, width, rotate, flip, World, new Color(200,80,20), scroll) {
        }

        public PolygonObstacle(Vector2 position, int width, int rotate, bool flip, World World, Color color, bool scroll) :
            this(position, width, rotate, flip, ObstacleVertices(width, rotate, flip), World, scroll) {
            _color = color;
        }

        private static Vertices ObstacleVertices(int width, float rotate, bool flip) {
            var corners = flip ? new Vector2[] { 
                new Vector2(0f,0f),
                new Vector2(width*60f, -60f),
                new Vector2(width*60f, 0f),
            } : new Vector2[] { 
                new Vector2(0f,0f),
                new Vector2(width*60f, 0f),
                new Vector2(width*60f, 60f),
            };
            var vert = new Vertices(corners);
            vert.Rotate(rotate.ToRadians());
            var flipVect = new Vector2(1f, -1f);
            return vert;
        }

        private PolygonObstacle(Vector2 position, int width, int rotate, bool flip, Vertices vertices, World World, bool scroll) {
            _scroll = scroll;
            _width = width;
            _rotate = rotate;
            _flip = flip;
            _texture = AssetCreator.Instance.TextureFromVertices(vertices, MaterialType.Squares, new Color(255, 0, 0), 4f);
            _body = CreateBody(position, World);
            FixtureFactory.AttachPolygon(vertices, 0.1f, _body);
        }

        public PolygonObstacle(XElement data, World World) :
            this(
                new Vector2(
                    Convert.ToSingle(data.Attribute("x").Value), 
                    Convert.ToSingle(data.Attribute("y").Value)
                ),
                Convert.ToInt32(data.Attribute("width").Value),
                Convert.ToInt32(data.Attribute("rotate").Value),
                Convert.ToBoolean(data.Attribute("flip").Value),
                World, XmlToColor(data.Element("Color")), true
            ) {
        }

        public PolygonObstacle Clone( Vector2 newPos, Color color, World World ) {
            return new PolygonObstacle(newPos, _width, _rotate, _flip, World, color, true);
        }

        public int Width { get { return _texture.Width; } }

        public int Height { get { return _texture.Height; } }

        public bool Scroll { get { return _scroll; } }

        public Color Color { get { return _color; } }

        private static Color XmlToColor(XElement color) {
            if (color == null) return new Color(255,128,0);
            return new Color(
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

        private static Body CreateBody(Vector2 position, World World) {
            var b = BodyFactory.CreateBody(World, position); // dummy moment of inertia and mass since static
            b.IsStatic = true;
            return b;
        }

        public Vector2 Position {
            get { return _body.Position; }
            set { _body.Position = value; }
        }

        public Body Body { get { return _body; } }

        public void Draw(Action<Texture2D, Vector2 /*pos*/, float /*rot*/, Vector2 /*origin*/> drawer)
        {
            drawer(_texture, _body.Position, 0, Vector2.Zero);
        }
    }
}