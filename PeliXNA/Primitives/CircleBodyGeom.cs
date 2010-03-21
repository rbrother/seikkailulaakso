using System;
using System.Collections.Generic;
using System.Text;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.GettingStarted.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2Fs = FarseerGames.FarseerPhysics.Mathematics.Vector2;
using Vector2Xna = Microsoft.Xna.Framework.Vector2;

namespace Net.Brotherus {
    public class CircleBodyGeom {
        private Vector2Fs _origin;
        private Body _body;
        private Geom _geom;
        private Texture2D _texture;
        private GraphicsDevice _graphicsDevice;
        private PhysicsSimulator _physicsSimulator;
        private readonly int _radius;

        public CircleBodyGeom(Vector2Fs position, int radius, float mass, GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator) {
            _graphicsDevice = graphicsDevice;
            _physicsSimulator = physicsSimulator;
            _radius = radius;

            _origin = new Vector2Fs(radius + 2, radius + 2);
            _body = BodyFactory.Instance.CreateCircleBody(physicsSimulator, radius, mass);
            _body.Position = position;
            _geom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, _body, radius, 32);
            _geom.RestitutionCoefficient = 0.0f;
            _geom.FrictionCoefficient = 1.0f;
            _geom.CollisionCategories = CollisionCategory.All;
            _geom.CollidesWith = CollisionCategory.All;
            this.Color = Color.Yellow;
        }

        public float FrictionCoefficient {
            get { return _geom.FrictionCoefficient; }
            set { _geom.FrictionCoefficient = value; }
        }

        public Body Body { get { return _body; } }

        public Geom Geom { get { return _geom; } }

        public Vector2Fs Position { get { return _body.Position; } }

        public float AngularVelocity {
            get { return _body.AngularVelocity; }
            set { _body.AngularVelocity = value; }
        }

        public Vector2Fs Velocity {
            get { return Body.LinearVelocity; }
        }

        public bool IsStatic { set { _body.IsStatic = value; } }

        public int CollisionGroup { set { _geom.CollisionGroup = value; } }

        public float RotationDeg {
            get { return (float) _body.Rotation.ToDegrees(); }
            set { _body.Rotation = value.ToRadians(); }
        }

        public float RotationRad { get { return _body.Rotation; } }

        public Color Color {
            set {
                _texture = DrawingHelper.CreateCircleTexture(_graphicsDevice, _radius, value, Color.Black);
            }
        }

        public void Draw(Action<Texture2D, Vector2Fs /*pos*/, float /*rot*/, Vector2Fs /*origin*/> drawer) {
            drawer(_texture, _body.Position, _body.Rotation, _origin);
        }

    }
}
