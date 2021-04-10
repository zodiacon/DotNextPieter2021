using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceDotNet.Objects {
    enum PowerupType {
        FasterFire,
        FasterMove,
        Shield,
    }

    class Powerup : Sprite {
        public PowerupType Type { get; private set; }
        float _rotateSpeed;

        public void Init(Texture2D texture, PowerupType type, float rotateSpeed) {
            InitTexture(texture);
            Type = type;
            _rotateSpeed = rotateSpeed;
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            Angle += _rotateSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }
}
