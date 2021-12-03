using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace TileGame
{
    public class InputField: DrawableGameComponent
    {
        private Game1 g;
        private SpriteBatch spriteBatch;
        private Texture2D whiteRectangle;
        private Rectangle buttonRect, submitRect;
        private bool isTouching, isPressed, isWriting, isSubmitBoxTouching, isSubmitted;
        private KeyboardState lastPressedKeys;
        private SpriteFont buttonFont;
        private string message = null;
        private string[] mapData;
        private int defaultRectWidht;
        private Character.CharacterBehaviour respectiveCharacter;
        private Game1.GameState currentGameState;

        public InputField(Game1 g, Vector2 position, Game1.GameState currentGameState, Character.CharacterBehaviour respectiveCharacter = null): base(g)
        {
            this.g = g;
            defaultRectWidht = 100;
            buttonRect = new Rectangle((int)position.X, (int)position.Y, defaultRectWidht, 25);
            submitRect = new Rectangle(buttonRect.X + buttonRect.Width + 5, buttonRect.Y, 25, 25);
            this.respectiveCharacter = respectiveCharacter;
            this.currentGameState = currentGameState;
            isSubmitted = false;
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            buttonFont = Game.Content.Load<SpriteFont>("BasicFont");

            whiteRectangle = new Texture2D(GraphicsDevice, 1, 1);
            whiteRectangle.SetData(new[] { Color.White });

        }
        protected override void UnloadContent()
        {
            base.UnloadContent();
            spriteBatch.Dispose();
            whiteRectangle.Dispose();
        }
        public override void Update(GameTime gameTime)
        {
            if (!isSubmitted) {
                if (Game1.IsWithinRectangle(g.mousePosition, buttonRect))
                {
                    isTouching = true;
                    if (g.clicked && isPressed == false)
                    {
                        isPressed = true;
                    }
                }
                else
                {
                    isTouching = false;
                    if (g.clicked && isPressed == true && isSubmitBoxTouching == false)
                    {
                        isPressed = false;
                        message = null;
                        isWriting = false;
                    }
                }
                if (isPressed)
                {
                    KeyboardState ks = Keyboard.GetState();
                    if (ks.GetPressedKeyCount() == 1)
                    {
                        if (!lastPressedKeys.IsKeyDown(ks.GetPressedKeys()[0]))
                        {
                            if (ks.GetPressedKeys()[0] == Keys.OemComma)
                            {
                                message += ", ";
                            }
                            else
                            {
                                int number = GetNumberFromKey(ks.GetPressedKeys()[0]);
                                if (number == -1)
                                {
                                    message += ks.GetPressedKeys()[0].ToString();
                                }
                                else
                                {
                                    message += number.ToString();
                                }
                            }
                        }
                    }
                    lastPressedKeys = ks;
                }
                if (message == null)
                {
                    buttonRect.Width = defaultRectWidht;
                    isSubmitBoxTouching = false;
                }
                else
                {
                    isWriting = true;
                    buttonRect.Width = (int)buttonFont.MeasureString(message).X;
                    if (Game1.IsWithinRectangle(g.mousePosition, submitRect))
                    {
                        isSubmitBoxTouching = true;
                        if (g.clicked)
                        {
                            isSubmitted = true;
                        }
                    }
                    else
                    {
                        isSubmitBoxTouching = false;
                    }
                }
                submitRect.X = buttonRect.X + buttonRect.Width + 5;
                base.Draw(gameTime);
            }
            else
            {
                if (currentGameState == Game1.GameState.characterMode)
                {
                    g.SetButtonActive(Game1.GameState.characterMode, "ToCharacterModeButton");
                    g.characterTable.SetCharacterPage(g.characterTable.page);
                    respectiveCharacter.respectiveData.extra = message;
                    respectiveCharacter.SetExtraSettings();
                }
                else if (currentGameState == Game1.GameState.createNewMapMode)
                {
                    mapData = message.Split(", ");
                    if (mapData.Length == 3)
                    {
                        g.CreateNewMap(mapData[0], int.Parse(mapData[1]), int.Parse(mapData[2]));
                    }
                    else
                    {
                        g.CreateNewMap(mapData[0]);
                    }
                    g.SetButtonActive(Game1.GameState.tileMode, "ToTileModeButton");
                }
                Dispose();
                g.Components.Remove(this);
            }
        }
        private int GetNumberFromKey(Keys key)
        {
            int keyVal = (int)key;
            int value = -1;
            if (keyVal >= (int)Keys.D0 && keyVal <= (int)Keys.D9)
            {
                value = (int)key - (int)Keys.D0;
            }
            else if (keyVal >= (int)Keys.NumPad0 && keyVal <= (int)Keys.NumPad9)
            {
                value = (int)key - (int)Keys.NumPad0;
            }
            return value;
        }
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            if (!isPressed) {
                if (isTouching)
                {
                    spriteBatch.Draw(whiteRectangle, buttonRect, Color.Salmon);
                }
                else
                {
                    spriteBatch.Draw(whiteRectangle, buttonRect, Color.White);
                }
            }
            else
            {
                spriteBatch.Draw(whiteRectangle, buttonRect, Color.Gray);
                if (isWriting)
                {
                    if (isSubmitBoxTouching)
                    {
                        spriteBatch.Draw(whiteRectangle, submitRect, Color.Salmon);
                    }
                    else
                    {
                        spriteBatch.Draw(whiteRectangle, submitRect, Color.White);
                    }
                }
                if (message != null)
                    spriteBatch.DrawString(buttonFont, message, new Vector2(buttonRect.X + 2.5f, buttonRect.Y), Color.Black);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
