using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TileGame
{
    public class Score: DrawableGameComponent
    {
        private Game1 g;
        private int score = 0;
        private string message;
        private SpriteFont basicFont;
        private Texture2D coinTexture;
        private double frameElapsedTime;
        private int currentFrame, totalFrame = 4;
        private bool isForward = true;
        private Rectangle frameRect;
        private Vector2 center, topRightCorner, messagePosition;
        private SpriteBatch spriteBatch;
        public Score(Game1 g): base(g)
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            this.g = g;
            basicFont = g.Content.Load<SpriteFont>("BasicFont");
            coinTexture = g.Content.Load<Texture2D>("Characters\\coin");
            frameRect = new Rectangle(0, 0, coinTexture.Width / (totalFrame + 1), coinTexture.Height);
            center = new Vector2(frameRect.Width * 0.5f, frameRect.Height * 0.5f);
            message = "x "+ score.ToString();
            topRightCorner = new Vector2(GraphicsDevice.Viewport.Width - frameRect.Width - basicFont.MeasureString(message).X - 35, frameRect.Height + 5);
            messagePosition = topRightCorner + new Vector2(1, 0) * frameRect.Width;
            messagePosition.X += 10;
        }
        public void AddScore(int add)
        {
            score = score + add;
            message = "x " + score.ToString();
            topRightCorner = new Vector2(GraphicsDevice.Viewport.Width - frameRect.Width - basicFont.MeasureString(message).X - 35, frameRect.Height + 5);
            messagePosition = topRightCorner + new Vector2(1, 0) * frameRect.Width;
            messagePosition.X += 10;
        }
        public int GetScore()
        {
            return score;
        }

        public override void Update(GameTime gameTime)
        {
            if (!g.player.isGameOver)
            {
                frameElapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (frameElapsedTime >= Character.frameTimeStep)
                {
                    if (isForward && currentFrame == totalFrame)
                    {
                        isForward = false;
                    }
                    else if (!isForward && currentFrame == 1)
                    {
                        isForward = true;
                    }
                    if (isForward)
                    {
                        currentFrame++;
                    }
                    else
                    {
                        currentFrame--;
                    }
                    frameRect.X = frameRect.Width * (currentFrame - 1);
                    frameElapsedTime = 0;
                    base.Update(gameTime);
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (!g.player.isGameOver)
            {
                base.Draw(gameTime);
                spriteBatch.Begin();
                spriteBatch.Draw(coinTexture, topRightCorner, frameRect, Color.White);
                spriteBatch.DrawString(basicFont, message, messagePosition, Color.White);
                spriteBatch.End();
            }
        }
    }
}
