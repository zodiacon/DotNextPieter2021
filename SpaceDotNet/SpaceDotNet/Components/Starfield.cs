using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDotNet.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceDotNet.Components {
    sealed class Starfield : DrawableGameComponent {
        const int Stars = 100;

        class Star {
            public Vector2 Position;
            public float Speed;
            public Color Tint;
        }

        public Starfield(Game game) : base(game) {
        }

        public override void Initialize() {
            base.Initialize();

            _player = Game.Components.First(c => c.GetType() == typeof(Player)) as Player;
        }

        protected override void LoadContent() {
            for (int i = 1; i <= 3; i++) {
                var texture = Game.Content.Load<Texture2D>($"sprites/asteroid{i}");
                _asteroidTextures.Add(texture);
            }
            _pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            _width = Game.GraphicsDevice.PresentationParameters.BackBufferWidth;
            _height = Game.GraphicsDevice.PresentationParameters.BackBufferHeight;

            for (int i = 0; i < Stars; i++) {
                var grey = _rnd.Next(200) + 20;
                var star = new Star {
                    Position = new Vector2(_rnd.NextFloat() * _width, _rnd.NextFloat() * _height * 2 - _height),
                    Speed = _rnd.NextFloat() * 3 + .2f,
                    Tint = new Color(grey, grey, grey)
                };
                _stars.Add(star);
            }

            _sb = new SpriteBatch(Game.GraphicsDevice);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime) {
            _sb.Begin();
            foreach (var star in _stars)
                _sb.Draw(_pixel, star.Position, star.Tint);

            foreach (var ast in _asteroids)
                ast.Draw(_sb);

            _sb.End();

        }

        public override void Update(GameTime gameTime) {
            foreach (var star in _stars) {
                star.Position.Y += star.Speed;
                if (star.Position.Y > _height + 10) {
                    star.Position = new Vector2(_rnd.NextFloat() * _width, _rnd.NextFloat() * 300 - 500);
                    var grey = _rnd.Next(200) + 20;
                    star.Speed = _rnd.NextFloat() * 3 + .2f;
                    star.Tint = new Color(grey, grey, grey);
                }

            }

            if (_player.State == PlayerState.Alive) {
                if (_asteroids.Count < _maxAsteroids && _rnd.Next(100) < 2) {
                    var ast = new Asteroid(_asteroidTextures[_rnd.Next(_asteroidTextures.Count)]);
                    _asteroids.Add(ast);
                }
            }

            for (int i = 0; i < _asteroids.Count; i++) {
                _asteroids[i].Update(gameTime);
                if (_asteroids[i].Position.Y > _height + 100) {
                    _asteroids.RemoveAt(i);
                    i--;
                }
            }
        }

        internal void InitLevel(int level) {
            _maxAsteroids = Levels.Data[level - 1].MaxAsteroids;
        }

        internal void RemoveAsteroid(Asteroid ast) {
            _asteroids.Remove(ast);
            ast.State = SpriteState.Hidden;
        }

        public ICollection<Asteroid> Asteroids => _asteroids; 

        List<Texture2D> _asteroidTextures = new List<Texture2D>();
        Texture2D _pixel;
        List<Star> _stars = new List<Star>(100);
        Random _rnd = new Random();
        SpriteBatch _sb;
        List<Asteroid> _asteroids = new List<Asteroid>(8);

        int _width, _height;
        Player _player;
        int _maxAsteroids;
    }
}
