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
    // Manager that handles the creation and upkeep of balls on rhythm 
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

        // Begin game sequence
        public void Begin(Song song)
        {
            // adjust volume and play song
            MediaPlayer.Volume = 0.5f;
            MediaPlayer.Play(song);
            _ballTimer.Start();
            HandleBeat(this, new EventArgs());
        }

        // Stop game sequence
        public void Stop()
        {
            _ballTimer.Stop();
        }

        // Control ball and game events based on music 8th notes
        public void HandleBeat (object sender, EventArgs e)
        {
            _timerCount++;
            _beatCounter++;

            if ((_timerCount - 1) % barLength == 0 && _timerCount != 1) // Increase game difficulty when a new section is reached
            {
                IncreaseDifficulty();
            }

            if (_timerCount - 1 >= 250) // Trigger the End of Music sequence if the game is completed
            {
                EndOfMusic();
            }

            if (_beatCounter >= _spawnInterval) { // If the current beat is a spawn beat, add balls
                AddBall();
                _beatCounter = 0;
            }
        }

        // Increase the difficulty of the game
        public void IncreaseDifficulty()
        {
            _difficulty++;

            if (_difficulty == 2) // Change spawn rate to quarter notes
            {
                _spawnInterval = 2;
            }
            else if (_difficulty == 3) // Increase the number of balls spawned per tick
            {
                _numBallsPerTick = 2;
            }
            else if (_difficulty == 4) // change spawn rate to 8th notes
            {
                _spawnInterval = 1;
                //_numBallsPerTick = 3;
            }
        }

        // Game completion sequence
        public void EndOfMusic()
        {
            _numBallsPerTick = 0;
        }

        // Spawn balls at the top of the screen
        public void AddBall()
        {
            for (int i = 0; i < _numBallsPerTick; i++) // Add _numBallsPerTick balls
            {
                Vector2 ballPosition = new Vector2(_random.Next(0, 8) * 100, 0);

                Ball newBall = new Ball(ballPosition, _ballAnimations);
                _ballList.Add(newBall);
            }
        }

        // Handle the updating for each ball
        public void Update()
        {
            List<Ball> removeList = new List<Ball>();

            foreach (Ball ball in _ballList)
            {
                ball.SetY(ball.Y + ball.Speed); // update ball Y coordinate
                
                if (ball.Y > _screenHeight - 50) // detect ground collision
                {
                    ball.GroundCollision = true;
                }

                if (ball.Y > _screenHeight - 25) // remove ball shortly after ground collision
                {
                    removeList.Add(ball);
                }

            }

            try
            {
                _ballList = _ballList.Except(removeList).ToList();  // remove flagged balls
            }
            catch
            {

            }
        }

        // Detect collision between balls and player
        public bool TestForPlayerCollision(Rectangle playerRectangle)
        {
            try
            {
                foreach (Ball ball in _ballList) // iterate through balls
                {
                    // detect collision with player
                    float nearestX = Clamp(ball.Center.X, playerRectangle.X + 10, playerRectangle.X + 80);
                    float nearestY = Clamp(ball.Center.Y, playerRectangle.Y + 30, playerRectangle.Y + 100);
                    if (Math.Pow(20, 2) > (Math.Pow(ball.Center.X - nearestX, 2) + Math.Pow(ball.Center.Y - nearestY, 2)))
                    {
                        ball.PlayerCollision = true;
                        return true;
                    }
                }
            }
            catch
            {

            }
            return false;
        }

        // Clamp function
        public float Clamp(float point, float min, float max)
        {
            if (point < min) return min;
            else if (point > max) return max;
            else return point;
        }

        // Call each active ball's UpdateAnimation method
        public void UpdateAnimations(GameTime gameTime)
        {
            try
            {
                foreach (Ball ball in _ballList)
                {
                    ball.UpdateAnimation(gameTime);
                }
            }
            catch
            {

            }
        }

        // Draw each active ball
        public void Draw(SpriteBatch spriteBatch)
        {
            try
            {
                foreach (Ball ball in _ballList)
                {
                    ball.Draw(spriteBatch);
                }
            }
            catch
            {

            }
        }
    }
}
