using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Slime_Rhythm;
using System;
using System.Collections.Generic;

namespace SlimeRhythm
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Random random = new Random();

        Texture2D background;
        Texture2D victory;
        Texture2D loss;
        Texture2D progressBar;
        Texture2D progressParticle;
        ParticleSystem progressParticleSystem;
        int progressX = 0;

        Song song;
        Player player;
        BallManager ballManager;

        bool gameWon;
        bool gameLost;
        float friction;

        KeyboardState oldKeyboardState;
        KeyboardState newKeyboardState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 700;
            graphics.ApplyChanges();

            friction = 0.018f;
            gameLost = false;
            gameWon = false;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load background
            background = Content.Load<Texture2D>("Game1-Background2");

            // Load victory and loss text
            victory = Content.Load<Texture2D>("Victory");
            loss = Content.Load<Texture2D>("Loss");

            // Load progress bar
            progressBar = Content.Load<Texture2D>("ProgressBar");
            progressParticle = Content.Load<Texture2D>("Player/Slime-Particle");
            progressParticleSystem = new ParticleSystem(1000, progressParticle);
            progressParticleSystem.Emitter = new Vector2(0, 100);
            progressParticleSystem.SpawnPerFrame = 1;
            progressParticleSystem.Opacity = 0.2f;

            // Set the SpawnParticle method
            progressParticleSystem.SpawnParticle = (ref Particle particle) =>
            {

                particle.Position = new Vector2(progressX, 100);
                particle.Velocity = new Vector2(
                    MathHelper.Lerp(0, 0, (float)random.NextDouble()), // X between -50 and 50
                    MathHelper.Lerp(0, 100, (float)random.NextDouble()) // Y between 0 and 100
                    );
                particle.Acceleration = 0.1f * new Vector2(0, (float)-random.NextDouble());

                particle.Color = Color.Gold;
                particle.Scale = 0.1f;
                particle.Life = 0.4f;
            };

            // Set the UpdateParticle method
            progressParticleSystem.UpdateParticle = (float deltaT, ref Particle particle) =>
            {
                particle.Velocity += deltaT * particle.Acceleration;
                particle.Position += deltaT * particle.Velocity;
                //particle.Scale -= deltaT * 2;
                particle.Life -= deltaT;
            };

            // Load player animations
            var playerAnimations = new Dictionary<string, Animation>()
            {
                { "IdleRight", new Animation(Content.Load<Texture2D>("Player/KingSlime-IdleRight"), 4) },
                { "IdleLeft", new Animation(Content.Load<Texture2D>("Player/KingSlime-IdleLeft"), 4) },
                { "MoveRight", new Animation(Content.Load<Texture2D>("Player/KingSlime-MoveRight"), 4) },
                { "MoveLeft", new Animation(Content.Load<Texture2D>("Player/KingSlime-MoveLeft"), 4) },
                { "DyingRight", new Animation(Content.Load<Texture2D>("Player/KingSlime-DyingRight"), 6, false) },
                { "DyingLeft", new Animation(Content.Load<Texture2D>("Player/KingSlime-DyingLeft"), 6, false) }
            };
            var slimeParticle = Content.Load<Texture2D>("Player/Slime-Particle");

            // Load ball animations
            var ballAnimations = new Dictionary<string, Animation>()
            {
                { "Falling", new Animation(Content.Load<Texture2D>("Ball/Ball1-Falling"), 2) },
                { "Collision", new Animation(Content.Load<Texture2D>("Ball/Ball1-Collision"), 3, false) }
            };
            var trailSprite = Content.Load<Texture2D>("Ball/Bomb-Trail");
            var explosionParticle = Content.Load<Texture2D>("Ball/Explosion-Particle");

            // correct frame speeds for ball animations
            ballAnimations["Falling"].FrameSpeed = 625f;
            ballAnimations["Collision"].FrameSpeed = 20f;

            // load song
            song = Content.Load<Song>("Game1Song");

            // create player object
            player = new Player(new Vector2((GraphicsDevice.Viewport.Width / 2) - 50, GraphicsDevice.Viewport.Height - 100), playerAnimations, slimeParticle);
            
            // create ball manager
            ballManager = new BallManager(ballAnimations, random, GraphicsDevice.Viewport.Height, trailSprite, explosionParticle);

            // begin ball spawning / music sequence
            ballManager.Begin(song);
            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Take and Parse Input
            newKeyboardState = Keyboard.GetState();

            // exit game
            if (newKeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            // player movement
            if (player.FatalCollision == false) // if the player has not died
            {
                if (newKeyboardState.IsKeyDown(Keys.Right)) // move right
                { 
                    player.Speed += player.Acceleration; // increase player's speed based on acceleration

                    if (player.Speed > player.MaxSpeed) player.Speed = player.MaxSpeed; // cap speed to player's max

                    player.FacingRight = true; // make the player face right
                }

                if (newKeyboardState.IsKeyDown(Keys.Left)) // move left
                {
                    player.Speed -= player.Acceleration; // decrease player's speed based on acceleration

                    if (player.Speed < 0 - player.MaxSpeed) player.Speed = 0 - player.MaxSpeed; // cap speed to player's max

                    player.FacingRight = false; // make the player face left
                }
            }

            // keep player in bounds
            if (player.X < 0) // left side
            {
                player.X = 0;
                player.Speed *= -0.3f;  // bounce off the wall, retaining some speed
            }
            if (player.X > GraphicsDevice.Viewport.Width - player.PlayerRectangle.Width) // right side
            {
                player.X = GraphicsDevice.Viewport.Width - player.PlayerRectangle.Width;
                player.Speed *= -0.3f;  // bounce off the wall, retaining some speed
            }

            // reduce speed based on friction if a movement key is not held
            if (!newKeyboardState.IsKeyDown(Keys.Right) && !newKeyboardState.IsKeyDown(Keys.Left))
            {
                if (player.Speed > 0)
                {
                    player.Speed -= friction;
                    if (player.Speed < 0) player.Speed = 0;
                }
                else if (player.Speed < 0)
                {
                    player.Speed += friction;
                    if (player.Speed > 0) player.Speed = 0;
                }
            }

            // update balls
            ballManager.Update(gameTime);

            // move player
            player.X += player.Speed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            player.SetX((int)player.X);

            // update particles
            player.UpdateParticles(gameTime);

            // test for collision with player and ball
            if (ballManager.TestForPlayerCollision(player.PlayerRectangle))
            {
                player.FatalCollision = true;
                gameLost = true;
                ballManager.Stop();
            }

            // progress bar
            progressX = (int)((gameTime.TotalGameTime.TotalSeconds / 80) * GraphicsDevice.Viewport.Width);
            progressParticleSystem.Update(gameTime);

            // test for victory
            if (ballManager.MusicCompleted && !player.FatalCollision) gameWon = true;

            // fade out music if the player is dead
            if (player.FatalCollision)
            {
                if (MediaPlayer.Volume > 0) MediaPlayer.Volume -= 0.005f;
            }

            oldKeyboardState = newKeyboardState;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // update animations
            player.UpdateAnimation(gameTime);
            ballManager.UpdateAnimations(gameTime);

            // spriteBatch Begin arguments from Stack Overflow post https://stackoverflow.com/questions/25145377/xna-blurred-sprites-when-scaled
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            // draw background
            spriteBatch.Draw(background, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);

            // draw progress bar
            if (!gameLost)
            {
                spriteBatch.Draw(progressBar, new Rectangle(0, 0, progressX,
                                                        GraphicsDevice.Viewport.Height), Color.White * 0.3f);
                progressParticleSystem.Draw(spriteBatch);
            }

            // draw loss or victory text
            if (gameLost) spriteBatch.Draw(loss, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            if (gameWon) spriteBatch.Draw(victory, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);

            // draw player
            player.Draw(spriteBatch);

            // draw balls
            ballManager.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
