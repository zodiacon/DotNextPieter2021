using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceDotNet {
    static class Extensions {
        public static float NextFloat(this Random rnd) {
            return (float)rnd.NextDouble();
        }
    }
}
