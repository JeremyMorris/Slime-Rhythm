using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Slime_Rhythm;

namespace SlimeRhythm
{
    // Object for the player character
    public class Player
    {
        private float _maxSpeed = 0.6f;
        private float _acceleration = 0.04f;
        private Rectangle _playerRectangle;
        protected AnimationManager _animationManager;
        protected Dictionary<string, Animation> _animations;
        protected ParticleSystem _particleSystem;

        public Texture2D Sprite { get; private set; }

        public float X { get; set; }

        public float Y { get; set; }

        public float Speed { get; set; }

        public float MaxSpeed { get { return _maxSpeed; } }

        public bool FacingRight { get; set; }

        public bool FatalCollision { get; set; }

        public float Acceleration { get { return _acceleration; } }

        public Rectangle PlayerRectangle { get { return _playerRectangle; } }

        Random random = new Random();

        public Player(Vector2 playerPosition, Texture2D sprite)
        {
            X = playerPosition.X;
            Y = playerPosition.Y;
            Speed = 0;
            _playerRectangle = new Rectangle((int)X, (int)Y, 100, 100);

            Sprite = sprite;
        }
        
        public Player(Vector2 playerPosition, Dictionary<string, Animation> animations, Texture2D particleSprite)
        {
            X = playerPosition.X;
            Y = playerPosition.Y;
            Speed = 0;
            _playerRectangle = new Rectangle((int)X, (int)Y, 100, 100);

            _animations = animations;
            _animationManager = new AnimationManager(_animations.First().Value);

            _particleSystem = new ParticleSystem(100, particleSprite);
            _particleSystem.Emitter = new Vector2(X, Y);
            _particleSystem.SpawnPerFrame = 1;

            // Set the SpawnParticle method
            _particleSystem.SpawnParticle = (ref Particle particle) =>
            {
                if (FacingRight)
                {
                    particle.Position = new Vector2(X, Y + 83);
                    particle.Velocity = new Vector2(
                        MathHelper.Lerp(-50, 0, (float)random.NextDouble()), // X between -50 and 50
                        MathHelper.Lerp(0, 100, (float)random.NextDouble()) // Y between 0 and 100
                        );
                    particle.Acceleration = 0.1f * new Vector2(0, (float)-random.NextDouble());
                }
                else
                {
                    particle.Position = new Vector2(X + 88, Y + 83);
                    particle.Velocity = new Vector2(
                        MathHelper.Lerp(0, 50, (float)random.NextDouble()), // X between -50 and 50
                        MathHelper.Lerp(0, 100, (float)random.NextDouble()) // Y between 0 and 100
                        );
                    particle.Acceleration = 0.1f * new Vector2(0, (float)-random.NextDouble());
                }
                particle.Color = Color.White;
                particle.Scale = 0.5f;
                particle.Life = 0.5f;
            };

            // Set the UpdateParticle method
            _particleSystem.UpdateParticle = (float deltaT, ref Particle particle) =>
            {
                particle.Velocity += deltaT * particle.Acceleration;
                particle.Position += deltaT * particle.Velocity;
                particle.Scale -= deltaT;
                particle.Life -= deltaT;
            };
        }

        public void SetX(int x)
        {
            _playerRectangle.X = x;
            this.X = x;
        }

        public void SetY(int y)
        {
            _playerRectangle.Y = y;
            this.Y = y;
        }

        public void UpdateParticles(GameTime gameTime)
        {
            _particleSystem.Update(gameTime);
        }

        // Draw the player
        public void Draw(SpriteBatch spriteBatch)
        {
            _particleSystem.Draw(spriteBatch);
            _animationManager.Draw(spriteBatch, PlayerRectangle);
        }

        // Change the currently playing animation based on the player's state
        public void UpdateAnimation(GameTime gameTime)
        {
            _animationManager.Update(gameTime);

            if (FacingRight)
            {
                if (FatalCollision) { _animationManager.Play(_animations["DyingRight"]); }
                else if (Speed == 0) { _animationManager.Play(_animations["IdleRight"]); }
                else { _animationManager.Play(_animations["MoveRight"]); }
            } 
            else
            {
                if (FatalCollision) { _animationManager.Play(_animations["DyingLeft"]); }
                else if (Speed == 0) { _animationManager.Play(_animations["IdleLeft"]); }
                else { _animationManager.Play(_animations["MoveLeft"]); }
            }
        }
    }
}
