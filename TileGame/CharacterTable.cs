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
        private SelectCharacterRect[] descriptionRect;
        public int page;
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

            descriptionRect = new SelectCharacterRect[4];
            for (int i = 0; i < descriptionRect.Length; i++)
            {
                descriptionRect[i] = new SelectCharacterRect(new Rectangle(tableRect.X + 10, tableRect.Y + i * 200 + 10, tableRect.Width - 20, tableRect.Y + 60));
            }

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

            Game1.OnClick += OnClick;
        }
        private void OnClick()
        {
            if (isHidden == false)
            {
                for (int i = 0; i < descriptionRect.Length; i++)
                {
                    if (descriptionRect[i].isTouching)
                    {
                        g.SetButtonActive(Game1.GameState.characterPlantMode, "");
                        g.PlantCharacterOnTileMap(page + i);
                    }
                }
            }
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
                for (int i = 0; i < descriptionRect.Length; i++)
                {
                    if (Game1.IsWithinRectangle(g.mousePosition, descriptionRect[i].selectRect))
                    {
                        descriptionRect[i].isTouching = true;
                    }
                    else
                    {
                        descriptionRect[i].isTouching = false;
                    }
                }
            }
            else
            {
                isHidden = true;
                if (descriptionRect[0].character != null)
                {
                    for (int i = 0; i < descriptionRect.Length; i++)
                    {
                        if (descriptionRect[i].character != null)
                        {
                            g.Components.Remove(descriptionRect[i].character);
                            descriptionRect[i].character.Dispose();
                            descriptionRect[i].character = null;
                        }
                    }
                }
            }
        }
        public void SetCharacterPage(int page)
        {
            this.page = page;
            for (int i = 0; i < descriptionRect.Length; i++) {
                if (descriptionRect[i].character != null)
                {
                    g.Components.Remove(descriptionRect[i].character);
                    descriptionRect[i].character.Dispose();
                }
                if (i < Character.totalCharacter)
                {
                    descriptionRect[i].SetCharacter(new Character(g, page + i, new Vector2(descriptionRect[i].selectRect.X + descriptionRect[i].selectRect.Width * 0.5f, descriptionRect[i].selectRect.Y + descriptionRect[i].selectRect.Height * 0.5f)));
                    g.Components.Add(descriptionRect[i].character);
                }
            }
        }
        public class SelectCharacterRect
        {
            public Rectangle selectRect;
            public bool isTouching;
            public Character character;

            public SelectCharacterRect (Rectangle selectRect)
            {
                this.selectRect = selectRect;
            }

            public void SetCharacter(Character character)
            {
                this.character = new Character(character);  
            }
        }
        public override void Draw(GameTime gameTime)
        {
            if (isHidden == false)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(whiteRectangle, tableRect, Color.Gray);
                for (int i = 0; i < descriptionRect.Length; i++)
                {
                    if (descriptionRect[i].isTouching)
                    {
                        spriteBatch.Draw(whiteRectangle, descriptionRect[i].selectRect, Color.Salmon);
                    }
                    else
                    {
                        spriteBatch.Draw(whiteRectangle, descriptionRect[i].selectRect, Color.White);
                    }
                }
                spriteBatch.End();
                base.Draw(gameTime);
            }
        }
    }
}
