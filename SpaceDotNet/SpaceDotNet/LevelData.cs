using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceDotNet {
    class LevelData {
        public int MaxAsteroids = 1;
        public int NumRows = 2;
        public int NumColumns = 7;
        public int StartSpeed = 40;
        public int MaxEnemyShots = 3;
        public float MaxEnemySpeed = 120;
        public int[] EnemiesInRows;
        public int ShotProb = 5;
        public float ShotSpeed = 50;
    }
}
