using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceDotNet {
    static class Levels {
        public static readonly LevelData[] Data = new LevelData[] {
            new LevelData {
                MaxAsteroids = 2,
                NumRows = 2,
                NumColumns = 7,
                EnemiesInRows = new [] { 0, 1 }
            },
            new LevelData {
                MaxAsteroids = 2,
                NumRows = 2,
                NumColumns = 8,
                EnemiesInRows = new [] { 0, 1 },
                StartSpeed = 60,
            },
            new LevelData {
                MaxAsteroids = 3,
                NumRows = 3,
                NumColumns = 9,
                StartSpeed = 70,
                EnemiesInRows = new [] { 0, 1, 2 }
            },
            new LevelData {
                MaxAsteroids = 3,
                NumRows = 4,
                NumColumns = 7,
                StartSpeed = 80,
                EnemiesInRows = new [] { 0, 1, 2, 1 }
            },
            new LevelData {
                MaxAsteroids = 3,
                NumRows = 4,
                NumColumns = 9,
                StartSpeed = 85,
                EnemiesInRows = new [] { 1, 2, 1, 2 }
            },
        };
    }
}
