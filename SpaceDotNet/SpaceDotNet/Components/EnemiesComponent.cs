using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDotNet.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpaceDotNet.Components {
    enum EnemyState {
        None,
        MarchRight,
        MarchLeft,
        Drop,
        Halt,
        Dead
    }

    class EnemiesComponent : DrawableGameComponent {
        public EnemiesComponent(Game game) : base(game) {
            _game = (SpaceDotNetGame)game;
        }

        protected override void LoadContent() {
            _sb = new SpriteBatch(Game.GraphicsDevice);
            var shotTexture = Game.Content.Load<Texture2D>("sprites/enemies/shot");
            for (int i = 0; i < _shots.Length; i++) {
                _shots[i] = new Sprite(shotTexture);
                _shots[i].State = SpriteState.Hidden;
            }

            base.LoadContent();
        }

        public void InitLevel(int level) {
            Debug.Assert(level > 0);
            _levelData = Levels.Data[level - 1];
            _matrix = new Enemy[_levelData.NumRows, _levelData.NumColumns];
            _ships.Clear();
            _alive = _levelData.NumColumns * _levelData.NumRows;
            ResetEnemies();
        }

        public void ResetEnemies(bool fullReset = true) {
            float x = 100, y = 70;
            _state = EnemyState.MarchRight;
            _targetX = Game.Window.ClientBounds.Width + 100;
            var data = _levelData;

            if (fullReset) {
                _currentSpeed = data.StartSpeed;
            }

            for (int row = 0; row < data.NumRows; row++) {
                for (int col = 0; col < data.NumColumns; col++) {
                    var ship = fullReset ? new Enemy(Game, data.EnemiesInRows[row]) : _matrix[row, col];
                    ship.SetPosition(new Vector2(x + col * 110, y + row * 50));
                    ship.GotoState(_state, _currentSpeed);
                    if (fullReset) {
                        _matrix[row, col] = ship;
                        _ships.Add(ship);
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime) {
            _sb.Begin();
            foreach (var ship in _ships)
                ship.Draw(gameTime, _sb);
            foreach (var shot in _shots)
                shot.Draw(_sb);
            _sb.End();
        }

        public override void Update(GameTime gameTime) {
            var newState = _state;
            float speed = 0;
            foreach (var ship in _ships) {
                if (!ship.IsAlive)
                    continue;

                if (_game.Player.PlayerEnemyHit(ship)) {
                    EnemyDead(ship);
                    break;
                }

                ship.Update(gameTime);
                switch (_state) {
                    case EnemyState.MarchRight:
                        if (ship.GetPosition().X > _targetX) {
                            newState = EnemyState.Drop;
                            speed = _currentSpeed;
                            _targetY = ship.GetPosition().Y + 40;
                            _leadShip = ship;
                        }
                        break;

                    case EnemyState.Drop:
                        if (_leadShip.GetPosition().Y > _targetY) {
                            if (_prevState == EnemyState.MarchRight) {
                                newState = EnemyState.MarchLeft;
                                _targetX = -100;
                            }
                            else {
                                newState = EnemyState.MarchRight;
                                _targetX = Game.Window.ClientBounds.Width + 100;
                            }
                            speed = _currentSpeed;
                        }
                        break;

                    case EnemyState.MarchLeft:
                        if (ship.GetPosition().X < _targetX) {
                            newState = EnemyState.Drop;
                            speed = _currentSpeed;
                            _targetY = ship.GetPosition().Y + 40;
                            _leadShip = ship;
                        }
                        break;
                }

                if (_game.Player.IsAlive && _state == EnemyState.MarchLeft || _state == EnemyState.MarchRight) {
                    if (_totalShots < _levelData.MaxEnemyShots && _game.Random.Next(100) < _levelData.ShotProb)
                        BeginShot(ship);
                }

            }

            if (newState != _state) {
                _prevState = _state;
                _state = newState;
                foreach (var ship in _ships)
                    ship.GotoState(newState, speed);
            }

            foreach (var shot in _shots) {
                if (shot.State != SpriteState.Visible)
                    continue;

                shot.Update(gameTime);
                if (shot.Position.Y > _game.Window.ClientBounds.Bottom + 40) {
                    shot.State = SpriteState.Hidden;
                    _totalShots--;
                }
                else if (_game.Player.PlayerHit(shot)) {
                    if (!_game.Player.IsShieldActive)
                        _game.Player.ExplodePlayer(gameTime);
                    shot.State = SpriteState.Hidden;
                    _totalShots--;
                }
            }
        }

        private void BeginShot(Enemy ship) {
            for (int i = 0; i < _shots.Length; i++) {
                _lastShotIndex = (_lastShotIndex + 1) % _shots.Length;
                if (_shots[i].State == SpriteState.Hidden) {
                    // available shot
                    var shot = _shots[i];
                    shot.Position = ship.GetPosition() + new Vector2(0, ship.Ship.Height / 2);
                    shot.Velocity = new Vector2(0, _levelData.ShotSpeed + _game.Random.Next(50));
                    shot.State = SpriteState.Visible;
                    _totalShots++;
                    break;
                }
            }
        }

        public Enemy IsHit(Sprite other) {
            return _ships.FirstOrDefault(s => s.IsAlive && s.Ship.IsCollision(other));
        }

        public void EnemyDead(Enemy enemy) {
            if ((_currentSpeed += enemy.Data.Power) > 500) {
                _currentSpeed = 500;
            }

            if (--_alive == 0)
                _game.NextLevel();
            else {
                if (_game.Random.Next(100) < 10 || _lastPowerupTime + TimeSpan.FromSeconds(12) < _game.GameTime.TotalGameTime) {
                    _game.CreatePowerup(enemy.Ship);
                    _lastPowerupTime = _game.GameTime.TotalGameTime;
                }
            }

        }

        SpriteBatch _sb;
        readonly List<Enemy> _ships = new List<Enemy>(32);
        Sprite[] _shots = new Sprite[10];
        EnemyState _state, _prevState;
        Enemy[,] _matrix;
        Enemy _leadShip;
        float _targetX, _targetY;
        float _currentSpeed;
        int _alive;
        SpaceDotNetGame _game;
        LevelData _levelData;
        int _totalShots;
        int _lastShotIndex = -1;
        TimeSpan _lastPowerupTime;
    }
}
