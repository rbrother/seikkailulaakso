using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.GettingStarted.DrawingSystem;
using FarseerGames.FarseerPhysics.Factories;


namespace Net.Brotherus
{
    public class Ukkeli
    {
        private RectangleBodyGeom _torso;
        private CircleBodyGeom _feet;

        private GraphicsDevice _graphicsDevice;
        private PhysicsSimulator _physicsSimulator;

        private int FEET_RADIUS = 30;
        private int FEET_DISTANCE = 65;

        public Ukkeli(Vector2 position, GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _graphicsDevice = graphicsDevice;
            _physicsSimulator = physicsSimulator;

            // Torso
            _torso = new RectangleBodyGeom(position, new Vector2(32, 96), 2, _graphicsDevice, _physicsSimulator);
            var joint = JointFactory.Instance.CreateFixedAngleJoint(_physicsSimulator, _torso.Body);
            joint.TargetAngle = 0.0f;
            joint.MaxImpulse = 300.0f;

            _feet = new CircleBodyGeom(position + new Vector2(0, FEET_DISTANCE), FEET_RADIUS, 1, graphicsDevice, physicsSimulator);
            //var _feetJoint = JointFactory.Instance.CreatePinJoint(physicsSimulator, _torso.Body, new Vector2(0, FEET_DISTANCE), _feet.Body, Vector2.Zero);
            JointFactory.Instance.CreateSliderJoint(physicsSimulator, _torso.Body, new Vector2(0, FEET_DISTANCE), _feet.Body, Vector2.Zero, 0, 20);
            SpringFactory.Instance.CreateLinearSpring(physicsSimulator, _torso.Body, Vector2.Zero, _feet.Body, Vector2.Zero, 1000, 5);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _torso.Draw(spriteBatch);
            _feet.Draw(spriteBatch);
        }

        internal void HandleKeyboardInput(InputState input, GameTime gameTime)
        {
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right))
            {
                Walk(1.0f);
            }
            else if (input.CurrentKeyboardState.IsKeyDown(Keys.Left))
            {
                Walk(-1.0f);
            }
            else // decelerate from walking
            {
                if (_feet.Body.AngularVelocity > 0.1f)
                {
                    _feet.Body.ApplyTorque(-TORQUE);
                }
                else if (_feet.Body.AngularVelocity < -0.1f)
                {
                    _feet.Body.ApplyTorque(TORQUE);
                }
            }
            // jumping
            if (input.IsNewKeyPress(Keys.LeftShift))
            {

            }
            else if (input.IsKeyLifted(Keys.LeftShift))
            {

            }
        }

        private float TORQUE = 30000f;

        private float MAX_WALK_ANGULAR_SPEED = 8.0f;

        public void Walk(float dir)
        {
            if (dir > 0.0f && _feet.Body.AngularVelocity < MAX_WALK_ANGULAR_SPEED)
            {
                _feet.Body.ApplyTorque(TORQUE * 2);
            }
            else if (dir < 0.0f && _feet.Body.AngularVelocity > -MAX_WALK_ANGULAR_SPEED)
            {
                _feet.Body.ApplyTorque(-TORQUE * 2);
            }
        }

    } // class


} // namespace
