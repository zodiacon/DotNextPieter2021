using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SpaceDotNet.Components;
using System;
using System.Diagnostics;

namespace SpaceDotNet {
    public class SpaceDotNetGame : Game {
        public static SpaceDotNetGame Instance;
        public readonly Random Random = new Random();
        public readonly int Width = 1080;
        public readonly int Height = 800;
        public int Level { get; private set; } = 4;
        public GameTime GameTime => _gameTime;

        internal Starfield StarField { get; private set; }
        internal Player Player { get; private set; }
        internal Enemies Enemies { get; private set; }

        public SpaceDotNetGame() {
            Debug.Assert(Instance == null);

            Instance = this;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            //TargetElapsedTime = TimeSpan.FromMilliseconds(20);
        }

        protected override void Initialize() {
            _graphics.PreferredBackBufferWidth = Width;
            _graphics.PreferredBackBufferHeight = Height;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();
            Window.Title = "Space Dot Net";

            Components.Add(StarField = new Starfield(this));
            Components.Add(Player = new Player(this));
            Components.Add(Enemies = new Enemies(this));

            for (int i = 0; i < _explostions.Length; i++)
                _explostions[i] = new Sprite();

            base.Initialize();
        }

        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _background = Content.Load<Texture2D>("backgrounds/bg");
            _defaultFont = Content.Load<SpriteFont>("fonts/default");
            _explosionTexture = Content.Load<Texture2D>("sprites/explosion");
            _music = Content.Load<Song>("audio/music");

            _bgPos = Random.Next(_background.Height);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = .5f;
            MediaPlayer.Play(_music);

            Enemies.InitLevel(Level);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime) {
            _gameTime = gameTime;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (gameTime.TotalGameTime - _lastTime > TimeSpan.FromMilliseconds(60)) {
                _bgPos++;
                if (_bgPos >= _background.Height)
                    _bgPos -= _background.Height;
                _lastTime = gameTime.TotalGameTime;
            }

            foreach (var exp in _explostions)
                exp.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            _spriteBatch.Begin();
            DrawBackground(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);

            _spriteBatch.Begin();
            foreach (var exp in _explostions)
                exp.Draw(_spriteBatch);
            DrawStats(_spriteBatch);
            _spriteBatch.End();

        }

        private void DrawBackground(SpriteBatch sb) {
            if (_bgPos >= 0 && _bgPos < Height) {
                sb.Draw(_background, new Vector2(0, _bgPos), new Rectangle(0, 0, Width, Height - _bgPos), Color.DarkGray);
            }
            if (_bgPos > 0) {
                sb.Draw(_background, new Vector2(0, 0), new Rectangle(0, _background.Height - _bgPos, Width, _bgPos), Color.DarkGray);
            }
        }

        public Sprite GetFreeExplostion() {
            for (int i = 0; i < _explostions.Length; i++) {
                _lastExplosion = (_lastExplosion + 1) % _explostions.Length;
                if (_explostions[_lastExplosion].State == SpriteState.Hidden) {
                    _explostions[_lastExplosion].InitTexture(_explosionTexture, 7);
                    return _explostions[_lastExplosion];
                }
            }
            Debug.WriteLine("out of explosion objects!");
            return null;
        }

        void DrawStats(SpriteBatch sb) {
            sb.DrawString(_defaultFont, $"Level: {Level}", new Vector2(20, 20), Color.Cyan);
            sb.DrawString(_defaultFont, $"Score: {Player.Score}", new Vector2(100, 20), Color.Yellow);
            sb.DrawString(_defaultFont, $"Ships: {Player.Lives}", new Vector2(250, 20), Color.Magenta);
        }

        void InitLevel(int level) {
            Debug.Assert(level > 0);

        }

        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        Texture2D _background;
        Texture2D _explosionTexture;
        int _bgPos;
        TimeSpan _lastTime;
        Sprite[] _explostions = new Sprite[16];
        int _lastExplosion = -1;
        GameTime _gameTime;
        SpriteFont _defaultFont;
        Song _music;
    }
}
