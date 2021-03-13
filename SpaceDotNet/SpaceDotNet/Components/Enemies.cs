using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDotNet.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SpaceDotNet.Components {
    enum EnemyState {
        None,
        MarchRight,
        MarchLeft,
        Drop,
        Halt,
        Dead
    }

    class Enemies : DrawableGameComponent {
        public Enemies(Game game) : base(game) {
            _game = (SpaceDotNetGame)game;
        }

        protected override void LoadContent() {
            _sb = new SpriteBatch(Game.GraphicsDevice);

            base.LoadContent();
        }

        public void InitLevel(int level) {
            Debug.Assert(level > 0);
            var data = Levels.Data[level - 1];
            _ships.Clear();
            float x = 100, y = 70;
            _state = EnemyState.MarchRight;
            _targetX = Game.Window.ClientBounds.Width + 100;
            _matrix = new Enemy[data.NumRows, data.NumColumns];

            for (int row = 0; row < data.NumRows; row++) {
                for (int col = 0; col < data.NumColumns; col++) {
                    var ship = new Enemy(Game, data.EnemiesInRows[row]);
                    ship.SetPosition(new Vector2(x + col * 110, y + row * 60));
                    ship.GotoState(_state, data.StartSpeed);
                    _matrix[row, col] = ship;
                    _ships.Add(ship);
                } 
            }
            _currentSpeed = data.StartSpeed;
            _alive = data.NumColumns * data.NumRows;
        }

        public override void Draw(GameTime gameTime) {
            _sb.Begin();
            foreach (var ship in _ships)
                ship.Draw(gameTime, _sb);
            _sb.End();
        }

        public override void Update(GameTime gameTime) {
            var newState = _state;
            float speed = 0;
            foreach (var ship in _ships) {
                if (ship.IsAlive) {
                    ship.Update(gameTime);
                    switch (_state) {
                        case EnemyState.MarchRight:
                            if (ship.GetPosition().X > _targetX) {
                                newState = EnemyState.Drop;
                                speed = 70;
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
                                _currentSpeed += 5;
                                speed = _currentSpeed;
                            }
                            break;
                            
                        case EnemyState.MarchLeft:
                            if (ship.GetPosition().X < _targetX) {
                                newState = EnemyState.Drop;
                                speed = 70;
                                _targetY = ship.GetPosition().Y + 40;
                                _leadShip = ship;
                            }
                            break;
                    }
                }
            }
            if (newState != _state) {
                _prevState = _state;
                _state = newState;
                foreach (var ship in _ships)
                    ship.GotoState(newState, speed);
            }
        }

        public Enemy IsHit(Sprite other) {
            return _ships.FirstOrDefault(s => s.IsAlive && s.Ship.IsCollision(other));
        }

        public void EnemyDead(Enemy enemy) {
            if (--_alive == 0)
                _game.NextLevel();
        }

        SpriteBatch _sb;
        readonly List<Enemy> _ships = new List<Enemy>(32);
        EnemyState _state, _prevState;
        Enemy[,] _matrix;
        Enemy _leadShip;
        float _targetX, _targetY;
        float _currentSpeed;
        int _alive;
        SpaceDotNetGame _game;

    }
}
