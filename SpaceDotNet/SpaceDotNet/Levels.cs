using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceDotNet {
    static class Levels {
        public static readonly LevelData[] Data = new LevelData[] {
            new LevelData {
                MaxAsteroids = 1,
                NumRows = 3,
                NumColumns = 6,
                StartSpeed = 50,
                EnemiesInRows = new [] { 0, 1, 0 }
            },
            new LevelData {
                MaxAsteroids = 2,
                NumRows = 3,
                NumColumns = 8,
                EnemiesInRows = new [] { 0, 1, 1 },
                StartSpeed = 55,
            },
            new LevelData {
                MaxAsteroids = 2,
                NumRows = 3,
                NumColumns = 9,
                StartSpeed = 60,
                EnemiesInRows = new [] { 2, 0, 1 }
            },
            new LevelData {
                MaxAsteroids = 2,
                NumRows = 4,
                NumColumns = 7,
                StartSpeed = 70,
                EnemiesInRows = new [] { 0, 1, 2, 1 }
            },
            new LevelData {
                MaxAsteroids = 3,
                NumRows = 4,
                NumColumns = 9,
                StartSpeed = 75,
                EnemiesInRows = new [] { 1, 2, 1, 2 }
            },
            new LevelData {
                MaxAsteroids = 3,
                NumRows = 5,
                NumColumns = 8,
                StartSpeed = 80,
                EnemiesInRows = new [] { 1, 2, 1, 2, 0 }
            },
        };
    }
}
