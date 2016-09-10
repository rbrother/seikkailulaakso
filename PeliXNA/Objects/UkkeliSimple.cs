using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;

namespace Net.Brotherus {

    public class UkkeliSimple {

        private Vector2 _origin;
        private Body _body;
        private Fixture _feetFixture;
        private Fixture _torsoFixture;
        private GraphicsDevice _graphicsDevice;
        private World _World;

        private const float MAX_WALK_SPEED = 300.0f;

        private int radius = 42;
        private float density = 0.2f;

        private bool _wantJump;
        private DateTime _jumpPressed;
        private Texture2D _ukkoImage;

        public UkkeliSimple(Vector2 position, float tileSize, GraphicsDevice graphicsDevice, World World) {
            _World = World;
            _graphicsDevice = graphicsDevice;

            _origin = new Vector2(44, 128);
            _body = BodyFactory.CreateBody(World, position);
            _body.BodyType = BodyType.Dynamic;

            _torsoFixture = FixtureFactory.AttachCircle(radius, density, _body, new Vector2(0.0f, 20.0f) );
            _feetFixture = FixtureFactory.AttachCircle(radius, density, _body, new Vector2(0.0f, -20.0f));

            
            // Make a joint to keep feet low and torso up
            var joint = JointFactory.CreateFixedAngleJoint(World, _body);
            joint.TargetAngle = 0.0f;

            _ukkoImage = AssetCreator.Instance.TextureFromFile("Content/ukko1.png");

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
                        Body.LinearVelocity = new Vector2(0, Body.LinearVelocity.Y);
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
                Body.ApplyLinearImpulse(new Vector2(0.0f, -150.0f));
                _wantJump = false;
            }
        }

        private bool ShortTimeFromJumpPressed { get { return FromJumpPressed.TotalMilliseconds < 100; } }

        private TimeSpan FromJumpPressed { get { return DateTime.Now - _jumpPressed; } }

        public bool FlyingInAir {
            get {
                foreach( Contact contact in _World.ContactList) {
                    if (object.Equals( contact.FixtureA, _feetFixture ) || object.Equals( contact.FixtureB, _feetFixture ) ) {
                        return false;
                    }
                }
                return true;
            }
        }

        private void Walk(float dir) {
            Body.ApplyForce(new Vector2(200.0f * dir, 0));
        }

        public Body Body { get { return _body; } }

        public Fixture Geom { get { return _feetFixture; }  }

        public Vector2 Position { get { return Body.Position; } }

        public double Rotation { get { return RotationRaw > 359.0 ? RotationRaw - 360.0 : RotationRaw; } }

        private double RotationRaw { get { return _body.Rotation.ToDegrees(); } }

        public Vector2 Velocity {
            get { return Body.LinearVelocity; }
        }

        public void Draw(Action<Texture2D, Vector2 /*pos*/, float /*rot*/, Vector2 /*origin*/> drawer) {
            drawer(UkkoImage, Body.Position, 0, _origin);
        }

        private Texture2D UkkoImage {
            get {
                return _ukkoImage;
            }
        }

    } // class

} // namespace
