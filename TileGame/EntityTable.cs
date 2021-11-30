using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
namespace TileGame
{
    public class EntityTable : DrawableGameComponent
    {
        private SpriteBatch spriteBatch;
        public Entity[] entityDetails;
        private Texture2D whiteRectangle;
        public int page;
        private Game1 g;
        private Rectangle tableRect;
        private SelectEntityRect[] descriptionRect;
        private Button toEntityUpButton, toEntityDownButton;
        private bool isHidden;
        public EntityTable (Game1 g): base(g)
        {
            this.g = g;
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
                        g.SetButtonActive(Game1.GameState.entityPlantMode, "");
                        g.PlantEntityOnTileMap(page + i);
                    }
                }
            }
        }
        protected override void LoadContent()
        {
            base.LoadContent();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            entityDetails = new Entity[4];
            page = 0;

            whiteRectangle = new Texture2D(GraphicsDevice, 1, 1);
            whiteRectangle.SetData(new[] { Color.White });

            tableRect = new Rectangle((int)(GraphicsDevice.Viewport.Width * 0.87f), (int)(GraphicsDevice.Viewport.Height * 0.1f), (int)(GraphicsDevice.Viewport.Width * 0.1f), (int)(GraphicsDevice.Viewport.Height * 0.735f));

            descriptionRect = new SelectEntityRect[4];
            for (int i = 0; i < descriptionRect.Length; i++)
            {
                descriptionRect[i] = new SelectEntityRect(new Rectangle(tableRect.X + 10, tableRect.Y + i * 200 + 10, tableRect.Width - 20, tableRect.Y + 60));
            }

            toEntityUpButton = new Button(g, "ToEntityUpButton", "Up", new Vector2(tableRect.X, tableRect.Y - 25));
            toEntityUpButton.isActive = true;
            toEntityUpButton.isHidden = true;
            g.UIButtons.Add(toEntityUpButton);
            g.Components.Add(toEntityUpButton);

            toEntityDownButton = new Button(g, "ToEntityDownButton", "Down", new Vector2(tableRect.X, tableRect.Y + tableRect.Height + 5));
            toEntityDownButton.isActive = true;
            toEntityDownButton.isHidden = true;
            g.UIButtons.Add(toEntityDownButton);
            g.Components.Add(toEntityDownButton);

        }
        public void SetPageEntity(int page)
        {
            for (int i = 0; i < entityDetails.Length; i++)
            {
                if (entityDetails[i] != null)
                {
                    g.Components.Remove(entityDetails[i]);
                    entityDetails[i].Dispose();
                    entityDetails[i] = null;
                }
                entityDetails[i] = new Entity(g, GetEntityStatus(page + i));
                g.Components.Add(entityDetails[i]);
            }
        }
        public class EntityAnimationStatus
        {
            public bool isAnimated;
            public int totalFrame, ID;
            public string name;

            public EntityAnimationStatus(int ID, string name, bool isAnimated = false, int totalFrame = 1)
            {
                this.isAnimated = isAnimated;
                this.totalFrame = totalFrame;
                this.ID = ID;
                this.name = name;
            }
        }
        public static EntityAnimationStatus GetEntityStatus(int index)
        {
            if (index > Entity.TotalEntity)
                return new EntityAnimationStatus(0, "tree1");
            switch (index)
            {
                case 0:
                    return new EntityAnimationStatus(0, "tree1");
                case 1:
                    return new EntityAnimationStatus(1, "shroom1");
                case 2:
                    return new EntityAnimationStatus(2, "shroom2");
                case 3:
                    return new EntityAnimationStatus(3, "grass", true, 3);
            }
            return new EntityAnimationStatus(0, "tree1");
        }
        protected override void UnloadContent()
        {
            base.UnloadContent();
            spriteBatch.Dispose();
            whiteRectangle.Dispose();
        }
        public class SelectEntityRect
        {
            public Rectangle selectRect;
            public bool isTouching;

            public SelectEntityRect (Rectangle selectRect)
            {
                this.selectRect = selectRect;
            }
        }
        public override void Update(GameTime gameTime)
        {
            if (g.gameState == Game1.GameState.entityMode || g.gameState == Game1.GameState.entityPlantMode)
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
                if (entityDetails[0] != null)
                {
                    for(int i = 0; i < entityDetails.Length; i++)
                    {
                        if (entityDetails[i] != null)
                        {
                            entityDetails[i].Dispose();
                            entityDetails[i] = null;
                        }
                    }
                }
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
                    spriteBatch.Draw(entityDetails[i].entityDetails.entityWholeTexture, new Vector2(descriptionRect[i].selectRect.X + descriptionRect[i].selectRect.Width * 0.5f, descriptionRect[i].selectRect.Y + descriptionRect[i].selectRect.Height * 0.5f) - entityDetails[i].entityDetails.partialCenter, entityDetails[i].entityDetails.frameRect, Color.White);
                }
                spriteBatch.End();
                base.Draw(gameTime);
            }
        }
    }
}
