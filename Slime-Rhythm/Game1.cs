using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
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

        Song song;
        Player player;
        BallManager ballManager;

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
            // TODO: Add your initialization logic here
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 700;
            graphics.ApplyChanges();

            friction = 0.015f;

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

            // Load ball animations
            var ballAnimations = new Dictionary<string, Animation>()
            {
                { "Falling", new Animation(Content.Load<Texture2D>("Ball/Ball1-Falling"), 2) },
                { "Collision", new Animation(Content.Load<Texture2D>("Ball/Ball1-Collision"), 3, false) }
            };

            ballAnimations["Falling"].FrameSpeed = 625f;
            ballAnimations["Collision"].FrameSpeed = 20f;

            // load song
            song = Content.Load<Song>("Game1Song");

            // create player object
            player = new Player(new Vector2((GraphicsDevice.Viewport.Width / 2) - 50, GraphicsDevice.Viewport.Height - 100), playerAnimations);
            
            // create ball manager
            ballManager = new BallManager(ballAnimations, random, GraphicsDevice.Viewport.Height);

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
            if (player.FatalCollision == false)
            {
                if (newKeyboardState.IsKeyDown(Keys.Right))
                { // move right
                    player.Speed += player.Acceleration;

                    if (player.Speed > player.MaxSpeed) player.Speed = player.MaxSpeed;

                    player.FacingRight = true;
                }

                if (newKeyboardState.IsKeyDown(Keys.Left)) // move left
                {
                    player.Speed -= player.Acceleration;

                    if (player.Speed < 0 - player.MaxSpeed) player.Speed = 0 - player.MaxSpeed;

                    player.FacingRight = false;
                }
            }

            // keep player in bounds
            if (player.X < 0) // left side
            {
                player.X = 0;
                player.Speed *= -0.3f;
            }
            if (player.X > GraphicsDevice.Viewport.Width - player.PlayerRectangle.Width) // right side
            {
                player.X = GraphicsDevice.Viewport.Width - player.PlayerRectangle.Width;
                player.Speed *= -0.3f;
            }

            // reduce speed based on friction if key is not held
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

            // move player
            player.X += player.Speed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            player.SetX((int)player.X);

            // update balls
            ballManager.Update(gameTime);

            // test for collision with player and ball
            if (ballManager.TestForPlayerCollision(player.PlayerRectangle))
            {
                player.FatalCollision = true;
                ballManager.Stop();
            }

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

            // draw player
            player.Draw(spriteBatch);

            // draw balls
            ballManager.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
