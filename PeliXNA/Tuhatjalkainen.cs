using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.GettingStarted.DrawingSystem;
using FarseerGames.FarseerPhysics.Factories;


namespace Net.Brotherus
{
    public class Tuhatjalkainen
    {
        private RectangleBodyGeom _torso;
        private List<LegPair> _legs = new List<LegPair>();

        private double _walkCycleTime = 0.0;

        private GraphicsDevice _graphicsDevice;
        private PhysicsSimulator _physicsSimulator;

        public Tuhatjalkainen(Vector2 position, GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _graphicsDevice = graphicsDevice;
            _physicsSimulator = physicsSimulator;

            // Torso
            _torso = new RectangleBodyGeom(position, Leg.PART_SIZE * 4, 10, _graphicsDevice, _physicsSimulator); ;

            for (float x = -Leg.PART_SIZE.X * 2; x <= Leg.PART_SIZE.X * 2 + 0.001; x += Leg.PART_SIZE.X)
            {
                _legs.Add(new LegPair(physicsSimulator, graphicsDevice, _torso, new Vector2(x, Leg.PART_SIZE.Y)));
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _torso.Draw(spriteBatch);
            foreach (var legPair in _legs) legPair.Draw(spriteBatch);
        }

        internal void HandleKeyboardInput(InputState input, GameTime gameTime)
        {
            if (input.CurrentKeyboardState.IsKeyDown(Keys.M))
            {
                Walk(gameTime, 1.0f);
            }
            else if (input.CurrentKeyboardState.IsKeyDown(Keys.N))
            {
                Walk(gameTime, -1.0f);
            }
        }

        public void Walk(GameTime gameTime, float dir)
        {
            _walkCycleTime += gameTime.ElapsedGameTime.TotalSeconds * 1.0f * dir;
            double phase = 0.0;
            foreach (var legPair in _legs)
            {
                legPair.Walk(_walkCycleTime + phase);
                phase += Math.PI / 4f;
            }
        }

    } // class

    public class LegPair
    {
        private Leg _left;
        private Leg _right;

        public LegPair(PhysicsSimulator physicsSimulator, GraphicsDevice graphicsDevice, RectangleBodyGeom torso, Vector2 pos)
        {
            _left = new Leg(physicsSimulator, graphicsDevice, torso, pos, 3);
            _right = new Leg(physicsSimulator, graphicsDevice, torso, pos, 3);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _left.Draw(spriteBatch);
            _right.Draw(spriteBatch);
        }

        public void Walk(double phase)
        {
            _left.Walk(phase);
            _right.Walk(phase + Math.PI);
        }

    }

    public class Leg
    {
        public static readonly Vector2 PART_SIZE = new Vector2(80, 16);
        private PhysicsSimulator _physicsSimulator;

        private RectangleBodyGeom _upper;
        private RectangleBodyGeom _lower;
        private AngleTargetLimitJoint _upperJoint;
        private AngleTargetLimitJoint _lowerJoint;

        public Leg(PhysicsSimulator physicsSimulator, GraphicsDevice graphicsDevice, RectangleBodyGeom torso,
            Vector2 attachPoint, float weight)
        {
            _physicsSimulator = physicsSimulator;

            _upper = new RectangleBodyGeom(torso.Position + attachPoint, PART_SIZE, weight, graphicsDevice, _physicsSimulator) { RotationDeg = 90 };
            _upperJoint = CreateJoint(torso, _upper, 0, 90, 180, attachPoint - new Vector2(0, PART_SIZE.X * 0.5f));
            _lower = new RectangleBodyGeom(torso.Position + attachPoint + new Vector2(0, PART_SIZE.X), PART_SIZE, weight, graphicsDevice, _physicsSimulator) { RotationDeg = 90 };
            _lowerJoint = CreateJoint(_upper, _lower, -135, 0, 135, new Vector2(0, PART_SIZE.X * 0.5f));

        }

        private AngleTargetLimitJoint CreateJoint(RectangleBodyGeom a, RectangleBodyGeom b,
            double minAngleDegrees, double targetAngleDegrees, double maxAngleDegrees, Vector2 jointRelPosition)
        {
            a.CreateRevoluteJoint(_physicsSimulator, b, jointRelPosition);
            return new AngleTargetLimitJoint(_physicsSimulator, a.Body, b.Body, minAngleDegrees, targetAngleDegrees, maxAngleDegrees);
        }

        public double TorsoTargetAngleDegrees { set { _upperJoint.TargetAngleDegrees = (float)value; } }

        public double KneeTargetAngleDegrees { set { _lowerJoint.TargetAngleDegrees = (float)value; } }

        public void Walk(double phase)
        {
            KneeTargetAngleDegrees = (1.0 - Math.Sin(phase)) * 90;
            TorsoTargetAngleDegrees = Math.Sin(phase - Math.PI * 0.5) * 30 + 90;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _upper.Draw(spriteBatch);
            _lower.Draw(spriteBatch);
        }
    }

} // namespace
