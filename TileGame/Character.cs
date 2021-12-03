using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;

namespace TileGame
{
    public class Character: DrawableGameComponent
    {
        public enum AnimationType { idle, walk, attack};
        public AnimationType currentAnimation, lastAnimation;
        public CharacterAnimation[] allAnimations;
        public string fileName;
        public Texture2D texture;
        public Rectangle frameRect;
        private bool isAnimated, isForward = true, hasAttacked, needAttackRest;
        public bool isRight;
        private int currentFrame, poseStartFrame, poseEndFrame, totalFrame;
        public int ID, gridIndex;
        private double frameElapsedTime, attackChargingTime;
        private Game1 g;
        public float rotate;

        private Vector2 pastStandablePosition, footPosition, gridPosition;
        public Vector2 position, center, velocity;
        public const int totalCharacter = 3;
        public const double frameTimeStep = 1000 / 8f;

        public Character(Game1 g, int ID, Vector2 position): base(g)
        {
            this.g = g;
            this.ID = ID;
            this.position = position;
            SetCharacterVisualData(ID);
        }
        public Character(Character c): base(c.g)
        {
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
                default:
                    fileName = "coin";
                    allAnimations[0] = new CharacterAnimation(AnimationType.idle, 1, 4);
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
        private Vector2 GetNormalizedDirection(Vector2 targetPosition, Vector2 selfPosition, float speed = -1)
        {
            Vector2 temp = Vector2.Zero;
            if (targetPosition.X > selfPosition.X)
            {
                if (speed != -1)
                    if (MathF.Abs(targetPosition.X - selfPosition.X) > speed * 2)
                        isRight = true;
                temp.X = 1;
            }
            else if (targetPosition.X < selfPosition.X)
            {
                if (speed != -1)
                    if (MathF.Abs(targetPosition.X - selfPosition.X) > speed * 2)
                        isRight = false;
                temp.X = -1;
            }
            if (targetPosition.Y > selfPosition.Y)
            {
                temp.Y = 1;
            }
            else if (targetPosition.Y < selfPosition.Y)
            {
                temp.Y = -1;
            }
            return temp;
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
        private bool DoesGridContainsCharacter(int index)
        {
            for (int i = 0; i < g.mapCharacters.Count; i++)
            {
                if (g.mapCharacters[i] != this && g.mapCharacters[i].gridIndex == gridIndex)
                {
                    position = pastStandablePosition;
                    SetGridIndex();
                    return true;
                }
            }
            return false;
        }
        private void SetGridIndex()
        {
            footPosition = position + new Vector2(0, 1) * frameRect.Height * 0.5f;
            gridPosition = TileMap.ToGrid(footPosition + velocity, g.currentMap.size);
            gridIndex = g.currentMap.GetTileIndexFromPosition(gridPosition.X, gridPosition.Y, g.mapOffset.X, g.mapOffset.Y);
        }
        private bool IsMovable()
        {
            SetGridIndex();
            if (Game1.IsWithinRectangle(position, g.mapRect) && g.currentMap.hitbox[gridIndex] != 1 && g.currentMap.tileCostume[gridIndex] != 0 && !DoesGridContainsCharacter(gridIndex))
            {
                return true;
            }
            return false;
        }
        public override void Update(GameTime gameTime)
        {
            if (velocity != Vector2.Zero) {
                if (IsMovable())
                {
                    pastStandablePosition = position;
                    position += velocity;
                    velocity *= 0.9f;
                }
                else
                {
                    position = pastStandablePosition;
                    SetGridIndex();
                    velocity = Vector2.Zero;
                }
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
                            velocity = -GetNormalizedDirection(g.player.footPosition, position) * 3;
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

        public class CharacterBehaviour: Character
        {
            public string name;
            private float range, attackWaitTime, speed;
            public TileMap.CharacterData respectiveData;
            private string[] extraSettings;
            private KeyboardState lastKeyboardState;
            private bool isDelayReady;
            public CharacterBehaviour(Game1 g, int ID, Vector2 position, TileMap.CharacterData respectiveData): base (g, ID, position)
            {
                name = fileName;
                CharacterSettings tempSettings = SetRange(name);
                range = tempSettings.range;
                attackWaitTime = tempSettings.attackWaitTime;
                speed = tempSettings.speed;
                this.respectiveData = respectiveData;
                if (respectiveData.extra != null)
                {
                    extraSettings = respectiveData.extra.Split(", ");
                }
                PerformDelay();
            }
            private async void PerformDelay()
            {
                await StartDelay();
            }
            private async Task StartDelay()
            {
                await Task.Delay(1000);
                isDelayReady = true;
            }
            public override void Initialize()
            {
                base.Initialize();
                SetGridIndex();
            }
            public void SetExtraSettings()
            {
                if (respectiveData.extra != null)
                {
                    extraSettings = respectiveData.extra.Split(", ");
                }
            }
            private class CharacterSettings
            {
                public float range, attackWaitTime, speed;
                public CharacterSettings(float range, float attackWaitTime, float speed)
                {
                    this.range = range;
                    this.attackWaitTime = attackWaitTime;
                    this.speed = speed;
                }
            }
            private CharacterSettings SetRange(string name)
            {
                switch (name)
                {
                    case "person1":
                        return new CharacterSettings(120, 1, 2);
                }
                return new CharacterSettings(60, 0.5f, 1);
            }
            private void ChaseTarget(Vector2 targetPosition)
            {
                currentAnimation = AnimationType.walk;
                velocity = GetNormalizedDirection(targetPosition, footPosition, speed);
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
                    if (GetDistance(position, g.player.footPosition) < 42 && isDelayReady)
                    {
                        if (extraSettings.Length == 2)
                        {
                            KeyboardState ks = Keyboard.GetState();
                            if (ks.IsKeyDown(Keys.Space) && !lastKeyboardState.IsKeyDown(Keys.Space))
                            {
                                g.CreateNewMap(extraSettings[0]);
                                int index = int.Parse(extraSettings[1]);
                                Vector2 playerPosition = new Vector2(g.currentMap.GetTileX(index), g.currentMap.GetTileY(index)) * g.currentMap.size + g.mapOffset;
                                g.player.position = playerPosition;
                                g.player.pastStandablePosition = playerPosition;
                            }
                            lastKeyboardState = ks;
                        }
                        rotate = (rotate + 0.1f) % 360;
                    }
                }
                else if (name == "person1")
                {
                    if (GetDistance(position, g.player.position) > range && hasAttacked == false)
                    {
                        ChaseTarget(g.player.footPosition);
                    }
                    else
                    {
                        if (hasAttacked == false)
                        {
                            hasAttacked = true;
                            currentAnimation = AnimationType.attack;
                            int attackIndex = g.GetInactiveAttack(ref g.enemyAttacks);
                            if (attackIndex != -1) {
                                g.enemyAttacks[attackIndex].SetAttack(position, g.player.position, range);
                            }
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
