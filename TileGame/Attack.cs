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
        private Vector2 currentPosition, direction;
        private Texture2D texture;
        public bool isActive;
        private bool isForward = true;
        private int currentFrame, totalFrame;
        private Rectangle frameRect;
        private double frameElapsedTime;
        private SpriteBatch spriteBatch;
        private float currentRange, totalRange;
        public Attack(Game1 g): base(g)
        {
            this.g = g;
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            texture = g.Content.Load<Texture2D>("Attacks\\attack1");
            currentFrame = 0;
            totalFrame = 3;
            frameRect = new Rectangle(0, 0, texture.Width / (totalFrame + 1), texture.Height);
        }

        public void SetAttack(Vector2 startPosition, Vector2 endPosition, float range)
        {
            isActive = true;
            currentPosition = startPosition;
            direction = Vector2.Normalize(endPosition - startPosition);
            currentRange = 0;
            totalRange = range;
        }

        public override void Update(GameTime gameTime)
        {
            if (isActive)
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
                if (currentRange < totalRange)
                {
                    currentRange += 4;
                    currentPosition += direction * 4;
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
                base.Draw(gameTime);
                spriteBatch.Begin();
                spriteBatch.Draw(texture, currentPosition, frameRect, Color.White);
                spriteBatch.End();
            }
        }
    }
}
