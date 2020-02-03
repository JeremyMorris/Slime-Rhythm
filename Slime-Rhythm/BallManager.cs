using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SlimeRhythm
{
    // ball manager created with assistance from Stack Overflow post https://stackoverflow.com/questions/22721875/xna-need-help-spawning-enemies-on-any-side-on-a-timer-tick-then-maving-them-m
    public class BallManager
    {
        const int barLength = 64;

        protected Dictionary<string, Animation> _ballAnimations;

        private List<Ball> _ballList = new List<Ball>();
        private Timer _ballTimer;
        private Random _random;
        private int _screenHeight;
        private int _numBallsPerTick;
        private int _timerCount;
        private int _difficulty;
        private int _beatCounter;
        private int _spawnInterval;

        public BallManager(Dictionary<string, Animation> animations, Random r, int screenHeight)
        {
            _ballAnimations = animations;
            _random = r;
            _screenHeight = screenHeight;
            _numBallsPerTick = 1;
            _timerCount = 0;
            _difficulty = 1;
            _beatCounter = 1;
            _spawnInterval = 4;

            // Create 96 BPM timer to align with music
            _ballTimer = new Timer(312/*625*/);
            _ballTimer.Elapsed += HandleBeat;
        }

        public void Begin(Song song)
        {
            // adjust volume and play song
            MediaPlayer.Volume = 0.5f;
            MediaPlayer.Play(song);
            _ballTimer.Start();
            HandleBeat(this, new EventArgs());
        }

        public void Stop()
        {
            _ballTimer.Stop();
        }

        public void HandleBeat (object sender, EventArgs e)
        {
            _timerCount++;
            _beatCounter++;

            if ((_timerCount - 1) % barLength == 0 && _timerCount != 1)
            {
                IncreaseDifficulty();
            }

            if (_timerCount - 1 >= 250)
            {
                EndOfMusic();
            }

            if (_beatCounter >= _spawnInterval) { 
                AddBall();
                _beatCounter = 0;
            }
        }

        public void IncreaseDifficulty()
        {
            _difficulty++;

            if (_difficulty == 2)
            {
                _spawnInterval = 2;
            }
            else if (_difficulty == 3)
            {
                _numBallsPerTick = 2;
            }
            else if (_difficulty == 4)
            {
                _spawnInterval = 1;
                //_numBallsPerTick = 3;
            }
        }

        public void EndOfMusic()
        {
            _numBallsPerTick = 0;
        }

        public void AddBall()
        {
            for (int i = 0; i < _numBallsPerTick; i++)
            {
                Vector2 ballPosition = new Vector2(_random.Next(0, 8) * 100, 0);

                Ball newBall = new Ball(ballPosition, _ballAnimations);
                _ballList.Add(newBall);
            }
        }

        public void Update(GameTime gameTime)
        {
            List<Ball> removeList = new List<Ball>();

            foreach (Ball ball in _ballList)
            {
                ball.SetY(ball.Y + ball.Speed); // update ball Y coordinate
                
                if (ball.Y > _screenHeight - 50)
                {
                    ball.GroundCollision = true;
                }

                if (ball.Y > _screenHeight - 25) // remove ball after ground collision
                {
                    removeList.Add(ball);
                }

            }

            _ballList = _ballList.Except(removeList).ToList();
        }

        public bool TestForPlayerCollision(Rectangle playerRectangle)
        {
            foreach (Ball ball in _ballList)
            {
                try
                {
                    float nearestX = Clamp(ball.Center.X, playerRectangle.X + 10, playerRectangle.X + 80);
                    float nearestY = Clamp(ball.Center.Y, playerRectangle.Y + 30, playerRectangle.Y + 100);
                    if (Math.Pow(20, 2) > (Math.Pow(ball.Center.X - nearestX, 2) + Math.Pow(ball.Center.Y - nearestY, 2)))
                    {
                        ball.PlayerCollision = true;
                        return true;
                    }
                }
                catch
                {

                }
            }
            return false;
        }

        public float Clamp(float point, float min, float max)
        {
            if (point < min) return min;
            else if (point > max) return max;
            else return point;
        }

        public void UpdateAnimations(GameTime gameTime)
        {
            foreach (Ball ball in _ballList)
            {
                try
                {
                    ball.UpdateAnimation(gameTime);
                }
                catch
                {

                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Ball ball in _ballList)
            {
                try
                {
                    ball.Draw(spriteBatch);
                }
                catch
                {

                }
            }
        }
    }
}
