using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SpaceDotNet.Components;
using SpaceDotNet.Objects;
using System;
using System.Diagnostics;

namespace SpaceDotNet {
    enum GameState {
        Running,
        Title,
        Over
    }

    public class SpaceDotNetGame : Game {
        public static SpaceDotNetGame Instance;
        public readonly Random Random = new Random();
        public readonly int Width = 1080;
        public readonly int Height = 800;
        public int Level { get; private set; } = 1;
        public GameTime GameTime => _gameTime;

        internal StarfieldComponent StarField { get; private set; }
        internal PlayerComponent Player { get; private set; }
        internal EnemiesComponent Enemies { get; private set; }

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

            Components.Add(StarField = new StarfieldComponent(this));
            Components.Add(Player = new PlayerComponent(this));
            Components.Add(Enemies = new EnemiesComponent(this));

            for (int i = 0; i < _explostions.Length; i++)
                _explostions[i] = new Sprite();

            for (int i = 0; i < _powerups.Length; i++)
                _powerups[i] = new Powerup();

            this.Deactivated += delegate {
                Pause(true);
            };
            this.Activated += delegate {
                Pause(false);
            };

            base.Initialize();
        }

        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _background = Content.Load<Texture2D>("backgrounds/bg");
            _defaultFont = Content.Load<SpriteFont>("fonts/default");
            _explosionTexture = Content.Load<Texture2D>("sprites/explosion");

            _powerUpsTextures = new Texture2D[] {
                Content.Load<Texture2D>("sprites/objects/Csharp1"),
                Content.Load<Texture2D>("sprites/objects/Csharp2"),
                Content.Load<Texture2D>("sprites/objects/dotnetcore"),
            };

            _music = Content.Load<Song>("audio/music");

            _bgPos = Random.Next(_background.Height);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = .5f;
            MediaPlayer.Play(_music);

            Audio.LoadContent(this);

            InitLevel(Level);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime) {
            _gameTime = gameTime;

            var keyState = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.Escape))
                Exit();
           
            if (_pauseCount > 0)
                return;

            if (gameTime.TotalGameTime - _lastTime > TimeSpan.FromMilliseconds(60)) {
                _bgPos++;
                if (_bgPos >= _background.Height)
                    _bgPos -= _background.Height;
                _lastTime = gameTime.TotalGameTime;
            }

            foreach (var exp in _explostions)
                exp.Update(gameTime);

            foreach (var pu in _powerups) {
                if (pu.State == SpriteState.Visible) {
                    pu.Update(gameTime);
                    if (Player.PlayerHit(pu)) {
                        ApplyPowerup(pu);
                    }
                    else if (pu.Position.Y > Window.ClientBounds.Bottom + 50) {
                        pu.State = SpriteState.Hidden;
                        _activePowerups--;
                    }
                }
            }
            base.Update(gameTime);
        }

        private void ApplyPowerup(Powerup pu) {
            pu.State = SpriteState.Hidden;
            _activePowerups--;
            Audio.PlayPowerup();
            Player.ApplyPowerup(pu.Type);
        }

        protected override void Draw(GameTime gameTime) {
            _spriteBatch.Begin();
            DrawBackground(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);

            _spriteBatch.Begin();
            foreach (var exp in _explostions)
                exp.Draw(_spriteBatch);

            foreach (var pu in _powerups)
                pu.Draw(_spriteBatch);

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

        public void NextLevel() {
            InitLevel(++Level);
        }

        void DrawStats(SpriteBatch sb) {
            sb.DrawString(_defaultFont, $"Level: {Level}", new Vector2(20, 20), Color.Cyan);
            sb.DrawString(_defaultFont, $"Score: {Player.Score}", new Vector2(120, 20), Color.Yellow);
            sb.DrawString(_defaultFont, $"Ships: {Player.Lives}", new Vector2(270, 20), Color.Magenta);
            if (_state == GameState.Over) {
                sb.DrawString(_defaultFont, "GAME OVER", new Vector2(210, 300), Color.Red, 0, new Vector2(0, 0), 5, SpriteEffects.None, 0);
                sb.DrawString(_defaultFont, "GAME OVER", new Vector2(216, 306), Color.LightBlue, 0, new Vector2(0, 0), 5, SpriteEffects.None, 0);
            }
        }

        public void Pause(bool pause) {
            _pauseCount += pause ? 1 : -1;
            if (_pauseCount > 0)
                MediaPlayer.Pause();
            else
                MediaPlayer.Resume();
        }

        void InitLevel(int level) {
            Debug.Assert(level > 0);
            _levelData = Levels.Data[Level - 1];
            Enemies.InitLevel(Level);
            StarField.InitLevel(Level);
        }

        public void GameOver() {
            Enemies.Visible = false;
            Player.Visible = false;
            _state = GameState.Over;
            MediaPlayer.Volume = .1f;
        }

        GraphicsDeviceManager _graphics;

        internal Powerup CreatePowerup(Sprite ship) {
            if (_activePowerups > 3)
                // not too many powerups at the same time
                return null;

            var type = Random.Next(3);
            int i = 0;
            for (; i < _powerups.Length; i++)
                if (_powerups[i].State == SpriteState.Hidden)
                    break;

            var pu = _powerups[i];
            pu.Init(_powerUpsTextures[type], (PowerupType)type, Random.NextFloat() * 2 - 1);
            pu.Position = ship.Position;
            pu.Velocity = new Vector2(0, Random.NextFloat() * 150 + 100);
            pu.State = SpriteState.Visible;
            pu.ScaleTo(50);
            _activePowerups++;
            return pu;
        }

        SpriteBatch _spriteBatch;
        Texture2D _background;
        Texture2D _explosionTexture;
        int _bgPos;
        TimeSpan _lastTime;
        Sprite[] _explostions = new Sprite[24];
        Texture2D[] _powerUpsTextures;
        Powerup[] _powerups = new Powerup[5];
        int _lastExplosion = -1;
        GameTime _gameTime;
        SpriteFont _defaultFont;
        LevelData _levelData;
        Song _music;
        GameState _state = GameState.Running;
        int _pauseCount = 0;
        int _activePowerups;
    }
}
