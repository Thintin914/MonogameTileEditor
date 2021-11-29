using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TileGame
{
    public class CharacterTable: DrawableGameComponent
    {
        private Game1 g;
        private SpriteBatch spriteBatch;
        private Texture2D whiteRectangle;
        private Rectangle tableRect;
        private bool isHidden;
        private Button toCharacterUpButton, toCharacterDownButton;
        public CharacterTable(Game1 g): base(g)
        {
            this.g = g;
        }
        protected override void LoadContent()
        {
            base.LoadContent();
            spriteBatch = new SpriteBatch(GraphicsDevice);

            whiteRectangle = new Texture2D(GraphicsDevice, 1, 1);
            whiteRectangle.SetData(new[] { Color.White });

            tableRect = new Rectangle((int)(GraphicsDevice.Viewport.Width * 0.87f), (int)(GraphicsDevice.Viewport.Height * 0.1f), (int)(GraphicsDevice.Viewport.Width * 0.1f), (int)(GraphicsDevice.Viewport.Height * 0.735f));

            toCharacterUpButton = new Button(g, "ToCharacterUpButton", "Up", new Vector2(tableRect.X, tableRect.Y - 25));
            toCharacterUpButton.isActive = true;
            toCharacterUpButton.isHidden = true;
            g.UIButtons.Add(toCharacterUpButton);
            g.Components.Add(toCharacterUpButton);

            toCharacterDownButton = new Button(g, "ToCharacterDownButton", "Down", new Vector2(tableRect.X, tableRect.Y + tableRect.Height + 5));
            toCharacterDownButton.isActive = true;
            toCharacterDownButton.isHidden = true;
            g.UIButtons.Add(toCharacterDownButton);
            g.Components.Add(toCharacterDownButton);

        }
        protected override void UnloadContent()
        {
            base.UnloadContent();
            spriteBatch.Dispose();
            whiteRectangle.Dispose();
        }
        public override void Update(GameTime gameTime)
        {
            if (g.gameState == Game1.GameState.characterMode)
            {
                base.Update(gameTime);
                isHidden = false;
            }
            else
            {
                isHidden = true;
            }
        }
        public override void Draw(GameTime gameTime)
        {
            if (isHidden == false)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(whiteRectangle, tableRect, Color.Gray);
                spriteBatch.End();
                base.Draw(gameTime);
            }
        }
    }
}
