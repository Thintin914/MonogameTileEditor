using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TileGame
{
    class SpreadAttack : DrawableGameComponent
    {
        private Game1 g;
        private SpriteBatch spriteBatch;
        private Texture2D texture;
        private int currentFrame, totalFrame, totalSpreadPower;
        private Rectangle frameRect;
        private Vector2 center, position;
        public SpreadAttack (Game1 g, Vector2 position, int spreadPower, int textureID): base(g)
        {
            this.g = g;
            texture = g.Content.Load<Texture2D>("SpreadAttacks\\spreadAttack" + textureID);
            SetFrame(textureID);
            frameRect = new Rectangle(0, 0, texture.Width / (totalFrame + 1), texture.Height);
            center = new Vector2(frameRect.Width * 0.5f, frameRect.Height * 0.5f);
            this.position = position;
            totalSpreadPower = spreadPower;
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
                    totalFrame = 4;
                    break;
                default:
                    totalFrame = 1;
                    break;
            }
        }
    }
}
