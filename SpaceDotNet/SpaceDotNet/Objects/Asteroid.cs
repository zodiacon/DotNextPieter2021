using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SpaceDotNet.Objects {
    class Asteroid : Sprite {
        static Random _rnd = SpaceDotNetGame.Instance.Random;

        public Asteroid(Texture2D texture) : base(texture) {
            Debug.Assert(_rnd != null);

            Velocity = new Vector2(_rnd.NextFloat() * 5 - 2.5f, _rnd.NextFloat() * 65 + 15);
            Position = new Vector2(_rnd.NextFloat() * SpaceDotNetGame.Instance.Width, _rnd.NextFloat() * 100 - 200);
            Angle = _rnd.NextFloat() * Constants.PI * 2;
            _rotationSpeed = _rnd.NextFloat() * 2 - 1;

            switch (_rnd.Next(3)) {
                case 0:
                    Tint.R -= (byte)_rnd.Next(200);
                    break;
                case 1:
                    Tint.G -= (byte)_rnd.Next(200);
                    break;
                case 2:
                    Tint.B -= (byte)_rnd.Next(200);
                    break;
            }
        }

        public override void Update(GameTime gameTime) {
            if ((Angle += _rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds) > Constants.PI * 2)
                Angle -= Constants.PI * 2;
            else if (Angle < 0)
                Angle += Constants.PI * 2;
            base.Update(gameTime);
        }

        float _rotationSpeed;
    }
}
