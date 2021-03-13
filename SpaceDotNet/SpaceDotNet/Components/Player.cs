using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SpaceDotNet.Components {
    public enum PlayerState {
        Alive,
        Respawn,
        Dead
    }

    class Player : DrawableGameComponent {
        public float MissilesPerSecond = 1;
        public int Score { get; private set; }
        public int Lives { get; private set; } = 3;

        public PlayerState State { get; private set; } = PlayerState.Alive;

        public Player(Game game) : base(game) {
        }

        public override void Initialize() {
            _sb = new SpriteBatch(Game.GraphicsDevice);
           
            base.Initialize();
        }

        protected override void LoadContent() {
            var shipTexture = Game.Content.Load<Texture2D>("sprites/player");
            _sprite = new Sprite(shipTexture) {
                Position = new Vector2(300, 700),
            };
            _sprite.ScaleTo(90);

            var burnerTexture = Game.Content.Load<Texture2D>("sprites/burner");
            _burner = new Sprite(burnerTexture, 10) {
                Position = new Vector2(_sprite.Position.X, _sprite.Position.Y + _sprite.Height - 20),
                Scale = .6f,
                AnimationFPS = 10
            };

            var missileTexture = Game.Content.Load<Texture2D>("sprites/missile");
            for (int i = 0; i < _missiles.Length; i++) {
                _missiles[i] = new Sprite(missileTexture, 10) {
                    State = SpriteState.Hidden,
                    Scale = .4f,
                    BoundingBoxShrinkFactor = .3f
                };
            }

            _missleExplodeTexture = Game.Content.Load<Texture2D>("sprites/missile-explode");
        }

        public override void Draw(GameTime gameTime) {
            _sb.Begin();
            foreach (var m in _missiles)
                m.Draw(_sb);
            _sprite.Draw(_sb);
            _burner.Draw(_sb);

            _sb.End();
        }

        public override void Update(GameTime gameTime) {
            var asteroids = SpaceDotNetGame.Instance.StarField.Asteroids;

            switch (State) {
                case PlayerState.Alive:
                    var state = Keyboard.GetState();
                    if (state.IsKeyDown(Keys.Left))
                        _sprite.Velocity.X = -_playerSpeed;
                    else if (state.IsKeyDown(Keys.Right))
                        _sprite.Velocity.X = _playerSpeed;
                    else
                        _sprite.Velocity.X = 0;

                    _burner.Velocity = _sprite.Velocity;

                    if (state.IsKeyDown(Keys.Space) && gameTime.TotalGameTime > _lastMissileShot) {
                        // start a missile
                        var missile = FindMissile();
                        Debug.Assert(missile != null);
                        missile.Position.X = _sprite.Position.X;
                        missile.Position.Y = _sprite.Position.Y - _sprite.Height + missile.Height;
                        _lastMissileShot = gameTime.TotalGameTime + TimeSpan.FromMilliseconds(1000 / MissilesPerSecond);
                        missile.State = SpriteState.Visible;
                        missile.Velocity.Y = -_missileSpeed;
                    }
                    foreach (var ast in asteroids) {
                        if (ast.IsCollision(_sprite)) {
                            // player hit
                            ExplodePlayer(gameTime);
                            // remove asteroid as well
                            SpaceDotNetGame.Instance.StarField.RemoveAsteroid(ast);
                            break;
                        }
                    }
                    _sprite.Update(gameTime);
                    _burner.Update(gameTime);
                    break;

                case PlayerState.Respawn:
                    if (_respawnTime < gameTime.TotalGameTime) {
                        // respawn player
                        _sprite.State = SpriteState.Visible;
                        _burner.State = SpriteState.Visible;
                        State = PlayerState.Alive;
                    }
                    break;
            }


            foreach (var m in _missiles) {
                if (m.State != SpriteState.Visible)
                    continue;

                foreach (var ast in asteroids) {
                    if (ast.IsCollision(m)) {
                        // asteroid hit
                        ExplodeMissile(m);
                        break;
                    }
                }
                m.Update(gameTime);
                if (m.Position.Y < -m.TextureHeight)
                    m.State = SpriteState.Hidden;
            }

            base.Update(gameTime);
        }

        private void ExplodePlayer(GameTime gt) {
            var exp = SpaceDotNetGame.Instance.GetFreeExplostion();
            exp.Position = _sprite.Position;
            exp.HideOnAnimationEnd = true;
            exp.AnimationFPS = 12;
            exp.ScaleTo(100, true);
            exp.State = SpriteState.Visible;
            _sprite.State = SpriteState.Hidden;
            _burner.State = SpriteState.Hidden;
            State = PlayerState.Respawn;
            _respawnTime = gt.TotalGameTime + TimeSpan.FromSeconds(5);
            Lives--;
        }

        private void ExplodeMissile(Sprite missile) {
            missile.State = SpriteState.Hidden;
            var exp = SpaceDotNetGame.Instance.GetFreeExplostion();
            exp.Position = missile.Position;
            exp.InitTexture(_missleExplodeTexture, 9);
            exp.AnimationFPS = 22;
            exp.Scale = .5f;
            exp.HideOnAnimationEnd = true;
            exp.State = SpriteState.Disabled;
        }

        private Sprite FindMissile() {
            for (int i = 0; i < _missiles.Length; i++) {
                _lastMissile++;
                _lastMissile %= _missiles.Length;
                if (_missiles[_lastMissile].State == SpriteState.Hidden)
                    return _missiles[_lastMissile];
            }
            return null;
        }

        Sprite _sprite;
        Sprite _burner;
        float _missileSpeed = 500;
        Sprite[] _missiles = new Sprite[16];
        SpriteBatch _sb;
        TimeSpan _lastMissileShot;
        Texture2D _missleExplodeTexture;
        int _lastMissile = -1;
        float _playerSpeed = 200;
        TimeSpan _respawnTime;
    }
}
