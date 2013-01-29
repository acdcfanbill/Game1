using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace WindowsGame1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private GamePadState gamePadState;
        private KeyboardState keyboardState;
        private MouseState mouseState;
        SpriteBatch spriteBatch;
        SpriteFont scoreFont;
        private Texture2D target;
        private Texture2D cursor;
        private SoundEffect hit_sound;
        private SoundEffect miss_sound;
        private Vector2 targetPos;
        private Vector2 cursorPos;
        private Vector2 targetVel;
        private Random rand = new Random();
        private int clickCount;
        private int clickHits;
        private int difficulty;
        private bool mouseLeftWasDown;
        private bool mouseRightWasDown;

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

            //this.IsMouseVisible = true;
            targetPos.X = 20; targetPos.Y = 50;  //initialize postition of the target
            difficulty = 50; //can be set higher for a 'harder' game
            clickCount = 0;
            clickHits = 0;
            mouseLeftWasDown = false;
            mouseRightWasDown = false;

            //randomly initialize the velocity components of our target
            targetVel.X = randomVelocity()*difficulty; targetVel.Y = randomVelocity()*difficulty; 

            //Set the window title
            Window.Title = "Click the bullseye! - Right Click resets your game";
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
            target = Content.Load<Texture2D>("assets/textures/target");
            cursor = Content.Load<Texture2D>("assets/textures/cursor");
            hit_sound = Content.Load<SoundEffect>("assets/sounds/success");
            miss_sound = Content.Load<SoundEffect>("assets/sounds/failure");
            scoreFont = Content.Load<SpriteFont>("assets/fonts/scoreFont");

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
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
            // Update the state of the keyboard or gamepad
            gamePadState = GamePad.GetState(PlayerIndex.One);
            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();

            bool mouseLeftNowDown;
            bool mouseRightNowDown;

            // Allows the game to exit
            if (exitKeyPressed() == false)
            {
                // first, get the seconds since the last update.
                float secs = (float)gameTime.ElapsedGameTime.TotalSeconds;

                //Check for side collisions, reverse velocitiy if necessary.
                if (targetPos.X < 0 || targetPos.X > Window.ClientBounds.Width - target.Width)
                {
                    targetVel.X = 0 - targetVel.X;
                }
                if (targetPos.Y < 0 || targetPos.Y > Window.ClientBounds.Height - target.Height)
                {
                    targetVel.Y = 0 - targetVel.Y;
                }

                //now we canupdate the postition of the target
                targetPos.X = targetPos.X + targetVel.X * secs;
                targetPos.Y = targetPos.Y + targetVel.Y * secs;

                //update the position of the custom cursor.  The 12 and 5 adjustments are to
                //to compensate for the fact the 'pointer' part of the cursor is not top left
                cursorPos.X = mouseState.X - 12; cursorPos.Y = mouseState.Y - 5;

                //need to get the states of the mouse buttons.
                ButtonState lb = mouseState.LeftButton;
                ButtonState rb = mouseState.RightButton;
                mouseLeftNowDown = lb == ButtonState.Pressed;
                mouseRightNowDown = rb == ButtonState.Pressed;

                if (mouseLeftNowDown && !mouseLeftWasDown)
                {
                    //check for a hit
                    if (mouseHit())
                    {
                        //we have a Hit
                        clickHits++;
                        clickCount++;
                        hit_sound.Play();
                    }
                    else
                    {
                        //the click missed
                        clickCount++;
                        miss_sound.Play();
                    }
                }
                if (mouseRightNowDown && !mouseRightWasDown)
                {
                    //reset the game
                    clickCount = 0;
                    clickHits = 0;
                    targetVel.X = randomVelocity() * difficulty; targetVel.Y = randomVelocity() * difficulty;
                }

                //update mouse positions for next time
                mouseLeftWasDown = mouseLeftNowDown;
                mouseRightWasDown = mouseRightNowDown;

                base.Update(gameTime);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            float hitPercentage;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (clickCount != 0)
                hitPercentage = ((float)clickHits / clickCount) * 100;
            else
                hitPercentage = 0;

            // draw the spritebatch
            spriteBatch.Begin();
            spriteBatch.Draw(target, targetPos, Color.White);
            spriteBatch.Draw(cursor, cursorPos, Color.White);
            spriteBatch.DrawString(scoreFont, "Score: " + hitPercentage + "%", new Vector2(20,10),
                Color.DarkBlue, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// This is a function to check if the user is pressing the escape key or the back button
        /// if they are, we will exit for now.
        /// </summary>
        /// <returns>returns false if neither are pressed, exits if true</returns>
        protected bool exitKeyPressed()
        {
            // Check to see whether ESC was pressed on the keyboard or BACK was pressed on the controller.
            if (keyboardState.IsKeyDown(Keys.Escape) || gamePadState.Buttons.Back == ButtonState.Pressed)
            {
                //We can exit here, or if we get a menu in place, can do the menu instead
                Exit();
                return true;
            }
            return false;
        }
        /// <summary>
        /// This returns a random number between -1 and +1 for use as a velocity
        /// </summary>
        /// <returns>random float between -1 and +1</returns>
        protected float randomVelocity()
        {
            return 2 * ((float) rand.NextDouble() - (float) 0.5);
        }

        /// <summary>
        /// This checks to see if the mouse is over the bullseye of the target
        /// </summary>
        /// <returns>true if mouse is, false if mouse is not</returns>
        protected bool mouseHit()
        {
            if(mouseState.X >= targetPos.X + 52 && mouseState.X <= targetPos.X + 68)
                if(mouseState.Y >= targetPos.Y + 52 && mouseState.Y <= targetPos.Y + 68)
                    return true;
            return false;

        }
    }
}