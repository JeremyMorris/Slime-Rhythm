using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SlimeRhythm
{
    // Controls the drawing and animations of game objects
    // Made referencing tutorial: https://www.youtube.com/watch?v=OLsiWxgONeM
    public class AnimationManager
    {
        private Animation _animation;

        private float _timer;

        public AnimationManager(Animation animation)
        {
            _animation = animation;
        }

        // Draw the current frame of the sprite
        public void Draw(SpriteBatch spriteBatch, Rectangle rectangle)
        {
            spriteBatch.Draw(_animation.Texture,
                            rectangle,
                            new Rectangle(_animation.CurrentFrame * _animation.FrameWidth, // get correct frame from spritesheet
                                          0,
                                          _animation.FrameWidth,
                                          _animation.FrameHeight),
                            Color.White);
        }

        // Change which animation is currently playing
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

        // Stop the current animation
        public void Stop()
        {
            _timer = 0;
            _animation.CurrentFrame = 0;
        }

        // Update which frame of the animation to play
        public void Update(GameTime gameTime)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_timer > _animation.FrameSpeed) // if the time since last frame change exceeds the animation's frame speed
            {
                _timer = 0f;

                // do not progress frames if the last frame of a non-looping animation is reached
                if (_animation.CurrentFrame == _animation.FrameCount - 1 && (!_animation.IsLooping)) return;
                
                _animation.CurrentFrame++; // increment the frame counter

                // if the last frame of the animation has been exceeded, loop back to the first frame
                if (_animation.CurrentFrame == _animation.FrameCount) 
                {
                    _animation.CurrentFrame = 0;
                }
            }
        }
    }
}
