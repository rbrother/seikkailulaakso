using System;
using System.Collections.Generic;
using FarseerGames.FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Net.Brotherus.SeikkailuLaakso;

namespace Net.Brotherus
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SeikkailuLaaksoGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private GameScreen _gameScreen;

        public SeikkailuLaaksoGame()
        {
            Window.Title = "Seikkailulaakso";
            _graphics = new GraphicsDeviceManager(this);

            _graphics.SynchronizeWithVerticalRetrace = false;

            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 10);
            IsFixedTimeStep = true;

            SetFullScreen(false);

            IsMouseVisible = true;

            _gameScreen = new GameScreen(this);
            Components.Add(_gameScreen);
        }

        public void SetFullScreen(bool fullScreen)
        {
            if (fullScreen)
            {
                _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                _graphics.IsFullScreen = true;
            }
            else
            {
                _graphics.PreferredBackBufferWidth = 1600;
                _graphics.PreferredBackBufferHeight = 1024;
                _graphics.IsFullScreen = false;
            }
        }

        public void AddPolygonObstacle(string picFile, double x, double y)
        {
            _gameScreen.AddObstacle(picFile, new Vector2((float)x, (float)y)); 
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _graphics.ApplyChanges();
            base.Initialize();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _gameScreen.GraphicsDevice.Clear(new Color(200,200,200));
            base.Draw(gameTime);
        }

    }
}