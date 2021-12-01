using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TileGame
{
    class Player: DrawableGameComponent
    {
        private Game1 g;
        private Texture2D texture;
        private Vector2 position, center, pointDirection, gridPosition, velocity;
        private float speed = 3f;
        private SpriteBatch spriteBatch;
        private KeyboardState lastKeyboardState;

        private Rectangle frameRect;
        private double frameElapsedTime;
        private bool isForward = true;
        private int currentFrame, poseStartFrame, poseEndFrame, totalFrame, gridIndex;
        public Character.AnimationType currentAnimation, lastAnimation;
        public Character.CharacterAnimation[] allAnimations;

        public Player(Game1 g, Vector2 position): base(g)
        {
            this.g = g;
            this.position = position;
            
            currentAnimation = Character.AnimationType.idle;
            lastAnimation = Character.AnimationType.idle;
            allAnimations = new Character.CharacterAnimation[3];

            allAnimations[0] = new Character.CharacterAnimation(Character.AnimationType.idle, 1, 3);
            allAnimations[1] = new Character.CharacterAnimation(Character.AnimationType.walk, 4, 6);
            allAnimations[2] = new Character.CharacterAnimation(Character.AnimationType.attack, 7, 9);

            totalFrame = Character.GetTotalFrame(ref allAnimations);
            SetFrameDetails(currentAnimation);
            texture = g.Content.Load<Texture2D>("Characters\\person1");
            frameRect = new Rectangle(0, 0, texture.Width / (totalFrame + 1), texture.Height);
            center = new Vector2(frameRect.Width * 0.5f, frameRect.Height * 0.5f);
        }
        private void SetFrameDetails(Character.AnimationType pose)
        {
            int tempStartFrame = 0, tempEndFrame = 0;
            bool canFind = false;
            for (int i = 0; i < allAnimations.Length; i++)
            {
                if (allAnimations[i] != null)
                {
                    tempStartFrame = allAnimations[i].startFrame;
                    tempEndFrame = allAnimations[i].endFrame;
                    if (allAnimations[i].pose == pose)
                    {
                        canFind = true;
                        break;
                    }
                }
            }
            if (canFind)
            {
                poseStartFrame = tempStartFrame;
                poseEndFrame = tempEndFrame;
            }
            else
            {
                currentFrame = 1;
                poseEndFrame = 1;
            }
        }
        protected override void LoadContent()
        {
            base.LoadContent();
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }
        private async Task DoAnimation(Character.AnimationType animation, GameTime gameTime)
        {
            SetFrameDetails(animation);
            lastAnimation = currentAnimation;
            currentFrame = poseStartFrame;
            isForward = true;
            while (currentFrame < poseEndFrame)
            {
                frameElapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (frameElapsedTime >= Character.frameTimeStep)
                {
                    frameElapsedTime = 0;
                    currentFrame++;
                    frameRect.X = frameRect.Width * (currentFrame - 1);
                }
                await Task.Yield();
            }
            while (currentFrame > poseStartFrame)
            {
                frameElapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (frameElapsedTime >= Character.frameTimeStep)
                {
                    frameElapsedTime = 0;
                    currentFrame--;
                    frameRect.X = frameRect.Width * (currentFrame - 1);
                }
                await Task.Yield();
            }
        }
        public override async void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            frameElapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (frameElapsedTime >= Character.frameTimeStep)
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

            KeyboardState ks = Keyboard.GetState();
            currentAnimation = Character.AnimationType.idle;
            if (ks.IsKeyDown(Keys.Space) && !lastKeyboardState.IsKeyDown(Keys.Space))
            {
                await DoAnimation(Character.AnimationType.attack, gameTime);
            }
            gridPosition = TileMap.ToGrid(position, g.currentMap.size);
            gridIndex = g.currentMap.GetTileIndexFromPosition(gridPosition.X, gridPosition.Y, g.mapOffset.X, g.mapOffset.Y);
            if (Game1.IsWithinRectangle(position + pointDirection * speed, g.mapRect) && g.currentMap.hitbox[gridIndex] != 1)
            {
                if (ks.IsKeyDown(Keys.W))
                {
                    currentAnimation = Character.AnimationType.walk;
                    position.Y += -speed;
                    pointDirection.Y = -1;
                }
                if (ks.IsKeyDown(Keys.S))
                {
                    currentAnimation = Character.AnimationType.walk;
                    position.Y += speed;
                    pointDirection.Y = 1;
                }
                if (ks.IsKeyDown(Keys.A))
                {
                    currentAnimation = Character.AnimationType.walk;
                    position.X += -speed;
                    pointDirection.X = -1;
                }
                if (ks.IsKeyDown(Keys.D))
                {
                    currentAnimation = Character.AnimationType.walk;
                    position.X += speed;
                    pointDirection.X = 1;
                }
            }
            else
            {
                position -= pointDirection * speed;
                pointDirection = Vector2.Zero;
            }
            lastKeyboardState = ks;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            spriteBatch.Begin();
            if (pointDirection.X == 1)
            {
                spriteBatch.Draw(texture, position - center, frameRect, Color.White);
            }
            else
            {
                spriteBatch.Draw(texture, position, frameRect, Color.White, 0, center, 1, SpriteEffects.FlipHorizontally, 1);
            }
            spriteBatch.End();
        }
    }
}
