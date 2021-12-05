using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TileGame
{
    class Particle: DrawableGameComponent
    {
        private Texture2D texture;
        private Random rand;
        private Vector2 position;
        private double curve;
        private SpriteBatch spriteBatch;
        private float rotate;
        private Color color;
        public Particle(Game1 g): base(g)
        {
            rand = new Random();
            SetStartPosition();
        }
        protected override void LoadContent()
        {
            base.LoadContent();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            texture = Game.Content.Load<Texture2D>("Particles\\particle1");
        }
        private void SetStartPosition()
        {
            position = new Vector2(GraphicsDevice.Viewport.Width, rand.Next(0, GraphicsDevice.Viewport.Height));
            curve = rand.Next(-20, -2);
            color = new Color(rand.Next(255), rand.Next(255), rand.Next(255));
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (position.X > 0)
            {
                position.X += (float)curve * 0.5f;
                rotate = (rotate + (float)curve * 0.01f) % 360;
                position.Y += MathF.Sin((float)(gameTime.TotalGameTime.TotalSeconds - curve) * 10) * (float)curve * 0.5f;
            }
            else
            {
                SetStartPosition();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            spriteBatch.Begin();
            spriteBatch.Draw(texture, position, null, color, rotate, Vector2.Zero, 1, SpriteEffects.None, 1);
            spriteBatch.End();
        }
    }
}
