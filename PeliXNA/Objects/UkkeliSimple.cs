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
using Vector2Fs = FarseerGames.FarseerPhysics.Mathematics.Vector2;
using Vector2Xna = Microsoft.Xna.Framework.Vector2;

namespace Net.Brotherus {

    public class UkkeliSimple {

        private Vector2Fs _origin;
        private Body _body;
        private Geom _feetGeom;
        private Geom _torsoGeom;
        private GraphicsDevice _graphicsDevice;
        private PhysicsSimulator _physicsSimulator;

        private const float MAX_WALK_SPEED = 300.0f;

        private int Radius { get { return 42; } }

        private bool _wantJump;
        private DateTime _jumpPressed;
        private Texture2D _ukkoImage;

        public UkkeliSimple(Vector2Fs position, float tileSize, GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator) {
            _physicsSimulator = physicsSimulator;
            _graphicsDevice = graphicsDevice;

            _origin = new Vector2Fs(44, 128);
            _body = BodyFactory.Instance.CreateCircleBody(physicsSimulator, Radius, 0.2f);
            _body.Position = position;

            _feetGeom = GeomFactory.Instance.CreateCircleGeom(_physicsSimulator, Body, Radius * 0.5f, 16);
            _feetGeom.RestitutionCoefficient = 0.0f;
            _feetGeom.FrictionCoefficient = 1.0f;
            _feetGeom.CollisionCategories = CollisionCategory.All;
            _feetGeom.CollidesWith = CollisionCategory.All;

            _torsoGeom = GeomFactory.Instance.CreateCircleGeom(_physicsSimulator, Body, Radius, 16, new Vector2Fs(0, -80), 0.0f);
            _torsoGeom.RestitutionCoefficient = 0.0f;
            _torsoGeom.FrictionCoefficient = 1.0f;
            _torsoGeom.CollisionCategories = CollisionCategory.All;
            _torsoGeom.CollidesWith = CollisionCategory.All;
            
            // Make a joint to keep feet low and torso up
            var joint = JointFactory.Instance.CreateFixedAngleJoint(physicsSimulator, _body);
            joint.TargetAngle = 0.0f;

            _ukkoImage = Texture2D.FromFile(_graphicsDevice, "Content/ukko1.png");

        }

        internal void HandleKeyboardInput(InputState input, GameTime gameTime) {
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right)) {
                if (Velocity.X < MAX_WALK_SPEED) {
                    Walk(1.0f);
                }
            } else if (input.CurrentKeyboardState.IsKeyDown(Keys.Left)) {
                if (Velocity.X > -MAX_WALK_SPEED) {
                    Walk(-1.0f);
                }
            } else { // decelerate from walking
                if (!FlyingInAir) {
                    if ( Velocity.X > 10.0f ) {
                        Walk(-1.0f);
                    } else if ( Velocity.X < -10.0f ) {
                        Walk(1.0f);
                    } else {
                        Body.LinearVelocity = new Vector2Fs(0, Body.LinearVelocity.Y);
                    }                    
                }                
            }
            // jumping
            if (input.IsNewKeyPress(Keys.Z)) {
                _wantJump = true;
                _jumpPressed = DateTime.Now;
            }
            if (ShortTimeFromJumpPressed && _wantJump && !FlyingInAir) {
                // Really do the jump
                Body.ApplyImpulse(new Vector2Fs(0.0f, -150.0f));
                _wantJump = false;
            }
        }

        private bool ShortTimeFromJumpPressed { get { return FromJumpPressed.TotalMilliseconds < 100; } }

        private TimeSpan FromJumpPressed { get { return DateTime.Now - _jumpPressed; } }

        public bool FlyingInAir {
            get {
                foreach( Geom g in _physicsSimulator.GeomList) {
                    if (!Object.ReferenceEquals(g, Geom)) {
                        if (Geom.Collide(g)) return false;
                    }
                }
                return true;
            }
        }

        private void Walk(float dir) {
            Body.ApplyForce(new Vector2Fs(200.0f * dir, 0));
        }

        public float FrictionCoefficient {
            get { return Geom.FrictionCoefficient; }
            set { Geom.FrictionCoefficient = value; }
        }

        public Body Body { get { return _body; } }

        public Geom Geom { get { return _feetGeom; }  }

        public Vector2Fs Position { get { return Body.Position; } }

        public double Rotation { get { return RotationRaw > 359.0 ? RotationRaw - 360.0 : RotationRaw; } }

        private double RotationRaw { get { return _body.Rotation.ToDegrees(); } }

        public Vector2Fs Velocity {
            get { return Body.LinearVelocity; }
        }

        public int CollisionGroup { set { Geom.CollisionGroup = value; } }

        public void Draw(Action<Texture2D, Vector2Fs /*pos*/, float /*rot*/, Vector2Fs /*origin*/> drawer) {
            drawer(UkkoImage, Body.Position, 0, _origin);
        }

        private Texture2D UkkoImage {
            get {
                return _ukkoImage;
            }
        }

    } // class

} // namespace
