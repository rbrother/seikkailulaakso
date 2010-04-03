using System;
using System.Collections.Generic;
using FarseerGames.FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Net.Brotherus.SeikkailuLaakso;
using Vector2Fs = FarseerGames.FarseerPhysics.Mathematics.Vector2;
using Vector2Xna = Microsoft.Xna.Framework.Vector2;

namespace Net.Brotherus
{
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

            _graphics.PreferredBackBufferWidth = GameScreen.ScreenSize.X;
            _graphics.PreferredBackBufferHeight = GameScreen.ScreenSize.Y;
            _graphics.IsFullScreen = true;

            IsMouseVisible = true;

            _gameScreen = new GameScreen(this);
            Components.Add(_gameScreen);
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
            _gameScreen.GraphicsDevice.Clear(new Color(0,0,0));
            base.Draw(gameTime);
        }

    }
}