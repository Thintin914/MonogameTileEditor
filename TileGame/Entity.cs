using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
namespace TileGame
{
    public class Entity : DrawableGameComponent
    {
        public const int TotalEntity = 4;
        public EntityDetails entityDetails;
        private double frameElapsedTime;
        public const double frameTimeStep = 1000 / 8f;
        private int currentFrame = 0, totalFrame;
        private bool isForward = true;

        public Entity (Game1 g,  EntityTable.EntityAnimationStatus status): base(g)
        {
            totalFrame = status.totalFrame;
            entityDetails = new EntityDetails(g, status);
        }

        public class EntityDetails : DrawableGameComponent
        {
            public string fileName;
            public Texture2D entityWholeTexture;
            public Rectangle frameRect;
            public Vector2 wholeCenter, partialCenter;
            public bool isAnimated;
            public int ID;

            public EntityDetails(Game1 g, EntityTable.EntityAnimationStatus status) : base(g)
            {
                ID = status.ID;
                isAnimated = status.isAnimated;
                fileName = EntityTable.GetEntityStatus(ID).name;
                entityWholeTexture = Game.Content.Load<Texture2D>("GameEntities\\" + fileName);
                wholeCenter = new Vector2(entityWholeTexture.Width * 0.5f, entityWholeTexture.Height * 0.5f);
                if (isAnimated)
                {
                    frameRect = new Rectangle(0, 0, entityWholeTexture.Width / (status.totalFrame + 1), entityWholeTexture.Height);
                }
                else
                {
                    frameRect = new Rectangle(0, 0, entityWholeTexture.Width / status.totalFrame, entityWholeTexture.Height);
                }  

                partialCenter = new Vector2(frameRect.X + frameRect.Width * 0.5f, frameRect.Y + frameRect.Height * 0.5f);
            }
        }
        public override void Update(GameTime gameTime)
        {
            if (entityDetails.isAnimated)
            {
                frameElapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (frameElapsedTime >= frameTimeStep)
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
                    entityDetails.frameRect.X = currentFrame * entityDetails.frameRect.Width;
                    frameElapsedTime = 0;
                    base.Update(gameTime);
                }
            }
        }
    }
}
