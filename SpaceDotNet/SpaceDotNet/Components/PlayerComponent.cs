using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceDotNet.Objects;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SpaceDotNet.Components {
    public enum PlayerState {
        Alive,
        Respawn,
        Dead
    }

    class PlayerComponent : DrawableGameComponent {
        public float MissilesPerSecond = 2.3f;
        public int Score { get; private set; }
        public int Lives { get; private set; } = 3;
        public bool IsShieldActive => _shieldActive;

        public PlayerState State { get; private set; } = PlayerState.Alive;

        public PlayerComponent(Game game) : base(game) {
            _game = (SpaceDotNetGame)game;
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
           
            _shield = new Sprite(Game.Content.Load<Texture2D>("sprites/shield"));
            _shield.ScaleTo(90);

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

            _explosionTextures = new Texture2D[3];
            for (int i = 0; i < _explosionTextures.Length; i++) {
                _explosionTextures[i] = Game.Content.Load<Texture2D>($"sprites/enemies/explosion{i + 1}");
            }
        }

        public override void Draw(GameTime gameTime) {
            _sb.Begin();
            foreach (var m in _missiles)
                m.Draw(_sb);
            _sprite.Draw(_sb);
            _burner.Draw(_sb);
            _shield.Draw(_sb);

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
                        Audio.CreateBullet();
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
                    if (_shieldActive) {
                        _shield.Position = _sprite.Position - new Vector2(0, 30);
                        var alpha = (float)(gameTime.TotalGameTime.TotalMilliseconds / 10) % 100 / 100.0f;
                        _shield.Tint = new Color(Color.White, alpha);
                    }

                    // update powerups

                    if (_missileSpeedTargetTime != TimeSpan.Zero && _missileSpeedTargetTime < gameTime.TotalGameTime)
                        MissilesPerSecond = 2.3f;

                    if (_playerSpeedTargetTime != TimeSpan.Zero && _playerSpeedTargetTime < gameTime.TotalGameTime)
                        _playerSpeed = 250;

                    if (_shieldTargetTime != TimeSpan.Zero && _shieldTargetTime < gameTime.TotalGameTime) {
                        _shieldActive = false;
                        _shield.State = SpriteState.Hidden;
                    }

                    break;

                case PlayerState.Respawn:
                    if (_respawnTime < gameTime.TotalGameTime) {
                        // reset enemies
                        _game.Enemies.ResetEnemies(false);

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
                        Audio.CreateExplosion(0);
                        break;
                    }
                }
                var enemy = _game.Enemies.IsHit(m);
                if (enemy != null) {
                    EnemyHit(m, enemy);
                }
                m.Update(gameTime);
                if (m.Position.Y < -m.TextureHeight)
                    m.State = SpriteState.Hidden;
            }

            base.Update(gameTime);
        }

        internal void ApplyPowerup(PowerupType type) {
            Score += 100 * _game.Level;

            switch (type) {
                case PowerupType.FasterFire:
                    if ((MissilesPerSecond += 1) > 5.5f)
                        _missileSpeed = 5.5f;
                    _missileSpeedTargetTime = _game.GameTime.TotalGameTime + TimeSpan.FromSeconds(10);
                    break;

                case PowerupType.FasterMove:
                    if ((_playerSpeed += 50) > 500)
                        _playerSpeed = 500;
                    _playerSpeedTargetTime = _game.GameTime.TotalGameTime + TimeSpan.FromSeconds(5);
                    break;

                case PowerupType.Shield:
                    if (!_shieldActive) {
                        _shieldActive = true;
                        _shield.State = SpriteState.Visible;
                    }
                    _shieldTargetTime = _game.GameTime.TotalGameTime + TimeSpan.FromSeconds(10);
                    break;
            }
        }

        public bool PlayerHit(Sprite sprite) {
            return _sprite.IsCollision(sprite);
        }

        public bool IsAlive => State == PlayerState.Alive;

        public bool PlayerEnemyHit(Enemy enemy) {
            if (_sprite.IsCollision(enemy.Ship)) {
                // player hit, explode
                ExplodePlayer(_game.GameTime);
                ExplodeEnemy(enemy);
                return true;
            }
            return false;
        }

        private void EnemyHit(Sprite missile, Enemy enemy) {
            // remove missile
            missile.State = SpriteState.Hidden;

            enemy.Hit(_game.Random.Next(10) + _misslePower);
            if (enemy.IsAlive) {
                ExplodeMissile(missile);
                Score += 5;
                Audio.CreateExplosion(0);
            }
            else {
                ExplodeEnemy(enemy);
                Score += enemy.Data.Score;
                _game.Enemies.EnemyDead(enemy);
                Audio.CreateExplosion(1);
            }
        }

        private void ExplodeEnemy(Enemy enemy) {
            var exp = _game.GetFreeExplostion();
            exp.InitTexture(_explosionTextures[_game.Random.Next(_explosionTextures.Length)], 8);
            exp.Position = enemy.Ship.Position;
            enemy.GotoState(EnemyState.Dead, 0);
            exp.HideOnAnimationEnd = true;
            exp.State = SpriteState.Visible;
            exp.AnimationFPS = 12;
            exp.ScaleTo(enemy.Ship.Width, true);
        }

        public void ExplodePlayer(GameTime gt) {
            Audio.CreateExplosion(1);
            var exp = _game.GetFreeExplostion();
            exp.Position = _sprite.Position;
            exp.HideOnAnimationEnd = true;
            exp.AnimationFPS = 12;
            exp.ScaleTo(100, true);
            exp.State = SpriteState.Visible;
            _sprite.State = SpriteState.Hidden;
            _burner.State = SpriteState.Hidden;
            if (--Lives > 0) {
                State = PlayerState.Respawn;
                _respawnTime = gt.TotalGameTime + TimeSpan.FromSeconds(5);
            }
            else {
                State = PlayerState.Dead;
                _game.GameOver();
            }

            // remove all powerups
            _missileSpeedTargetTime = _shieldTargetTime = _playerSpeedTargetTime = TimeSpan.Zero;
            _shieldActive = false;
            MissilesPerSecond = 2.3f;
            _playerSpeed = 250;
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
        Sprite _shield;
        float _missileSpeed = 500;
        int _misslePower = 5;
        Sprite[] _missiles = new Sprite[10];
        SpriteBatch _sb;
        TimeSpan _lastMissileShot;
        Texture2D _missleExplodeTexture;
        int _lastMissile = -1;
        float _playerSpeed = 250;
        TimeSpan _respawnTime;
        SpaceDotNetGame _game;
        Texture2D[] _explosionTextures;
        bool _shieldActive = false;
        TimeSpan _shieldTargetTime;
        TimeSpan _missileSpeedTargetTime;
        TimeSpan _playerSpeedTargetTime;
    }
}
