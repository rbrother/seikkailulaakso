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
        private Geom _geom;
        private GraphicsDevice _graphicsDevice;
        private PhysicsSimulator _physicsSimulator;

        private const float MAX_WALK_SPEED = 300.0f;
        private HashSet<Geom> _collidingGeoms = new HashSet<Geom>();

        private int Radius { get { return 42; } }

        private bool _wantJump;
        private DateTime _jumpPressed;

        public UkkeliSimple(Vector2Fs position, float tileSize, GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator) {
            _physicsSimulator = physicsSimulator;
            _graphicsDevice = graphicsDevice;

            _origin = new Vector2Fs(44, 106);
            _body = BodyFactory.Instance.CreateCircleBody(physicsSimulator, Radius, 0.2f);
            _body.Position = position;


            Geom.OnCollision = HandleCollision;
            Geom.OnSeparation = HandleSeparation;

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
            if (input.IsNewKeyPress(Keys.LeftShift)) {
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

        private bool FlyingInAir {
            get {
                return (_collidingGeoms.Count == 0);
            }
        }


        private bool HandleCollision(Geom a, Geom b, ContactList contacts) {
            foreach (Geom g in new Geom[] { a, b }) {
                if (!Object.ReferenceEquals(g, this.Geom)) {
                    _collidingGeoms.Add(g);
                }
            }
            return true;
        }

        private void HandleSeparation(Geom a, Geom b) {
            foreach (Geom g in new Geom[] { a, b }) {
                if (!Object.ReferenceEquals(g, this.Geom)) {
                    if (!this.Geom.Collide(g)) {
                        _collidingGeoms.Remove(g);
                    }
                }
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

        public Geom Geom { 
            get {
                if (_geom == null) {
                    _geom = GeomFactory.Instance.CreateCircleGeom(_physicsSimulator, Body, Radius, 32);
                    _geom.RestitutionCoefficient = 0.0f;
                    _geom.FrictionCoefficient = 1.0f;
                    _geom.CollisionCategories = CollisionCategory.All;
                    _geom.CollidesWith = CollisionCategory.All;
                }
                return _geom; 
            } 
        }

        public Vector2Fs Position { get { return Body.Position; } }

        public float AngularVelocity {
            get { return Body.AngularVelocity; }
            set { Body.AngularVelocity = value; }
        }

        public Vector2Fs Velocity {
            get { return Body.LinearVelocity; }
        }

        public bool IsStatic { set { Body.IsStatic = value; } }

        public int CollisionGroup { set { Geom.CollisionGroup = value; } }

        public float RotationDeg {
            get { return (float) Body.Rotation.ToDegrees(); }
            set { Body.Rotation = value.ToRadians(); }
        }

        public float RotationRad { get { return Body.Rotation; } }

        public void Draw(Action<string, Vector2Fs /*pos*/, float /*rot*/, Vector2Fs /*origin*/> drawer) {
            drawer("Content/ukko.png", Body.Position, 0, _origin);
        }


    } // class

} // namespace
