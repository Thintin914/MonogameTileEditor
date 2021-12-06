using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TileGame
{
    public class SpreadAttack : DrawableGameComponent
    {
        private Game1 g;
        private SpriteBatch spriteBatch;
        public Texture2D texture;
        private int currentFrame, totalFrame;
        public int totalMapSize;
        public Rectangle frameRect;
        public Vector2 center;
        private double frameElapsedTime;
        private bool isForward = true;
        public bool isActive;

        private List<int> expandedIndex = new List<int>();
        private List<int> expandedIndexPower = new List<int>();
        public int[] spreadedTileIndex;
        public SpreadAttack (Game1 g): base(g)
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            this.g = g;
        }
        public void SetSpreadAttack(int gridIndex, int spreadPower, int textureID)
        {
            texture = g.Content.Load<Texture2D>("SpreadAttacks\\spreadAttack" + textureID);
            SetFrame(textureID);
            frameRect = new Rectangle(0, 0, texture.Width / (totalFrame + 1), texture.Height);
            center = new Vector2(frameRect.Width * 0.5f, frameRect.Height * 0.5f);
            totalMapSize = g.currentMap.x * g.currentMap.y;

            expandedIndex.Clear();
            Spread(gridIndex, spreadPower);
            expandedIndexPower.Clear();
            WaitSpread();
        }

        private async void WaitSpread()
        {
            isActive = true;
            Task addTask = PerformSpreading();
            await addTask;

            expandedIndex.Clear();

            await PerformRemoving();

            isActive = false;
            Array.Clear(spreadedTileIndex, 0, spreadedTileIndex.Length);
        }
        private async Task PerformSpreading()
        {
            spreadedTileIndex = new int[expandedIndex.Count];
            int total = spreadedTileIndex.Length / 6;
            for (int i = 0; i < total; i++)
            {
                for (int k = i * 6; k < i * 6 + 6; k++)
                {
                    spreadedTileIndex[k] = expandedIndex[k];
                }
                await Task.Delay(1);
            }
        }
        private async Task PerformRemoving()
        {
            int total = spreadedTileIndex.Length / 6;
            for (int i = 0; i < total; i++)
            {
                for(int k = i * 6; k < i * 6 + 6; k++)
                {
                    spreadedTileIndex[k] = 0;
                }
                await Task.Delay(1);
            }
        }
        private void Spread(int gridIndex, int power)
        {
            expandedIndex.Add(gridIndex);
            expandedIndexPower.Add(power);
            if (power - 1 > 0)
            {
                Random rand = new Random();
                int index = rand.Next(0, 3);
                if (index == 0)
                {
                    if (IsSpreadable(gridIndex - g.currentMap.x))
                    {
                        Spread(gridIndex - g.currentMap.x, power - 1);
                    }
                    if (IsSpreadable(gridIndex - 1))
                    {
                        Spread(gridIndex - 1, power - 1);
                    }
                    if (IsSpreadable(gridIndex + g.currentMap.x))
                    {
                        Spread(gridIndex + g.currentMap.x, power - 1);
                    }
                    if (IsSpreadable(gridIndex + 1))
                    {
                        Spread(gridIndex + 1, power - 1);
                    }
                }
                else if (index == 1)
                {
                    if (IsSpreadable(gridIndex - g.currentMap.x))
                    {
                        Spread(gridIndex - g.currentMap.x, power - 1);
                    }
                    if (IsSpreadable(gridIndex + g.currentMap.x))
                    {
                        Spread(gridIndex + g.currentMap.x, power - 1);
                    }
                    if (IsSpreadable(gridIndex - 1))
                    {
                        Spread(gridIndex - 1, power - 1);
                    }
                    if (IsSpreadable(gridIndex + 1))
                    {
                        Spread(gridIndex + 1, power - 1);
                    }
                }
                else
                {
                    if (IsSpreadable(gridIndex + 1))
                    {
                        Spread(gridIndex + 1, power - 1);
                    }
                    if (IsSpreadable(gridIndex - 1))
                    {
                        Spread(gridIndex - 1, power - 1);
                    }
                    if (IsSpreadable(gridIndex - g.currentMap.x))
                    {
                        Spread(gridIndex - g.currentMap.x, power - 1);
                    }
                    if (IsSpreadable(gridIndex + g.currentMap.x))
                    {
                        Spread(gridIndex + g.currentMap.x, power - 1);
                    }
                }
            }
        }
        private bool IsSpreadable(int gridIndex)
        {
            if (gridIndex > -1 && gridIndex < totalMapSize)
            {
                if (!expandedIndex.Contains(gridIndex))
                {
                    return true;
                }
            }
            return false;
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
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (isActive)
            {
                if (spreadedTileIndex.Length > 0)
                {
                    base.Draw(gameTime);
                    spriteBatch.Begin();
                    for (int k = 0; k < spreadedTileIndex.Length; k++)
                    {
                        if (spreadedTileIndex[k] != 0)
                        spriteBatch.Draw(texture, new Vector2(g.currentMap.GetTileX(spreadedTileIndex[k]), g.currentMap.GetTileY(spreadedTileIndex[k])) * g.currentMap.size + g.mapOffset - center, frameRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, (float)spreadedTileIndex[k] / (float)totalMapSize);
                    }
                    spriteBatch.End();
                }
            }
        }
    }
}
