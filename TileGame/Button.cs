using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace TileGame
{
    public class Button: DrawableGameComponent
    {
        private Game1 g;
        private SpriteBatch spriteBatch;
        private Texture2D whiteRectangle;
        private SpriteFont buttonFont;
        private Rectangle buttonRect;
        public string message, name;
        public bool isTouching, isActive, isHidden;
        public Button(Game1 g, string name, string message, Vector2 position): base(g)
        {
            this.g = g;
            this.name = name;
            this.message = message;
            buttonFont = Game.Content.Load<SpriteFont>("BasicFont");
            buttonRect = new Rectangle((int)position.X, (int)position.Y, (int)buttonFont.MeasureString(message).X + 5, (int)buttonFont.MeasureString(message).Y + 5);
            Game1.OnClick += OnClick;
        }
        private void OnClick()
        {
            if (isTouching)
            {
                isActive = false;
                if (name == "ToPlayModeButton")
                {
                    g.SetButtonActive(Game1.GameState.playMode, name);
                }
                else if (name == "ToTileModeButton")
                {
                    g.SetButtonActive(Game1.GameState.tileMode, name);
                }
                else if (name == "ToHitboxModeButton")
                {
                    g.SetButtonActive(Game1.GameState.hitboxMode, name);
                }
                else if (name == "ToEntityModeButton")
                {
                    g.entityTable.SetPageEntity(g.entityTable.page);
                    g.SetButtonActive(Game1.GameState.entityMode, name);
                }
                else if (name == "ToEntityRemoveModeButton")
                {
                    g.SetButtonActive(Game1.GameState.entityRemoveMode, name);
                }
                else if (name == "ToEntityUpButton")
                {
                    if (g.entityTable.page - 1 > -1)
                    {
                        g.entityTable.page -= 1;
                        g.entityTable.SetPageEntity(g.entityTable.page);
                    }
                    isActive = true;
                }
                else if (name == "ToEntityDownButton")
                {
                    if (g.entityTable.page + 1 < Entity.TotalEntity - 3)
                    {
                        g.entityTable.page += 1;
                        g.entityTable.SetPageEntity(g.entityTable.page);
                    }
                    isActive = true;
                }
                else if (name == "ToCharacterMode")
                {
                    g.SetButtonActive(Game1.GameState.characterMode, name);
                }
            }
        }
        protected override void LoadContent()
        {
            base.LoadContent();
            spriteBatch = new SpriteBatch(GraphicsDevice);

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
            if (isActive || isHidden)
            {
                if (Game1.IsWithinRectangle(g.mousePosition, buttonRect))
                {
                    isTouching = true;
                }
                else
                {
                    isTouching = false;
                }
            }
            else
            {
                isTouching = false;
            }
            base.Draw(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            if (isHidden == false)
            {
                spriteBatch.Begin();
                if (isActive)
                {
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
                }
                spriteBatch.DrawString(buttonFont, message, new Vector2(buttonRect.X + 2.5f, buttonRect.Y), Color.Black);
                spriteBatch.End();

                base.Draw(gameTime);
            }
        }
    }
}
