using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SpaceDotNet {
    public enum SpriteState {
        Visible,
        Hidden,
        Disabled
    }

    public class Sprite {
        public Texture2D Texture { get; private set; }
        public Vector2 Position;
        public Vector2 Origin;
        public Color Tint = Color.White;
        public float Angle;
        public float Scale = 1;
        public Vector2 Velocity;
        public int AnimationFPS = 8;
        public SpriteState State = SpriteState.Visible;
        public int TextureHeight { get; private set; }
        public int TotalFrames { get; private set; }
        public bool HideOnAnimationEnd { get; set; }
        public float BoundingBoxShrinkFactor = .1f;

        public Sprite(Texture2D texture, int frames = 1) {
            InitTexture(texture, frames);
        }

        public Sprite() {
            State = SpriteState.Hidden;
        }

        public void InitTexture(Texture2D texture, int frames = 1) {
            Texture = texture ?? throw new ArgumentNullException(nameof(texture));
            TotalFrames = frames >= 1 ? frames : throw new ArgumentException("must have at least one frame", nameof(frames));

            int height = Texture.Height / frames;
            _source.Clear();
            for (int i = 0; i < frames; i++) {
                _source.Add(new Rectangle(new Point(0, i * height), new Point(texture.Width, height)));
            }
            TextureHeight = height;
            Origin = new Vector2(texture.Width / 2, height / 2);
            _frame = 0;
            _lastTime = TimeSpan.Zero;
        }

        public float Height => TextureHeight * Scale;
        public float Width => Texture.Width * Scale;

        public virtual void Draw(SpriteBatch sb) {
            if (Texture != null && State != SpriteState.Hidden) {
                sb.Draw(Texture, Position, _source[_frame], Tint, Angle, Origin, Scale, SpriteEffects.None, 0);
            }
        }

        public void ScaleTo(float widthOrHeight, bool height = false) {
            Scale = height ? widthOrHeight / TextureHeight : widthOrHeight / Texture.Width;
        }

        public virtual void Update(GameTime gameTime) {
            if (_lastTime == TimeSpan.Zero)
                _lastTime = gameTime.TotalGameTime;

            if (State == SpriteState.Hidden)
                return;

            if (TotalFrames > 1 && gameTime.TotalGameTime.TotalMilliseconds - _lastTime.TotalMilliseconds > 1000 / AnimationFPS) {
                _frame = (++_frame) % TotalFrames;
                _lastTime = gameTime.TotalGameTime;
                if (_frame == 0 && HideOnAnimationEnd) {
                    State = SpriteState.Hidden;
                }
            }
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public virtual BoundingBox Bounds {
            get {
                if (Texture == null)
                    throw new InvalidOperationException("no texture associated with the sprite");

                var box = new BoundingBox {
                    X = Position.X - Width / 2,
                    Y = Position.Y - Height / 2,
                    Width = Width,
                    Height = Height
                };
                return box.Shrink(BoundingBoxShrinkFactor);
            }
        }

        public virtual bool IsCollision(Sprite other) {
            if (State != SpriteState.Visible || other.State != SpriteState.Visible)
                return false;

            return Bounds.Intersects(other.Bounds);
        }

        List<Rectangle> _source = new List<Rectangle>();
        int _frame;
        TimeSpan _lastTime;
    }
}
