using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDotNet.Components;
using System;

namespace SpaceDotNet.Objects {
    class Enemy {
        public Enemy(Game game, int index) {
            _data = Enemies.Data[index];

            _ship.InitTexture(Enemies.LoadShipTexture(game, index));
            _exhaust.InitTexture(Enemies.LoadExhaustTexture(game, index), 4);
        }

        public bool IsAlive { get; private set; } = true;

        public void SetPosition(in Vector2 pos) {
            _ship.Position = pos;
            _ship.State = SpriteState.Visible;
            _exhaust.State = SpriteState.Visible;
        }

        public Vector2 GetPosition() {
            return _ship.Position;
        }

        public void SetDirection(float speed, float angle) {
            _ship.Velocity = new Vector2(speed, 0);
            _ship.Angle = angle;
            _exhaust.Velocity = new Vector2(speed, 0);
            _exhaust.Angle = angle;
            _exhaust.Position = _ship.Position - new Vector2(_ship.Width / 2.5f * (float)Math.Cos(_ship.Angle), 0);
        }

        public virtual void Draw(GameTime gt, SpriteBatch sb) {
            _ship.Draw(sb);
            _exhaust.Draw(sb);
        }

        public virtual void Update(GameTime gt) {
            _ship.Update(gt);
            _exhaust.Update(gt);
        }

        public void GotoState(EnemyState state, float speed) {
            if (state == _state)
                return;

            _state = state;
            switch(state) {
                case EnemyState.MarchRight:
                    SetDirection(speed, 0);
                    break;

                case EnemyState.MarchLeft:
                    SetDirection(-speed, Constants.PI);
                    break;

                case EnemyState.Drop:
                    _ship.Velocity = new Vector2(0, speed);
                    _exhaust.Velocity = _ship.Velocity;
                    break;

                case EnemyState.Halt:
                    _ship.Velocity = _exhaust.Velocity = new Vector2();
                    break;
            };
        }

        EnemyData _data;
        EnemyState _state = EnemyState.None;
        Sprite _ship = new Sprite() { Scale = .8f };
        Sprite _exhaust = new Sprite() { Scale = 1 };
    }
}
