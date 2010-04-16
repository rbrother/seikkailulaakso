using System;
using System.Linq;
using System.Xml.Linq;
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
    public interface ITextureCache {
        Texture2D GetTexture(string fileName);
    }

    public class GameScreen : DrawableGameComponent, ITextureCache
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

        private Texture2D _background;

        private PolygonObstacle _draggedObstacle;

        private SpriteFonts _spriteFonts;
        

        private static readonly float TILE_SIZE = 60.0f;
        private Dictionary<string, Texture2D> textures;

        public GameScreen(Game game) : base(game)
        {
            _contentManager = new ContentManager(game.Services);
            _physicsSimulator = new PhysicsSimulator(GRAVITY);
            _obstacles = new List<PolygonObstacle>();
            this.textures = new Dictionary<string, Texture2D>();
        }

        public Texture2D GetTexture(string fileName) {
            if (!this.textures.ContainsKey(fileName)) {
                this.textures[fileName] = Texture2D.FromFile(GraphicsDevice, fileName);
            }
            return this.textures[fileName];
        }

        private static Vector2Fs CoarsePosition(Vector2Fs pos) {
            return new Vector2Fs(Quantize(pos.X), Quantize(pos.Y));
        }

        private static float Quantize(float val) {
            return Convert.ToInt32( Math.Floor(val / TILE_SIZE) * TILE_SIZE );
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load content belonging to the screen manager.
            _background = GetTexture( "Content/taustakuvat/berkin_talo.png" );
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _ukkeli = new UkkeliSimple(new Vector2Fs(150, 800), TILE_SIZE, GraphicsDevice, _physicsSimulator);
            _spriteFonts = new SpriteFonts(_contentManager);

            if (System.IO.File.Exists(TrackFileName)) {
                LoadTrack();
            } else {
                AddGound();
            }
        }

        private void AddGound() {
            float x = 0.0f;
            float y = ScreenSize.Y - TILE_SIZE;
            while (x < 2000.0f) {
                AddObstacle("tile-60", new Vector2Fs(x, y));
                x += TILE_SIZE;
            }
        }

        public void AddObstacle(string textureName, Vector2Fs position) {
            _obstacles.Add(new PolygonObstacle(position, "Tiles/" + textureName + ".png", _physicsSimulator, this));
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
            HandleKeyboardInput(_input);
            HandleMouseInput(_input);
            Scroll(secondsElapsed);
        }

        private Dictionary<Keys, string> ObstacleKeys {
            get {
                var d = new Dictionary<Keys, string>();
                d.Add(Keys.D1, "tile-60");
                d.Add(Keys.F1, "tile-60-1-0");
                d.Add(Keys.F2, "tile-60-1-1");
                d.Add(Keys.F3, "tile-60-1-2");
                d.Add(Keys.F4, "tile-60-1-3");
                d.Add(Keys.F5, "tile-60-2-0");
                d.Add(Keys.F6, "tile-60-2-1");
                d.Add(Keys.F7, "tile-60-2-2");
                d.Add(Keys.F8, "tile-60-2-3");
                d.Add(Keys.F9, "tile-60-2-4");
                d.Add(Keys.F10, "tile-60-2-5");
                d.Add(Keys.F11, "tile-60-2-6");
                d.Add(Keys.F12, "tile-60-2-7");
                return d;
            }
        }

        private void HandleKeyboardInput(InputState input) {
            foreach (Keys k in ObstacleKeys.Keys) {
                if (input.IsNewKeyPress(k)) {
                    AddObstacle(ObstacleKeys[k], CoarsePosition(MouseWorldPos));
                    return;
                }
            }
            if (input.IsNewKeyPress(Keys.S)) {
                SaveTrack();
            } else if (input.IsNewKeyPress(Keys.L)) {
                LoadTrack();
            }
        }

        private void SaveTrack() {
            TrackXml.Save(TrackFileName);
        }

        private void LoadTrack() {
            // Remove existing obstacles from physics sim
            foreach (var obstacle in _obstacles) {
                _physicsSimulator.Remove(obstacle.Geom);
                _physicsSimulator.Remove(obstacle.Body);
            }
            _obstacles = XElement.Load(TrackFileName).Elements("Obstacle")
                .Select(obsXml => new PolygonObstacle(obsXml, _physicsSimulator, this))
                .ToList();
        }

        private XElement TrackXml {
            get {
                return new XElement("Rata", _obstacles.Select( obs => obs.Xml ) );
            }
        }

        private string TrackFileName {
            get { return @"Radat\Rata1.xml"; }
        }

        private void HandleMouseInput(InputState input) {
            // Press left button down: start drag
            if (input.LastMouseState.LeftButton == ButtonState.Released &&
                input.CurrentMouseState.LeftButton == ButtonState.Pressed) {
                _draggedObstacle = FindObstacleAtMouse;
            }
            // Lift fet button up: end drag
            else if (_input.LastMouseState.LeftButton == ButtonState.Pressed &&
                     _input.CurrentMouseState.LeftButton == ButtonState.Released) {
                _draggedObstacle = null;
            } else if (_input.LastMouseState.RightButton == ButtonState.Released &&
                     _input.CurrentMouseState.RightButton == ButtonState.Pressed) {
                _obstacles.Remove(FindObstacleAtMouse);
            }
            // Move picked obstacle
            if (_draggedObstacle != null) {
                _draggedObstacle.Position = CoarsePosition(MouseWorldPos);
            }
        }

        private PolygonObstacle FindObstacleAtMouse {
            get {
                var pickedGeom = _physicsSimulator.Collide(MouseWorldPos);
                if (pickedGeom != null) {
                    return _obstacles.Find(obs => obs.Body == pickedGeom.Body);
                }
                return null;
            }
        }

        private Vector2Fs MouseWorldPos {
            get { return new Vector2Fs(_input.CurrentMouseState.X + _scrollPosition, _input.CurrentMouseState.Y); }
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

            Action<string /*texturename*/, Vector2Fs /*pos*/, float /*rot*/, Vector2Fs /*origin*/> drawer = (tex, pos, rot, orig) => {
                _spriteBatch.Draw(GetTexture(tex), pos.ToVector2Xna(-_scrollPosition), null, Color.White, rot, orig.ToVector2Xna(), 1, SpriteEffects.None, 0);
            };
            Action<string /*texturename*/, Vector2Fs /*pos*/, float /*rot*/, Vector2Fs /*origin*/> selectedDrawer = (tex, pos, rot, orig) => {
                _spriteBatch.Draw(GetTexture(tex), pos.ToVector2Xna(-_scrollPosition), null, Color.Gray, rot, orig.ToVector2Xna(), 1, SpriteEffects.None, 0);
            };

            // Background is made always only of three tiles so we must alter which ones
            int bgIndex = Convert.ToInt32( Math.Floor(_scrollPosition / ScreenSize.X) );
            for (int i = bgIndex - 1; i <= bgIndex + 1; ++i) {
                var flip = i % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                _spriteBatch.Draw(_background, new Rectangle((int)-_scrollPosition + ScreenSize.X * i, 0, ScreenSize.X, ScreenSize.Y), null, Color.White, 0, Vector2Xna.Zero, flip, 0.0f);
            }

            foreach (var obstacle in _obstacles) {
                if (object.ReferenceEquals(obstacle, _draggedObstacle)) {
                    obstacle.Draw(selectedDrawer);
                } else {
                    obstacle.Draw(drawer);
                }
            }
            DrawTitle("Berkin Talo");
            _ukkeli.Draw(drawer);            
            _spriteBatch.End();
        }

        private void DrawTitle(string text) {
            float x = ScreenSize.X / 2 - _spriteFonts.BardTitle.MeasureString(text).X / 2;
            DrawShadowString(text, new Vector2(x, 10), _spriteFonts.BardTitle);
        }

        private void DrawShadowString(string text, Vector2 location, SpriteFont font) {            
            _spriteBatch.DrawString(font, text, location + new Vector2(2,2), Color.Black);
            _spriteBatch.DrawString(font, text, location, Color.White);
        }

        private float ManOnScreenX {
            get { return _ukkeli.Position.X - _scrollPosition; }
        }
    }
}