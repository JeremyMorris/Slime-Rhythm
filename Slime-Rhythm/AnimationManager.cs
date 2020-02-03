using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SlimeRhythm
{
    // Made referencing tutorial: https://www.youtube.com/watch?v=OLsiWxgONeM
    public class AnimationManager
    {
        private Animation _animation;

        private float _timer;

        public AnimationManager(Animation animation)
        {
            _animation = animation;
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle rectangle)
        {
            spriteBatch.Draw(_animation.Texture,
                            rectangle,
                            new Rectangle(_animation.CurrentFrame * _animation.FrameWidth,
                                          0,
                                          _animation.FrameWidth,
                                          _animation.FrameHeight),
                            Color.White);
        }

        public void Play(Animation animation)
        {
            if (animation == _animation)
            {
                return;
            }

            _animation = animation;
            _animation.CurrentFrame = 0;
            _timer = 0;
        }

        public void Stop()
        {
            _timer = 0f;
            _animation.CurrentFrame = 0;
        }

        public void Update(GameTime gameTime)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_timer > _animation.FrameSpeed)
            {
                _timer = 0f;

                if (_animation.CurrentFrame == _animation.FrameCount - 1 && (!_animation.IsLooping)) return;
                
                _animation.CurrentFrame++;

                if (_animation.CurrentFrame == _animation.FrameCount)
                {
                    _animation.CurrentFrame = 0;
                }
            }
        }
    }
}
