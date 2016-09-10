using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Net.Brotherus.SeikkailuLaakso;
using Microsoft.SDK.Samples.VistaBridge.Library;


namespace Net.Brotherus
{
    public class GameScreen : DrawableGameComponent
    {
        private static GraphicsDevice _sharedGraphicsDevice = null;

        private Vector2 _scrollPosition = Vector2.Zero;
        private float _scrollSpeed = 0.0f;
        private float _secondsElapsed;

        public static Point ScreenSize = new Point(1600, 1200);
        private static readonly float SCROLL_MARGIN = 400.0f;

        private static readonly Vector2 GRAVITY = new Vector2(0, 1000);
        private World _world;
        private ContentManager _contentManager;
        private SpriteBatch _spriteBatch;
        private InputState _input = new InputState();

        private UkkeliSimple _ukkeli;
        private List<PolygonObstacle> _obstacles, _menuObstacles, _colorObstacles;
        private PolygonObstacle _draggedObstacle, _selectedMenuObstacle, _selectedColorObstacle;
        private Texture2D _background;

        private SpriteFonts _spriteFonts;

        private Vector2 _menuPosition = new Vector2(150,150);
        
        private static readonly float TILE_SIZE = 60.0f;

        private static readonly Vector2 UKKELI_START_POS = new Vector2(150, 800);

        private int _infoLine = 0;
        private string _fileName = null;

        private float _flashFraction = 0.0f;

        public GameScreen(Game game) : base(game)
        {
            _contentManager = new ContentManager(game.Services);
            _world = new World(GRAVITY);
            _obstacles = new List<PolygonObstacle>();
            _menuObstacles = new List<PolygonObstacle>();
            _colorObstacles = new List<PolygonObstacle>();
        }

        private static Vector2 CoarsePosition(Vector2 pos) {
            return new Vector2(Quantize(pos.X), Quantize(pos.Y));
        }

        private static float Quantize(float val) {
            return Convert.ToInt32( Math.Floor(val / TILE_SIZE) * TILE_SIZE );
        }        

        public static GraphicsDevice SharedGraphicsDevice {
            get {
                if (_sharedGraphicsDevice == null) throw new ApplicationException("GraphicsDevice not set");
                return _sharedGraphicsDevice;
            }
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            _sharedGraphicsDevice = GraphicsDevice;
            // Load content belonging to the screen manager.
            _background = AssetCreator.Instance.TextureFromFile("Content/taustakuvat/berkin_talo.png");
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _ukkeli = new UkkeliSimple(UKKELI_START_POS, TILE_SIZE, GraphicsDevice, _world);
            _spriteFonts = new SpriteFonts(_contentManager);
            AssetCreator.Instance.LoadContent(_contentManager);

            AddGound();
            AddMenuObstacles();
        }

        private void AddMenuObstacles() {
            Vector2 pos = _menuPosition + new Vector2(20, 20);
            for (int width = 1; width <= 2; width++) {
                foreach (int rotate in new int[] { 0, 90, 180, 270 }) {
                    foreach (bool flip in new bool[] { false, true }) {
                        var obstacle = new PolygonObstacle(pos, width, rotate, flip, _world, false);
                        _menuObstacles.Add( obstacle );
                        pos = pos + new Vector2(obstacle.Width + 10, 0);
                    }
                }
            }
            pos = _menuPosition + new Vector2(20, 150);
            for (int red = 0; red <= 255; red += 127) {
                for (int green = 0; green <= 255; green += 127) {
                    for (int blue = 0; blue <= 255; blue += 127) {
                        _colorObstacles.Add(new PolygonObstacle(pos, 1, 0, false, _world, new Color(red, green, blue), false));
                        pos = pos + new Vector2(40, 0);
                    }
                }
            }
        }

        private void AddGound() {
            float x = 0.0f;
            float y = ScreenSize.Y - TILE_SIZE;
            while (x < 2000.0f) {
                AddObstacle( 1, 0, false, new Vector2(x, y));
                x += TILE_SIZE;
            }
        }

        public void AddObstacle(int width, int rotate, bool flip, Vector2 position) {
            _obstacles.Add(new PolygonObstacle(position, width, rotate, flip, _world, true));
        }

        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent() {
            _contentManager.Unload();
            _world.Clear();
        }

        public override void Update(GameTime gameTime) {
            // Read the keyboard and gamepad.
            _input.Update();
            _secondsElapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _world.Step(_secondsElapsed);
            _ukkeli.HandleKeyboardInput(_input, gameTime);
            HandleKeyboardInput(_input);
            HandleMouseInput(_input);
            Scroll();
            _flashFraction += _secondsElapsed * 2.0f;
            if (_flashFraction > 1.0f) _flashFraction = 0.0f;
            if (_ukkeli.Position.Y > 2000.0f) Die();
        }

        private void Die() {
            _ukkeli.Body.Position = UKKELI_START_POS;
            _scrollPosition = Vector2.Zero;
            _scrollSpeed = 0.0f;
        }

        private void HandleKeyboardInput(InputState input) {
            if (input.IsNewKeyPress(Keys.S)) {
                SaveTrack();
            } else if (input.IsNewKeyPress(Keys.L)) {
                var openDialog = new CommonOpenFileDialog("Load Track");
                openDialog.Title = "Load Track";
                openDialog.InitialDirectory = TrackDir;
                openDialog.Filters.Add(new CommonFileDialogFilter("Track XML Files", "xml"));
                var result = openDialog.ShowDialog();
                if (!result.Canceled && openDialog.FileNames.Count > 0) {
                    LoadTrack(openDialog.FileNames.First());
                }
            } 
        }

        private void HandleMouseInput(InputState input) {
            // Press left button down: start drag
            if (input.LastMouseState.LeftButton == ButtonState.Released &&
                input.CurrentMouseState.LeftButton == ButtonState.Pressed) {                
                if (input.LastKeyboardState.IsKeyDown(Keys.LeftShift) && _selectedMenuObstacle != null && _selectedColorObstacle != null) {
                    _obstacles.Add(_selectedMenuObstacle.Clone(CoarsePosition(MouseWorldPos), _selectedColorObstacle.Color, _world));
                } else {
                    if (ObstacleAt(MouseWorldPos, _obstacles) != null) {
                        _draggedObstacle = ObstacleAt(MouseWorldPos, _obstacles);
                    } else if (ObstacleAt(MouseScreenPos, _menuObstacles) != null) {
                        _selectedMenuObstacle = ObstacleAt(MouseScreenPos, _menuObstacles);
                    } else if (ObstacleAt(MouseScreenPos, _colorObstacles) != null) {
                        _selectedColorObstacle = ObstacleAt(MouseScreenPos, _colorObstacles);
                    }
                }
            }
            // Lift left button up: end drag
            else if (_input.LastMouseState.LeftButton == ButtonState.Pressed &&
                     _input.CurrentMouseState.LeftButton == ButtonState.Released) {
                _draggedObstacle = null;
            } else if (_input.LastMouseState.RightButton == ButtonState.Released &&
                     _input.CurrentMouseState.RightButton == ButtonState.Pressed) {
                RemoveObstacle(ObstacleAt(MouseWorldPos, _obstacles));
            }
            // Move picked obstacle
            if (_draggedObstacle != null) {
                _draggedObstacle.Position = CoarsePosition(MouseWorldPos);
            }
        }

        private PolygonObstacle ObstacleAt(Vector2 pos, List<PolygonObstacle> obstacles) {
            return null;
        }

        private void SaveTrack() {
            if (_fileName == null) {
                var saveDialog = new CommonOpenFileDialog("Save Track");
                saveDialog.Title = "Save Track";
                saveDialog.InitialDirectory = TrackDir;
                saveDialog.Filters.Add(new CommonFileDialogFilter("Track XML Files", "xml"));
                var result = saveDialog.ShowDialog();
                if (result.Canceled || saveDialog.FileNames.Count == 0) return;
                _fileName = saveDialog.FileNames.First();
                if (!_fileName.EndsWith(".xml")) _fileName += ".xml";
            }
            TrackXml.Save(_fileName);
        }

        private void LoadTrack(string FileName) {
            _fileName = FileName;
            // Remove existing obstacles from physics sim
            foreach (var obstacle in _obstacles) {                
                _world.RemoveBody(obstacle.Body);
            }

            _obstacles = XElement.Load(FileName).Elements("Obstacle")
                .Select(obsXml => new PolygonObstacle(obsXml, _world))
                .ToList();
        }

        private string TrackDir {
            get {
                return AssemblyDir.GetDirectories("Radat").First().FullName;
            }
        }

        private DirectoryInfo AssemblyDir {
            get {
                return new FileInfo(new Uri(this.GetType().Assembly.CodeBase).LocalPath).Directory;
            }
        }

        private XElement TrackXml {
            get {
                return new XElement("Rata", _obstacles.Select(obs => obs.Xml));
            }
        }

        private void RemoveObstacle(PolygonObstacle obstacle) {
            if (obstacle == null) return;
            _world.RemoveBody(obstacle.Body);
            _obstacles.Remove(obstacle);
        }

        private Vector2 MouseWorldPos {
            get { return new Vector2(_input.CurrentMouseState.X + _scrollPosition.X, _input.CurrentMouseState.Y); }
        }

        private Vector2 MouseScreenPos {
            get { return new Vector2(_input.CurrentMouseState.X, _input.CurrentMouseState.Y); }
        }

        private void Scroll() {
            if (ManOnScreenX > ScreenSize.X - SCROLL_MARGIN) {
                _scrollSpeed += _secondsElapsed;
            } else if (ManOnScreenX < SCROLL_MARGIN) {
                _scrollSpeed -= _secondsElapsed;
            } else if (Math.Abs(_scrollSpeed) < 0.1f ) {
                _scrollSpeed = 0.0f;
            } else {
                _scrollSpeed -= Math.Sign(_scrollSpeed) * _secondsElapsed;
            }
            _scrollSpeed = _scrollSpeed.InLimits(-100.0f, 100.0f);
            _scrollPosition = new Vector2( _scrollPosition.X + _secondsElapsed * _scrollSpeed * 500.0f, _scrollPosition.Y);
        }

        /// <summary>
        /// This is called when the screen should draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime) {
            _infoLine = 0;
            _spriteBatch.Begin();

            // Background is made always only of three tiles so we must alter which ones
            int bgIndex = Convert.ToInt32( Math.Floor(_scrollPosition.X / ScreenSize.X) );
            for (int i = bgIndex - 1; i <= bgIndex + 1; ++i) {
                var flip = i % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                _spriteBatch.Draw(_background, new Rectangle((int)-_scrollPosition.X + ScreenSize.X * i, 0, ScreenSize.X, ScreenSize.Y), null, Color.White, 0, Vector2.Zero, flip, 0.0f);
            }

            //_spriteBatch.Draw(_textureCache.RectangleTile(24, 4, 0, System.Drawing.Color.White), _menuPosition, Color.White);



            foreach (var obstacle in AllObstacles) obstacle.Draw(Drawer(obstacle.Scroll, Color.White));
            foreach (var obstacle in SelectedObstacles) obstacle.Draw(Drawer(obstacle.Scroll, new Color(_flashFraction, _flashFraction, _flashFraction, 1.0f)));
            
            if (_fileName != null) DrawTitle( Path.GetFileName(_fileName) );
            DrawInfoLines();
            _ukkeli.Draw( Drawer(true, Color.White) );            
            _spriteBatch.End();
        }

        private Action<Texture2D, Vector2 /*pos*/, float /*rot*/, Vector2 /*origin*/> Drawer(bool scroll, Color color) {
            return (tex, pos, rot, orig) => {
                _spriteBatch.Draw(tex, scroll ? pos - _scrollPosition : pos, null, color, rot, orig, 1, SpriteEffects.None, 0);
            };
        }

        private IEnumerable<PolygonObstacle> AllObstacles {
            get {
                return _obstacles.Concat(_menuObstacles).Concat(_colorObstacles);
            }
        }

        private IEnumerable<PolygonObstacle> SelectedObstacles {
            get { return (new PolygonObstacle[] { _draggedObstacle, _selectedMenuObstacle, _selectedColorObstacle }).OfType<PolygonObstacle>(); }
        }

        private void DrawTitle(string text) {
            float x = ScreenSize.X / 2 - _spriteFonts.BardTitle.MeasureString(text).X / 2;
            DrawShadowString(text, new Vector2(x, 10), _spriteFonts.BardTitle);
        }

        private void DrawInfoLines() {
            DrawInfoLine("Ukkeli X = {0:0.0}", _ukkeli.Position.X);
            DrawInfoLine("Ukkeli Y = {0:0.0}", _ukkeli.Position.Y);
            DrawInfoLine("Ukkeli Rotation = {0:0.0}", _ukkeli.Rotation);
            DrawInfoLine("Ukkeli FlyingInAir: {0}", _ukkeli.FlyingInAir);
            DrawInfoLine("FPS = {0:0.0}", 1.0f / _secondsElapsed);
            DrawInfoLine("Shift + Click: Add new piece");
            DrawInfoLine("Drag: Move a piece");
            DrawInfoLine("Right-click: Remove piece");
        }

        private void DrawInfoLine(string text, params object[] pars) {
            DrawShadowString( string.Format( text, pars ), new Vector2(10, 10 + 20 * _infoLine), _spriteFonts.SmallFont);
            _infoLine++;
        }

        private void DrawShadowString(string text, Vector2 location, SpriteFont font) {            
            _spriteBatch.DrawString(font, text, location + new Vector2(2,2), Color.Black);
            _spriteBatch.DrawString(font, text, location, Color.White);
        }

        private float ManOnScreenX {
            get { return _ukkeli.Position.X - _scrollPosition.X; }
        }
    }
}