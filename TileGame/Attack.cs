using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TileGame
{
    class Attack: DrawableGameComponent
    {
        private Game1 g;
        private Vector2 position;
        private Texture2D texture;
        private float direction, distance;
        public bool isActive;
        private bool isForward = true;
        private int currentFrame, totalFrame;
        private Rectangle frameRect;
        private double frameElapsedTime;
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

        public void SetAttack(float direction, float distance, Vector2 startPosition)
        {
            isActive = true;
            this.direction = direction % 1;
            this.distance = distance;
            position = startPosition;
        }

        private Vector2 MoveToward(float direction, Vector2 position)
        {
            return new Vector2(MathF.Sin(direction), MathF.Cos(direction));
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
                position += MoveToward(direction, position) * 0.5f;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (isActive)
            {
                base.Draw(gameTime);
                spriteBatch.Begin();
                spriteBatch.Draw(texture, position, frameRect, Color.White);
                spriteBatch.End();
            }
        }
    }
}
