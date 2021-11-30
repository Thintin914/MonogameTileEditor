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
        private bool isAnimated, isForward = true;
        private int currentFrame, poseStartFrame, poseEndFrame, totalFrame;
        public int ID;
        private double frameElapsedTime;
        private Game1 g;
        private SpriteBatch spriteBatch;
        private float rotate;

        public Vector2 position, center;
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
            currentAnimation = AnimationType.walk;
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
            totalFrame = GetTotalFrame();
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
        private int GetTotalFrame()
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
            spriteBatch.Draw(texture, position, frameRect, Color.White, rotate, center, 1, SpriteEffects.None, 1);
            spriteBatch.End();
        }

        public class CharacterBehaviour: Character
        {
            public string name;

            public CharacterBehaviour(Game1 g, int ID, Vector2 position): base (g, ID, position)
            {
                name = fileName;
            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);
                if (name == "portal1")
                {
                    rotate = (rotate + 0.1f) % 360;
                }
            }
        }
    }
}
