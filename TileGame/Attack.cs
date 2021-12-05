using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace TileGame
{
    public class Attack: DrawableGameComponent
    {
        private Game1 g;
        public Vector2 currentPosition;
        private Vector2 direction, center;
        public Texture2D texture;
        public bool isActive;
        private bool isForward = true;
        private int currentFrame, totalFrame, currentTextureID = 0;
        public Rectangle attackRect;
        private Rectangle frameRect;
        private double frameElapsedTime;
        private SpriteBatch spriteBatch;
        private float currentRange, totalRange;
        private SpriteEffects flipside;
        public Attack(Game1 g): base(g)
        {
            this.g = g;
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }
        private void SetFrame(int ID)
        {
            switch (ID)
            {
                case 1:
                    totalFrame = 3;
                    break;
                case 2:
                    totalFrame = 1;
                    break;
                default:
                    totalFrame = 1;
                    break;
            }
        }
        public void SetAttack(Vector2 startPosition, Vector2 endPosition, float range, int textureID)
        {
            if (currentTextureID != textureID)
            {
                texture = g.Content.Load<Texture2D>("Attacks\\attack" + textureID);
                SetFrame(textureID);
                frameRect = new Rectangle(0, 0, texture.Width / (totalFrame + 1), texture.Height);
                center = new Vector2(frameRect.Width * 0.5f, frameRect.Height * 0.5f);
            }
            isActive = true;
            currentPosition = startPosition;
            direction = Vector2.Normalize(endPosition - startPosition);
            currentFrame = 0;
            currentRange = 0;
            totalRange = range + 40;
            attackRect = new Rectangle((int)(currentPosition.X - center.X - 5), (int)(currentPosition.Y - center.Y - 5), (int)(frameRect.Width + 10), frameRect.Height + 10);
            if (endPosition.X > startPosition.X)
            {
                flipside = SpriteEffects.None;
            }
            else
            {
                flipside = SpriteEffects.FlipHorizontally;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (isActive)
            {
                if (totalFrame > 1)
                {
                    frameElapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (frameElapsedTime >= Character.frameTimeStep)
                    {
                        if (isForward && currentFrame + 1 == totalFrame)
                        {
                            isForward = false;
                        }
                        else if (!isForward && currentFrame == 0)
                        {
                            isForward = true;
                        }
                        if (isForward)
                        {
                            currentFrame += 1;
                        }
                        else
                        {
                            currentFrame -= 1;
                        }
                        frameRect.X = currentFrame * frameRect.Width;
                        frameElapsedTime = 0;
                        base.Update(gameTime);
                    }
                }
                if (currentRange < totalRange)
                {
                    currentRange += 6;
                    currentPosition += direction * 6;
                    attackRect.X = (int)(currentPosition.X - center.X - 5);
                    attackRect.Y = (int)(currentPosition.Y -center.Y - 5);
                }
                else
                {
                    isActive = false;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (isActive)
            {
                if (texture != null)
                {
                    base.Draw(gameTime);
                    spriteBatch.Begin();
                    spriteBatch.Draw(texture, currentPosition - center, frameRect, Color.White, 0, Vector2.Zero, 1, flipside, 0);
                    spriteBatch.End();
                }
            }
        }
    }
}
