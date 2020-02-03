using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SlimeRhythm
{
    public class Ball
    {
        protected AnimationManager _animationManager;
        protected Dictionary<string, Animation> _animations;
        private Rectangle _ballRectangle;

        public Vector2 Center { get; set; }

        public float Y { get; set; }

        public float Speed { get; private set; }

        public bool PlayerCollision { get; set; }

        public bool GroundCollision { get; set; }

        public Ball(Vector2 ballOrigin, Dictionary<string, Animation> animations)
        {
            PlayerCollision = false;
            GroundCollision = false;

            Center = new Vector2(ballOrigin.X + 50, ballOrigin.Y + 50);
            Y = ballOrigin.Y + 25;
            Speed = 5f;

            _ballRectangle = new Rectangle((int)ballOrigin.X + 25, (int)ballOrigin.Y + 25, 50, 50);

            _animations = animations;
            _animationManager = new AnimationManager(_animations.First().Value);
        }

        public void SetY(float y)
        {
            Center = new Vector2(this.Center.X, y + 50);
            Y = y;
            _ballRectangle.Y = (int)y;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _animationManager.Draw(spriteBatch, _ballRectangle);
        }

        public void UpdateAnimation(GameTime gameTime)
        {
            _animationManager.Update(gameTime);

            if (PlayerCollision) { _animationManager.Play(_animations["Collision"]); }
            else if (GroundCollision) { _animationManager.Play(_animations["Collision"]); }
            else { _animationManager.Play(_animations["Falling"]); }
            
        }
    }
}
