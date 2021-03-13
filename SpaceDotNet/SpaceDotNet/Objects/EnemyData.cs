using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceDotNet.Objects {
    record EnemyData {
        public int Score { get; init; }
        public int Power { get; init; }
    }

    static class Enemies {
        public static readonly ContentManager Content = SpaceDotNetGame.Instance.Content;

        public static readonly EnemyData[] Data = new EnemyData[] {
            new EnemyData { Power = 10, Score = 10 },
            new EnemyData { Power = 20, Score = 20 },
            new EnemyData { Power = 50, Score = 80 },
        };

        public static Texture2D LoadShipTexture(Game game, int index) {
            return game.Content.Load<Texture2D>($"sprites/enemies/ship{index + 1}");
        }

        public static Texture2D LoadExhaustTexture(Game game, int index) {
            return game.Content.Load<Texture2D>($"sprites/enemies/exhaust{index + 1}");
        }
    }
}
