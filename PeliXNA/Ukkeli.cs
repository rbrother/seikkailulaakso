using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.GettingStarted.DrawingSystem;
using FarseerGames.FarseerPhysics.Factories;


namespace Net.Brotherus
{
    public class Ukkeli
    {
        private RectangleBodyGeom _torso;
        private RectangleBodyGeom _top;
        private CircleBodyGeom _feet;

        private RectangleBodyGeom[] _connectors;
        private RevoluteJoint[] _joints;
        private AngleJoint[] _springs;

        private GraphicsDevice _graphicsDevice;
        private PhysicsSimulator _physicsSimulator;

        public Ukkeli(Vector2 position, GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _graphicsDevice = graphicsDevice;
            _physicsSimulator = physicsSimulator;

            // Feet
            _feet = new CircleBodyGeom(position + new Vector2(0, 32), 32, 0.2f, graphicsDevice, physicsSimulator);
            _feet.CollisionGroup = 1;

            // Torso
            _torso = new RectangleBodyGeom(position, 32, 64, 0.2f, _graphicsDevice, _physicsSimulator);
            _torso.CollisionGroup = 1;
            var joint = JointFactory.Instance.CreateFixedAngleJoint(_physicsSimulator, _torso.Body);
            joint.TargetAngle = 0;
            joint.MaxImpulse = 800;

            // Feet-Torso
            JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, _torso.Body, _feet.Body, position + new Vector2(0, 32));

            // Top
            var connectorSize = new Vector2(8, 26);
            var connectorMass = 0.5f;
            _top = new RectangleBodyGeom(position + new Vector2(0, -112), 32, 32, 2, _graphicsDevice, _physicsSimulator) { CollisionGroup = 2 };
            _connectors = new RectangleBodyGeom[] {
                new RectangleBodyGeom(position + new Vector2(-16,-80), connectorSize, connectorMass, graphicsDevice, physicsSimulator) { CollisionGroup = 1 },
                new RectangleBodyGeom(position + new Vector2(16,-80), connectorSize, connectorMass, graphicsDevice, physicsSimulator) { CollisionGroup = 1 },
                new RectangleBodyGeom(position + new Vector2(-16,-48), connectorSize, connectorMass, graphicsDevice, physicsSimulator) { CollisionGroup = 1 },
                new RectangleBodyGeom(position + new Vector2(16,-48), connectorSize, connectorMass, graphicsDevice, physicsSimulator) { CollisionGroup = 1 }
            };
            _joints = new RevoluteJoint[] {
                JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, _connectors[0].Body, _top.Body, position + new Vector2(-16,-96) ),
                JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, _connectors[1].Body, _top.Body, position + new Vector2(16, -96)),
                JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, _connectors[0].Body, _connectors[2].Body, position + new Vector2(-16, -64)),
                JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, _connectors[1].Body, _connectors[3].Body, position + new Vector2(16, -64)),
                JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, _connectors[2].Body, _torso.Body, position + new Vector2(-16, -32)),
                JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, _connectors[3].Body, _torso.Body, position + new Vector2(16, -32))
            };
            foreach (var j in _joints)
            {
                j.Softness = 0.0f;
                j.BiasFactor = 0.5f; // Correct more strongly
            }

            _springs = new AngleJoint[] {
                JointFactory.Instance.CreateAngleJoint(physicsSimulator, _top.Body, _connectors[0].Body),
                JointFactory.Instance.CreateAngleJoint(physicsSimulator, _top.Body, _connectors[1].Body),
                JointFactory.Instance.CreateAngleJoint(physicsSimulator, _connectors[0].Body, _connectors[2].Body),
                JointFactory.Instance.CreateAngleJoint(physicsSimulator, _connectors[1].Body, _connectors[3].Body),
                JointFactory.Instance.CreateAngleJoint(physicsSimulator, _connectors[2].Body, _torso.Body),
                JointFactory.Instance.CreateAngleJoint(physicsSimulator, _connectors[3].Body, _torso.Body)
            };

            DoBounce = false;
        }

        public float MaxConnectorImpulse
        {
            set
            {
                foreach (var spring in _springs)
                {
                    spring.MaxImpulse = value;
                }
            }
        }

        public bool DoBounce
        {
            set
            {
                if (value)
                {
                    BounceAngle = 10;
                    MaxConnectorImpulse = 1000.0f;
                }
                else
                {
                    BounceAngle = 80;
                    MaxConnectorImpulse = 100.0f; // Less "bouncy" fall to ground
                }
            }
        }

        public float BounceAngle
        {
            set
            {
                var rad = MathExt.ToRadians(value);
                _springs[0].TargetAngle = rad;
                _springs[1].TargetAngle = -rad;
                _springs[2].TargetAngle = -rad*2;
                _springs[3].TargetAngle = rad*2;
                _springs[4].TargetAngle = rad;
                _springs[5].TargetAngle = -rad;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _torso.Draw(spriteBatch);
            _top.Draw(spriteBatch);
            _feet.Draw(spriteBatch);
            foreach (var connector in _connectors) connector.Draw(spriteBatch);
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
                DoBounce = true;
            }
            else if (input.IsKeyLifted(Keys.LeftShift))
            {
                DoBounce = false;
            }
        }

        private float TORQUE = 60000f;

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
