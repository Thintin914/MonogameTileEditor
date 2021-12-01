using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace TileGame
{
    public class Character: DrawableGameComponent
    {
        public enum AnimationType { idle, walk, attack};
        public AnimationType currentAnimation, lastAnimation;
        public CharacterAnimation[] allAnimations;
        public string fileName;
        private Texture2D texture;
        public Rectangle frameRect;
        private bool isAnimated, isForward = true, isRight = true, hasAttacked, needAttackRest;
        private int currentFrame, poseStartFrame, poseEndFrame, totalFrame;
        public int ID;
        private double frameElapsedTime, attackChargingTime;
        private Game1 g;
        private SpriteBatch spriteBatch;
        private float rotate;

        public Vector2 position, center, velocity;
        public const int totalCharacter = 3;
        public const double frameTimeStep = 1000 / 8f;

        public Character(Game1 g, int ID, Vector2 position): base(g)
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            this.g = g;
            this.ID = ID;
            this.position = position;
            SetCharacterVisualData(ID);
        }
        public Character(Character c): base(c.g)
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            g = c.g;
            ID = c.ID;
            position = c.position;
            SetCharacterVisualData(ID);
        }
        public class CharacterAnimation
        {
            public AnimationType pose;
            public int startFrame, endFrame;
            public CharacterAnimation(AnimationType pose, int startFrame, int endFrame)
            {
                this.pose = pose;
                this.startFrame = startFrame;
                this.endFrame = endFrame;
            }
        }
        public void SetCharacterVisualData(int ID)
        {
            currentAnimation = AnimationType.idle;
            lastAnimation = AnimationType.idle;
            allAnimations = new CharacterAnimation[3];
            switch (ID)
            {
                case 0:
                    fileName = "coin";
                    allAnimations[0] = new CharacterAnimation(AnimationType.idle, 1, 4);
                    break;
                case 1:
                    fileName = "portal1";
                    allAnimations[0] = new CharacterAnimation(AnimationType.idle, 1, 1);
                    break;
                case 2:
                    fileName = "person1";
                    allAnimations[0] = new CharacterAnimation(AnimationType.idle, 1, 3);
                    allAnimations[1] = new CharacterAnimation(AnimationType.walk, 4, 6);
                    allAnimations[2] = new CharacterAnimation(AnimationType.attack, 7, 9);
                    break;
            }
            totalFrame = GetTotalFrame(ref allAnimations);
            SetFrameDetails(currentAnimation);
            if (poseEndFrame > 1)
                isAnimated = true;
            texture = g.Content.Load<Texture2D>("Characters\\" + fileName);
            if (isAnimated)
            {
                frameRect = new Rectangle(0, 0, texture.Width / (totalFrame + 1), texture.Height);
            }
            else
            {
                frameRect = new Rectangle(0, 0, texture.Width / totalFrame, texture.Height);
            }
            center = new Vector2(frameRect.Width * 0.5f, frameRect.Height * 0.5f);
        }
        public static int GetTotalFrame(ref CharacterAnimation[] allAnimations)
        {
            int tempFrame = 0;
            for (int i = 0; i < allAnimations.Length; i++)
            {
                if (allAnimations[i] != null)
                {
                    tempFrame = allAnimations[i].endFrame;
                }
                else
                {
                    break;
                }
            }
            return tempFrame;
        }
        private void SetFrameDetails(AnimationType pose)
        {
            int tempStartFrame = 0, tempEndFrame = 0;
            bool canFind = false;
            for(int i = 0; i < allAnimations.Length; i++)
            {
                if (allAnimations[i] != null) {
                    tempStartFrame = allAnimations[i].startFrame;
                    tempEndFrame = allAnimations[i].endFrame;
                    if (allAnimations[i].pose == pose)
                    {
                        canFind = true;
                        break;
                    }
                }
            }
            if (canFind) {
                poseStartFrame = tempStartFrame;
                poseEndFrame = tempEndFrame;
            }
            else
            {
                currentFrame = 1;
                poseEndFrame = 1;
            }
        }
        public override void Update(GameTime gameTime)
        {
            if (velocity != Vector2.Zero) {
                position += velocity;
                velocity *= 0.9f;
                if (MathF.Abs(velocity.X) < 0.1f)
                    velocity.X = 0;
                if (MathF.Abs(velocity.Y) < 0.1f)
                    velocity.Y = 0;
            }
            if (isAnimated)
            {
                frameElapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (frameElapsedTime >= frameTimeStep)
                {
                    if (currentAnimation != lastAnimation)
                    {
                        SetFrameDetails(currentAnimation);
                        lastAnimation = currentAnimation;
                    }
                    if (currentFrame < poseStartFrame || currentFrame > poseEndFrame)
                    {
                        currentFrame = poseStartFrame;
                    }
                    if (isForward && currentFrame == poseEndFrame)
                    {
                        isForward = false;
                        if (hasAttacked == true && currentAnimation == AnimationType.attack)
                        {
                            needAttackRest = true;
                            currentAnimation = AnimationType.idle;
                            attackChargingTime = gameTime.TotalGameTime.TotalSeconds;
                        }
                    }
                    else if (!isForward && currentFrame == poseStartFrame)
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
            base.Draw(gameTime);
            spriteBatch.Begin();
            if (isRight) 
            {
                spriteBatch.Draw(texture, position, frameRect, Color.White, rotate, center, 1, SpriteEffects.None, 0);
            }
            else
            {
                spriteBatch.Draw(texture, position, frameRect, Color.White, rotate, center, 1, SpriteEffects.FlipHorizontally, 0);
            }
            spriteBatch.End();
        }

        public class CharacterBehaviour: Character
        {
            public string name;
            private float range, attackWaitTime;
            private Vector2 gridPosition, footPosition, pastStandablePosition;
            public int gridIndex;

            public CharacterBehaviour(Game1 g, int ID, Vector2 position): base (g, ID, position)
            {
                SetGridIndex();
                name = fileName;
                CharacterSettings tempSettings = SetRange(name);
                range = tempSettings.range;
                attackWaitTime = tempSettings.attackWaitTime;
            }
            private class CharacterSettings
            {
                public float range, attackWaitTime;
                public CharacterSettings(float range, float attackWaitTime)
                {
                    this.range = range;
                    this.attackWaitTime = attackWaitTime;
                }
            }
            private CharacterSettings SetRange(string name)
            {
                switch (name)
                {
                    case "person1":
                        return new CharacterSettings(60, 1);
                }
                return new CharacterSettings(60, 0.5f);
            }
            private void ChaseTarget(Vector2 targetPosition)
            {
                currentAnimation = AnimationType.walk;
                SetGridIndex();
                if (g.currentMap.hitbox[gridIndex] != 1 && g.currentMap.tileCostume[gridIndex] != 0) {
                    pastStandablePosition = position;
                    if (targetPosition.X > footPosition.X)
                    {
                        isRight = true;
                        velocity.X = 1;
                    }
                    else if (targetPosition.X < footPosition.X)
                    {
                        isRight = false;
                        velocity.X = -1;
                    }
                    if (targetPosition.Y > footPosition.Y)
                    {
                        velocity.Y = 1;
                    }
                    else if (targetPosition.Y < footPosition.Y)
                    {
                        velocity.Y = -1;
                    }
                }
                else
                {
                    position = pastStandablePosition;
                    velocity = Vector2.Zero;
                }
            }
            private void SetGridIndex()
            {
                footPosition = position + new Vector2(0, 1) * frameRect.Height * 0.4f;
                gridPosition = TileMap.ToGrid(footPosition + velocity, g.currentMap.size);
                gridIndex = g.currentMap.GetTileIndexFromPosition(gridPosition.X, gridPosition.Y, g.mapOffset.X, g.mapOffset.Y);
            }
            public static float GetDistance(Vector2 a, Vector2 b)
            {
                return MathF.Sqrt(MathF.Pow(a.X - b.X, 2) + MathF.Pow(a.Y - b.Y, 2));
            }
            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);
                if (name == "portal1")
                {
                    if (GetDistance(position, g.player.footPosition) < 42)
                    {

                    }
                    else
                    {
                        rotate = (rotate + 0.1f) % 360;
                    }
                }
                else if (name == "person1")
                {
                    if (GetDistance(position, g.player.footPosition) > range && hasAttacked == false)
                    {
                        ChaseTarget(g.player.footPosition);
                    }
                    else
                    {
                        if (hasAttacked == false)
                        {
                            hasAttacked = true;
                            currentAnimation = AnimationType.attack;
                        }
                        else if (needAttackRest)
                        {
                            if (gameTime.TotalGameTime.TotalSeconds - attackChargingTime > attackWaitTime)
                            {
                                needAttackRest = false;
                                hasAttacked = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
