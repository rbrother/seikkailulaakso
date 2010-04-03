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
using Vector2Fs = FarseerGames.FarseerPhysics.Mathematics.Vector2;
using Vector2Xna = Microsoft.Xna.Framework.Vector2;

namespace Net.Brotherus
{
    public class GameScreen : DrawableGameComponent
    {
        private float _scrollPosition = 0.0f;
        private float _scrollSpeed = 0.0f;


        public static Point ScreenSize = new Point(1600, 1200);
        private static readonly float SCROLL_MARGIN = 400.0f;

        private static readonly Vector2Fs GRAVITY = new Vector2Fs(0, 1000);
        private PhysicsSimulator _physicsSimulator;
        private ContentManager _contentManager;
        private SpriteBatch _spriteBatch;
        private InputState _input = new InputState();

        private UkkeliSimple _ukkeli;
        private List<PolygonObstacle> _obstacles;

        private LineBrush _lineBrush = new LineBrush(1, Color.Black); //used to draw spring on mouse grab

        private Texture2D _background;


        private float TILE_SIZE = 60.0f;
        private Dictionary<string, Texture2D> textures;

        public GameScreen(Game game) : base(game)
        {
            _contentManager = new ContentManager(game.Services);
            _physicsSimulator = new PhysicsSimulator(GRAVITY);
            _obstacles = new List<PolygonObstacle>();
            this.textures = new Dictionary<string, Texture2D>();
        }

        private Texture2D GetTexture(string fileName) {
            if (!this.textures.ContainsKey(fileName)) {
                this.textures[fileName] = Texture2D.FromFile(GraphicsDevice, fileName);
            }
            return this.textures[fileName];
        }

        private Texture2D SquareTexture { get { return GetTexture("Tiles/tile-60.png"); } }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load content belonging to the screen manager.
            _background = Texture2D.FromFile(GraphicsDevice, "Content/taustakuvat/berkin_talo.png");
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _lineBrush.Load(GraphicsDevice);
            _ukkeli = new UkkeliSimple(new Vector2Fs(150, 800), GraphicsDevice, _physicsSimulator);
            AddGound();
        }

        private void AddGound() {
            float x = 0.0f;
            float y = ScreenSize.Y - TILE_SIZE;
            while (x < 2000.0f) {
                AddObstacle(SquareTexture, new Vector2Fs(x, y));
                x += TILE_SIZE;
            }
        }

        public void AddObstacle(Texture2D texture, Vector2Fs position) {
            _obstacles.Add(new PolygonObstacle(position, texture, GraphicsDevice, _physicsSimulator));
        }


        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent() {
            _contentManager.Unload();
            _physicsSimulator.Clear();
        }

        public override void Update(GameTime gameTime) {
            // Read the keyboard and gamepad.
            _input.Update();
            float secondsElapsed = gameTime.ElapsedGameTime.Milliseconds * .001f;
            _physicsSimulator.Update(secondsElapsed);
            _ukkeli.HandleKeyboardInput(_input, gameTime);
            Scroll(secondsElapsed);
        }

        private void Scroll(float secondsElapsed) {
            if (ManOnScreenX > ScreenSize.X - SCROLL_MARGIN) {
                _scrollSpeed += secondsElapsed;
            } else if (ManOnScreenX < SCROLL_MARGIN) {
                _scrollSpeed -= secondsElapsed;
            } else if (Math.Abs(_scrollSpeed) < 0.1f ) {
                _scrollSpeed = 0.0f;
            } else {
                _scrollSpeed -= Math.Sign(_scrollSpeed) * secondsElapsed;
            }
            _scrollSpeed = _scrollSpeed.InLimits(-100.0f, 100.0f);
            _scrollPosition += secondsElapsed * _scrollSpeed * 500.0f;
        }

        /// <summary>
        /// This is called when the screen should draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime) {
            _spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            Action<Texture2D, Vector2Fs /*pos*/, float /*rot*/, Vector2Fs /*origin*/> drawer = (tex, pos, rot, orig) => {
                _spriteBatch.Draw(tex, pos.ToVector2Xna(-_scrollPosition), null, Color.White, rot, orig.ToVector2Xna(), 1, SpriteEffects.None, 0);
            };

            // Background is made always only of three tiles so we must alter which ones
            int bgIndex = Convert.ToInt32( Math.Floor(_scrollPosition / ScreenSize.X) );
            for (int i = bgIndex - 1; i <= bgIndex + 1; ++i) {
                var flip = i % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                _spriteBatch.Draw(_background, new Rectangle((int)-_scrollPosition + ScreenSize.X * i, 0, ScreenSize.X, ScreenSize.Y), null, Color.White, 0, Vector2Xna.Zero, flip, 0.0f);
            }


            foreach (var obstacle in _obstacles) obstacle.Draw(drawer);
            _ukkeli.Draw(drawer);            
            _spriteBatch.End();
        }

        private float ManOnScreenX {
            get { return _ukkeli.Position.X - _scrollPosition; }
        }
    }
}