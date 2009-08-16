using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FarseerGames.FarseerPhysics;
using Microsoft.Xna.Framework.Input;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.GettingStarted.DrawingSystem;
using Net.Brotherus.SeikkailuLaakso;

namespace Net.Brotherus
{
    public class GameScreen : DrawableGameComponent
    {
        private static readonly Vector2 GRAVITY = new Vector2(0, 1000);
        private PhysicsSimulator _physicsSimulator;
        private ContentManager _contentManager;
        private SpriteBatch _spriteBatch;
        private InputState _input = new InputState();

        private Ukkeli _ukkeli;
        private List<PolygonObstacle> _obstacles;

        private LineBrush _lineBrush = new LineBrush(1, Color.Black); //used to draw spring on mouse grab
        private FixedLinearSpring _mousePickSpring;
        private Geom _pickedGeom;

        public GameScreen(Game game) : base(game)
        {
            _contentManager = new ContentManager(game.Services);
            _physicsSimulator = new PhysicsSimulator(GRAVITY);
            _obstacles = new List<PolygonObstacle>();
        }

        public void AddObstacle(string picFile, Vector2 position)
        {
             _obstacles.Add(new PolygonObstacle(position, picFile));
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load content belonging to the screen manager.
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _lineBrush.Load(GraphicsDevice);
            _ukkeli = new Ukkeli(new Vector2(150, 800), GraphicsDevice, _physicsSimulator);
            foreach (var obstacle in _obstacles)
            {                
                obstacle.Load(GraphicsDevice, _physicsSimulator);
            }
        }

        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            _contentManager.Unload();
            _physicsSimulator.Clear();
        }

        public override void Update(GameTime gameTime)
        {
            // Read the keyboard and gamepad.
            _input.Update();
            float secondsElapsed = gameTime.ElapsedGameTime.Milliseconds * .001f;
            _physicsSimulator.Update(secondsElapsed);
            _ukkeli.HandleKeyboardInput(_input, gameTime);
            HandleMouseInput(_input);
        }

        private void HandleMouseInput(InputState input)
        {
            Vector2 point = new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y);
            if (input.LastMouseState.LeftButton == ButtonState.Released &&
                input.CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                CreateMouseSpring(point);
            }
            else if (input.LastMouseState.LeftButton == ButtonState.Pressed &&
                     input.CurrentMouseState.LeftButton == ButtonState.Released)
            {
                DestroyMouseSpring();
            }
            //move anchor point
            if (input.CurrentMouseState.LeftButton == ButtonState.Pressed && _mousePickSpring != null)
            {
                _mousePickSpring.WorldAttachPoint = point;
            }
        }

        private void DestroyMouseSpring()
        {
            if (_mousePickSpring != null && _mousePickSpring.IsDisposed == false)
            {
                _mousePickSpring.Dispose();
                _mousePickSpring = null;
            }
        }

        private void CreateMouseSpring(Vector2 point)
        {
            _pickedGeom = _physicsSimulator.Collide(point);
            if (_pickedGeom != null)
            {
                _mousePickSpring = SpringFactory.Instance.CreateFixedLinearSpring(_physicsSimulator,
                    _pickedGeom.Body, _pickedGeom.Body.GetLocalPosition(point), point, 20, 10);
            }
        }

        /// <summary>
        /// This is called when the screen should draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            foreach (var obstacle in _obstacles) obstacle.Draw(_spriteBatch);
            _ukkeli.Draw(_spriteBatch);
            if (_mousePickSpring != null)
            {
                _lineBrush.Draw(_spriteBatch,
                                _mousePickSpring.Body.GetWorldPosition(_mousePickSpring.BodyAttachPoint),
                                _mousePickSpring.WorldAttachPoint);
            }
            _spriteBatch.End();
        }

    }
}