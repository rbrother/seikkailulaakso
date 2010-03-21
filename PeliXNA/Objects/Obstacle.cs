using System;
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
        private string _textureFile;
        private Vector2Fs _position;

        private Geom _geom;
        private Body _body;
        private Texture2D _texture;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">top-left position</param>
        /// <param name="textureFile"></param>
        public PolygonObstacle(Vector2Fs position, string textureFile)
        {
            _position = position;
            _textureFile = textureFile;
        }

        /// <summary>
        /// This cannot be done in constructor since graphicsdevice and physicssimulator don't exist yet then.
        /// Load is called later
        /// </summary>
        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _texture = Texture2D.FromFile(graphicsDevice, _textureFile);
            uint[] textureData = new uint[_texture.Width * _texture.Height];
            _texture.GetData<uint>(textureData);
            Vertices poly = Vertices.CreatePolygon(textureData, _texture.Width, _texture.Height);
            _body = BodyFactory.Instance.CreateBody(physicsSimulator, 1, 1); // dummy moment of inertia and mass since static
            _body.Position = _position;
            _body.IsStatic = true;

            // Don't use Geom factory: it shifts the vertices according to center-of-mass
            _geom = new Geom(_body, poly, Vector2Fs.Zero, 0, 1.0f);
            _geom.RestitutionCoefficient = 0.0f;
            physicsSimulator.Add(_geom);            
        }

        public void Draw(Action<Texture2D, Vector2Fs /*pos*/, float /*rot*/, Vector2Fs /*origin*/> drawer)
        {
            drawer(_texture, _body.Position, 0, Vector2Fs.Zero);
        }
    }
}