using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace TileGame
{
    public class Player: DrawableGameComponent
    {
        private Game1 g;
        public Texture2D texture;
        public Vector2 position, footPosition, pastStandablePosition;
        private Vector2 gridPosition, velocity;
        public Vector2 center;
        private float speed = 3f;
        private KeyboardState lastKeyboardState;

        public Rectangle frameRect, HPRect, barRect;
        private double frameElapsedTime, invincibleTime;
        private bool isForward = true;
        public Color currentColor;
        private Color hitColor;
        public SpriteEffects flipside;
        private int currentFrame, poseStartFrame, poseEndFrame, totalFrame;
        public int gridIndex, HP, maxHP = 3;
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
            currentColor = Color.White;
            hitColor = new Color(255, 66, 148);
            HP = maxHP;
            HPRect = new Rectangle((int)(position.X - center.X), (int)(position.Y - center.Y - 10), frameRect.Width, 5);
            barRect = HPRect;
            barRect.X -= 1;
            barRect.Y -= 1;
            barRect.Width += 2;
            barRect.Height += 2;
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
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            frameElapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (frameElapsedTime >= Character.frameTimeStep)
            {
                if (currentColor == Color.White)
                {
                    for (int i = 0; i < g.enemyAttacks.Length; i++)
                    {
                        if (Game1.IsWithinRectangle(position, g.enemyAttacks[i].attackRect))
                        {
                            if (HP > 0)
                                HP--;
                            currentColor = hitColor;
                            invincibleTime = gameTime.TotalGameTime.TotalSeconds;
                            break;
                        }
                    }
                }

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

            }
            footPosition = position + new Vector2(0, 1) * frameRect.Height * 0.5f;
            gridPosition = TileMap.ToGrid(footPosition, g.currentMap.size);
            gridIndex = g.currentMap.GetTileIndexFromPosition(gridPosition.X, gridPosition.Y, g.mapOffset.X, g.mapOffset.Y);
            if (Game1.IsWithinRectangle(footPosition + velocity * speed, g.mapRect) && g.currentMap.hitbox[gridIndex] != 1)
            {
                pastStandablePosition = position;
                if (ks.IsKeyDown(Keys.W))
                {
                    currentAnimation = Character.AnimationType.walk;
                    velocity.Y = -speed;
                }
                if (ks.IsKeyDown(Keys.S))
                {
                    currentAnimation = Character.AnimationType.walk;
                    velocity.Y = speed;
                }
                if (ks.IsKeyDown(Keys.A))
                {
                    currentAnimation = Character.AnimationType.walk;
                    velocity.X = -speed;
                    flipside = SpriteEffects.FlipHorizontally;
                }
                if (ks.IsKeyDown(Keys.D))
                {
                    currentAnimation = Character.AnimationType.walk;
                    velocity.X = speed;
                    flipside = SpriteEffects.None;
                }
            }
            else
            {
                position = pastStandablePosition;
                velocity = Vector2.Zero;
            }
            position += velocity;
            velocity *= 0.9f;
            if (MathF.Abs(velocity.X) < 0.1f)
                velocity.X = 0;
            if (MathF.Abs(velocity.Y) < 0.1f)
                velocity.Y = 0;

            HPRect.X = (int)(position.X - center.X);
            HPRect.Y = (int)(position.Y - center.Y - 10);
            HPRect.Width = (int)((float)HP / (float)maxHP * frameRect.Width);
            barRect.X = HPRect.X - 1;
            barRect.Y = HPRect.Y - 1;
            if (currentColor == hitColor && gameTime.TotalGameTime.TotalSeconds - invincibleTime > 1.5f)
            {
                currentColor = Color.White;
            }
            if (HP < 1)
            {
                HP = maxHP;
            }
            lastKeyboardState = ks;
        }
    }
}
