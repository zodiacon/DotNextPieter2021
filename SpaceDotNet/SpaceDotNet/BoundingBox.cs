using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceDotNet {
    public struct BoundingBox {
        public float X, Y;
        public float Width, Height;

        public bool Intersects(in BoundingBox box) {
            return Math.Abs(box.X + box.Width / 2 - X - Width / 2) < (Width + box.Width) / 2
                && Math.Abs(box.Y + box.Height / 2 - Y - Height / 2) < (Height + box.Height) / 2;
        }

        public BoundingBox Shrink(float amount) {
            X -= Width * amount;
            Width *= (1 - amount);
            Y -= Height * amount;
            Height *= (1 - amount);

            return this;
        }
    }
}
