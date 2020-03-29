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
    // Ball object
    public class Ball
    {
        protected AnimationManager _animationManager;
        protected Dictionary<string, Animation> _animations;
        private Rectangle _ballRectangle;
        protected ParticleSystem _trailParticleSystem;
        protected ParticleSystem _explosionParticleSystem;

        Random random = new Random();

        public Vector2 Center { get; set; }

        public float Y { get; set; }

        public float Speed { get; private set; }

        public bool PlayerCollision { get; set; }

        public bool GroundCollision { get; set; }

        public Ball(Vector2 ballOrigin, Dictionary<string, Animation> animations, Texture2D trailSprite, Texture2D explosionParticle)
        {
            PlayerCollision = false;
            GroundCollision = false;

            Center = new Vector2(ballOrigin.X + 50, ballOrigin.Y + 50);
            Y = ballOrigin.Y + 25;
            Speed = 5f;

            _ballRectangle = new Rectangle((int)ballOrigin.X + 25, (int)ballOrigin.Y + 25, 50, 50);

            _animations = animations;
            _animationManager = new AnimationManager(_animations.First().Value);

            _trailParticleSystem = new ParticleSystem(100, trailSprite);
            _trailParticleSystem.Emitter = new Vector2(Center.X, Center.Y);
            _trailParticleSystem.SpawnPerFrame = 4;
            _trailParticleSystem.Opacity = 0.08f;

            // Set the SpawnParticle method
            _trailParticleSystem.SpawnParticle = (ref Particle particle) =>
            {

                particle.Position = new Vector2(Center.X - 11, Center.Y - 50);
                particle.Velocity = new Vector2(
                    MathHelper.Lerp(0, 0, (float)random.NextDouble()), // X between -50 and 50
                    MathHelper.Lerp(0, 100, (float)random.NextDouble()) // Y between 0 and 100
                    );
                particle.Acceleration = 0.1f * new Vector2(0, (float)-random.NextDouble());
                
                particle.Color = Color.White;
                particle.Scale = 0.8f;
                particle.Life = 0.3f;
            };

            // Set the UpdateParticle method
            _trailParticleSystem.UpdateParticle = (float deltaT, ref Particle particle) =>
            {
                particle.Velocity += deltaT * particle.Acceleration;
                particle.Position += deltaT * particle.Velocity;
                particle.Position += new Vector2(0.5f, 0);
                particle.Scale -= deltaT * 2;
                particle.Life -= deltaT;
            };

            _explosionParticleSystem = new ParticleSystem(100, explosionParticle);
            _explosionParticleSystem.Emitter = new Vector2(Center.X, Center.Y);
            _explosionParticleSystem.SpawnPerFrame = 8;
            _explosionParticleSystem.Opacity = 0.3f;

            // Set the SpawnParticle method
            _explosionParticleSystem.SpawnParticle = (ref Particle particle) =>
            {

                particle.Position = new Vector2(Center.X - 11, Center.Y - 50);
                particle.Velocity = new Vector2(
                    MathHelper.Lerp(-300, 300, (float)random.NextDouble()), // X between -50 and 50
                    MathHelper.Lerp(-100, 100, (float)random.NextDouble()) // Y between 0 and 100
                    );
                particle.Acceleration = 0.1f * new Vector2(0, (float)-random.NextDouble());

                particle.Color = Color.White;
                particle.Scale = 0.8f;
                particle.Life = 0.3f;
            };

            // Set the UpdateParticle method
            _explosionParticleSystem.UpdateParticle = (float deltaT, ref Particle particle) =>
            {
                particle.Velocity += deltaT * particle.Acceleration;
                particle.Position += deltaT * particle.Velocity;
                particle.Position += new Vector2(0.5f, 0);
                particle.Scale -= deltaT * 5;
                particle.Life -= deltaT;
            };
        }

        public void SetY(float y)
        {
            Center = new Vector2(this.Center.X, y + 50);
            Y = y;
            _ballRectangle.Y = (int)y;
        }

        public void UpdateParticles(GameTime gameTime)
        {
            _trailParticleSystem.Update(gameTime);
            if (PlayerCollision || GroundCollision) _explosionParticleSystem.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _trailParticleSystem.Draw(spriteBatch);
            if (PlayerCollision || GroundCollision) _explosionParticleSystem.Draw(spriteBatch);
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
