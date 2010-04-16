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
        private string _textureName;
        private ITextureCache _textureCache;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">top-left position</param>
        /// <param name="textureFile"></param>
        public PolygonObstacle(Vector2Fs position, string textureName, PhysicsSimulator physicsSimulator, ITextureCache textureCache) {
            _textureName = textureName;
            _textureCache = textureCache;

            _body = CreateBody(position, physicsSimulator);
            _geom = CreateGeom(_body, PolygonVertices);

            physicsSimulator.Add(_geom);
        }

        public PolygonObstacle(XElement data, PhysicsSimulator physicsSimulator, ITextureCache textureCache) :
            this(
                new Vector2Fs(
                    Convert.ToSingle(data.Attribute("x").Value), 
                    Convert.ToSingle(data.Attribute("y").Value)
                ), 
                data.Attribute("texture").Value, 
                physicsSimulator, textureCache
            ) {
        }

        public XElement Xml {
            get {
                return new XElement("Obstacle",
                    new XAttribute("x", _body.Position.X),
                    new XAttribute("y", _body.Position.Y),
                    new XAttribute("texture", _textureName)
                );
            }
        }

        private Texture2D Texture {
            get { return _textureCache.GetTexture(_textureName); }
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

        public void Draw(Action<string, Vector2Fs /*pos*/, float /*rot*/, Vector2Fs /*origin*/> drawer)
        {
            drawer(_textureName, _body.Position, 0, Vector2Fs.Zero);
        }
    }
}