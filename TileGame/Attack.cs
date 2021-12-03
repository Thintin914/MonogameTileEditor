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
        private Vector2 startPosition, endPosition, currentPosition;
        private float range, distance, lerpPercentageRequired;
        private Texture2D texture;
        public bool isActive;
        private bool isForward = true;
        private int currentFrame, totalFrame;
        private Rectangle frameRect;
        private double frameElapsedTime, currentLerpPercentage;
        private SpriteBatch spriteBatch;
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
            this.startPosition = startPosition;
            currentPosition = startPosition;
            this.endPosition = endPosition;
            this.range = range;
            distance = Character.CharacterBehaviour.GetDistance(startPosition, endPosition);
            lerpPercentageRequired = range / distance;
            currentLerpPercentage = 0;
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
                if (currentLerpPercentage < lerpPercentageRequired)
                {
                    currentLerpPercentage += 0.01f;
                    currentPosition = Vector2.Lerp(startPosition, endPosition, (float)currentLerpPercentage);
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
