using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceDotNet {
    static class Audio {
        public static void LoadContent(Game game) {
            var content = game.Content;
            _explosions[0] = content.Load<SoundEffect>("audio/explosion1");
            _explosions[1] = content.Load<SoundEffect>("audio/explosion2");
            _bullet = content.Load<SoundEffect>("audio/bullet");
            _powerup = content.Load<SoundEffect>("audio/coin");
        }

        static SoundEffectInstance CreateSound(SoundEffect effect) {
            var instance = effect.CreateInstance();
            instance.Play();
            return instance;
        }

        public static SoundEffectInstance CreateExplosion(int index) {
            return CreateSound(_explosions[index]);
        }

        public static SoundEffectInstance PlayPowerup() {
            return CreateSound(_powerup);
        }

        public static SoundEffectInstance CreateBullet() {
            return CreateSound(_bullet);
        }

        static SoundEffect[] _explosions = new SoundEffect[2];
        static SoundEffect _bullet;
        static SoundEffect _powerup;
    }
}
