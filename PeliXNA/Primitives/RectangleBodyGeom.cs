﻿using System;
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

namespace Net.Brotherus
{
    public class RectangleBodyGeom
    {
        private Vector2 _origin;
        private Body _body;
        private Geom _geom;
        private Texture2D _texture;
        private GraphicsDevice _graphicsDevice;
        private PhysicsSimulator _physicsSimulator;
        private readonly int _width;
        private readonly int _height;

        public RectangleBodyGeom(Vector2 position, Vector2 size, float mass, GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator) :
            this(position, (int) size.X, (int) size.Y, mass, graphicsDevice, physicsSimulator)
        {
        }

        public RectangleBodyGeom(Vector2 position, int width, int height, float mass, GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _graphicsDevice = graphicsDevice;
            _physicsSimulator = physicsSimulator;
            _width = width;
            _height = height;

            _origin = new Vector2(width * 0.5f, height * 0.5f);
            _body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, width, height, mass);
            _body.Position = position;
            _geom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _body, width, height, Vector2.Zero /*offset*/, 0 /*rotation offset*/);
            _geom.RestitutionCoefficient = 0.0f;
            _geom.FrictionCoefficient = 1.0f;
            _geom.CollisionGroup = 1;
            _geom.CollisionCategories = CollisionCategory.All;
            _geom.CollidesWith = CollisionCategory.All;
            this.Color = Color.Yellow;
        }

        public float FrictionCoefficient
        {
            get { return _geom.FrictionCoefficient; }
            set { _geom.FrictionCoefficient = value;  }
        }

        public Body Body { get { return _body; } }

        public Geom Geom { get { return _geom; } }

        public Vector2 Position { get { return _body.Position; } }

        public float Rotation { get { return _body.Rotation; } }

        public bool IsStatic { set { _body.IsStatic = value; } }

        public int CollisionGroup { set { _geom.CollisionGroup = value; } }

        public float RotationDeg 
        {
            get { return (float) MathExt.ToDegrees(_body.Rotation);  }
            set { _body.Rotation = MathExt.ToRadians(value); } 
        }

        public float RotationRad { get { return _body.Rotation; } }

        public Color Color { 
            set { 
                _texture = DrawingHelper.CreateRectangleTexture(_graphicsDevice, _width, _height, value, Color.Black);
            } 
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _body.Position, null, Color.White, _body.Rotation, _origin, 1, SpriteEffects.None, 0);
        }

        internal AngleJoint CreateAngleJoint(PhysicsSimulator _physicsSimulator, RectangleBodyGeom b, float targetAngle)
        {
            var joint = JointFactory.Instance.CreateAngleJoint(_physicsSimulator, _body, b._body);
            joint.TargetAngle = targetAngle;
            joint.Softness = 0.8f;
            joint.MaxImpulse = 300.0f;
            return joint;
        }

        internal RevoluteJoint CreateRevoluteJoint(PhysicsSimulator physicsSimulator, RectangleBodyGeom b, Vector2 relativeAnchorPosition)
        {
            return JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, _body, b._body, _body.Position + relativeAnchorPosition);
        }

    }
}
