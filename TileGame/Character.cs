using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;

namespace TileGame
{
    public class Character: DrawableGameComponent
    {
        public enum AnimationType { idle, walk, attack};
        public AnimationType currentAnimation, lastAnimation;
        public CharacterAnimation[] allAnimations;
        public string fileName;
        public Texture2D texture;
        public Rectangle frameRect, hitbox;
        private bool isAnimated, isForward = true, hasAttacked, needAttackRest, isPush;
        public SpriteEffects flipside;
        private int currentFrame, poseStartFrame, poseEndFrame, totalFrame;
        public int ID, gridIndex;
        private double frameElapsedTime, attackChargingTime;
        private Game1 g;
        public float rotate;
        private float range, speed, attackRange;
        private int attackTextureID;
        private Vector2 pastStandablePosition, footPosition, gridPosition, reflectPosition;
        public Vector2 position, center, velocity;
        public const int totalCharacter = 7;
        public const double frameTimeStep = 1000 / 8f;

        public Character(Game1 g, int ID, Vector2 position): base(g)
        {
            this.g = g;
            this.ID = ID;
            this.position = position;
            SetCharacterVisualData(ID);
            SetGridIndex();
        }
        public Character(Character c): base(c.g)
        {
            g = c.g;
            ID = c.ID;
            position = c.position;
            SetCharacterVisualData(ID);
            SetGridIndex();
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
                case 3:
                    fileName = "slime";
                    allAnimations[0] = new CharacterAnimation(AnimationType.idle, 1, 3);
                    allAnimations[1] = new CharacterAnimation(AnimationType.walk, 4, 6);
                    allAnimations[2] = new CharacterAnimation(AnimationType.attack, 7, 9);
                    break;
                case 4:
                    fileName = "bigSlime";
                    allAnimations[0] = new CharacterAnimation(AnimationType.idle, 1, 3);
                    allAnimations[1] = new CharacterAnimation(AnimationType.walk, 1, 3);
                    allAnimations[2] = new CharacterAnimation(AnimationType.attack, 4, 6);
                    break;
                case 5:
                    fileName = "chest";
                    allAnimations[0] = new CharacterAnimation(AnimationType.idle, 1, 1);
                    break;
                case 6:
                    fileName = "rockman";
                    allAnimations[0] = new CharacterAnimation(AnimationType.idle, 1, 2);
                    allAnimations[1] = new CharacterAnimation(AnimationType.walk, 1, 2);
                    allAnimations[2] = new CharacterAnimation(AnimationType.attack, 2, 5);
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
            hitbox = frameRect;
            center = new Vector2(frameRect.Width * 0.5f, frameRect.Height * 0.5f);
        }
        private Vector2 GetNormalizedDirection(Vector2 targetPosition, Vector2 selfPosition, float speed = -1)
        {
            Vector2 temp = Vector2.Zero;
            if (targetPosition.X > selfPosition.X)
            {
                if (speed != -1)
                    if (MathF.Abs(targetPosition.X - selfPosition.X) > speed * 2)
                        flipside = SpriteEffects.None;
                temp.X = 1;
            }
            else if (targetPosition.X < selfPosition.X)
            {
                if (speed != -1)
                    if (MathF.Abs(targetPosition.X - selfPosition.X) > speed * 2)
                        flipside = SpriteEffects.FlipHorizontally;
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
            if (hasAttacked == false)
            {
                for (int i = 0; i < g.mapCharacters.Count; i++)
                {
                    if (g.mapCharacters[i] != this && g.mapCharacters[i].gridIndex == index)
                    {
                        currentAnimation = AnimationType.attack;
                        position = pastStandablePosition;
                        isPush = true;
                        hasAttacked = true;
                        reflectPosition = g.mapCharacters[i].position;
                        return true;
                    }
                }
            }
            return false;
        }
        private void SetGridIndex()
        {
            footPosition = position + new Vector2(0, 1) * hitbox.Height * 0.5f;
            gridPosition = TileMap.ToGrid(footPosition + velocity * speed, g.currentMap.size);
            gridIndex = g.currentMap.GetTileIndexFromPosition(gridPosition.X, gridPosition.Y, g.mapOffset.X, g.mapOffset.Y);
        }
        private bool IsMovable()
        {
            Vector2 tempFootPosition = position + new Vector2(0, 1) * hitbox.Height * 0.5f;
            Vector2 tempgridPosition = TileMap.ToGrid(tempFootPosition + velocity * speed, g.currentMap.size);
            int tempIndex = g.currentMap.GetTileIndexFromPosition(tempgridPosition.X, tempgridPosition.Y, g.mapOffset.X, g.mapOffset.Y);
            if (tempIndex > -1 && tempIndex < g.currentMap.hitbox.Count)
            {
                if (Game1.IsWithinRectangle(position, g.mapRect) && g.currentMap.hitbox[tempIndex] != 1 && g.currentMap.tileCostume[tempIndex] != 0 && !DoesGridContainsCharacter(tempIndex))
                {
                    SetGridIndex();
                    return true;
                }
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
                            velocity = -GetNormalizedDirection(reflectPosition, position) * speed;
                            if (!isPush)
                            {
                                PerformAttack(fileName);
                            }
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
            hitbox.X = (int)(position.X - center.X);
            hitbox.Y = (int)(position.Y - center.Y);
        }
        private void PerformAttack(string name)
        {
            switch (name)
            {
                case "bigSlime":
                    Vector2 spawnPosition = position + GetNormalizedDirection(g.player.position, position) * g.currentMap.size;
                    if (Game1.IsWithinRectangle(spawnPosition, g.mapRect))
                    {
                        g.mapCharacters.Add(new CharacterBehaviour(g, 3, spawnPosition));
                        g.Components.Add(g.mapCharacters[g.mapCharacters.Count - 1]);
                    }
                    break;
                case "rockman":
                    Vector2[] allDirections = new Vector2[8];
                    allDirections[0] = new Vector2(0, 1);
                    allDirections[1] = new Vector2(0, -1);
                    allDirections[2] = new Vector2(-1, 0);
                    allDirections[3] = new Vector2(1, 0);
                    allDirections[4] = new Vector2(-1, 1);
                    allDirections[5] = new Vector2(1, 1);
                    allDirections[6] = new Vector2(-1, -1);
                    allDirections[7] = new Vector2(1, -1);
                    int rockIndex = -1;
                    for (int i = 0; i < allDirections.Length; i++)
                    {
                        rockIndex =  g.GetInactiveAttack(ref g.enemyAttacks);
                        if (rockIndex != -1)
                        {
                            g.enemyAttacks[rockIndex].SetAttack(position, position + allDirections[i], attackRange, attackTextureID);
                        }
                    }
                    break;
                default:
                    int attackIndex = g.GetInactiveAttack(ref g.enemyAttacks);
                    if (attackIndex != -1)
                    {
                        g.enemyAttacks[attackIndex].SetAttack(position, g.player.position, attackRange, attackTextureID);
                    }
                    break;
            }
        }
        public class CharacterBehaviour: Character
        {
            private float attackWaitTime;
            public TileMap.CharacterData respectiveData;
            private string[] extraSettings;
            private KeyboardState lastKeyboardState;
            private bool isDelayReady, isAggressive, hasEnteredPortal;
            public Color currentColor = Color.White;
            private Color hitColor;
            public int maxHP, HP;
            private double invincibleTime;
            public Rectangle HPRect, barRect;
            private SoundEffect hit1, hit2, money1, money2, money3, tick, deny;

            public CharacterBehaviour(Game1 g, int ID, Vector2 position, TileMap.CharacterData respectiveData = null): base (g, ID, position)
            {
                CharacterSettings tempSettings = SetRange(fileName);
                range = tempSettings.range;
                attackWaitTime = tempSettings.attackWaitTime;
                speed = tempSettings.speed;
                maxHP = tempSettings.maxHP;
                HP = tempSettings.maxHP;
                attackTextureID = tempSettings.attackTextureID;
                isAggressive = tempSettings.isAggressive;
                attackRange = tempSettings.attackRange;
                this.respectiveData = respectiveData;
                if (respectiveData != null && respectiveData.extra != null)
                {
                    extraSettings = respectiveData.extra.Split(", ");
                }
                hitColor = new Color(255, 66, 148);
                HPRect = new Rectangle((int)(position.X - center.X), (int)(position.Y - center.Y - 10), frameRect.Width, 5);
                barRect = HPRect;
                barRect.X -= 1;
                barRect.Y -= 1;
                barRect.Width += 2;
                barRect.Height += 2;

                if (maxHP > 0)
                {
                    hit1 = g.Content.Load<SoundEffect>("Sounds\\hit");
                    hit2 = g.Content.Load<SoundEffect>("Sounds\\hit2");
                }
                if (fileName == "coin")
                {
                    money1 = g.Content.Load<SoundEffect>("Sounds\\money");
                    money2 = g.Content.Load<SoundEffect>("Sounds\\money2");
                    money3 = g.Content.Load<SoundEffect>("Sounds\\money3");
                }
                if (fileName == "portal1")
                {
                    tick = g.Content.Load<SoundEffect>("Sounds\\tick");
                    deny = g.Content.Load<SoundEffect>("Sounds\\deny");
                }
                if (isAggressive)
                {
                    deny = g.Content.Load<SoundEffect>("Sounds\\deny");
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
                if (respectiveData != null && respectiveData.extra != null)
                {
                    extraSettings = respectiveData.extra.Split(", ");
                }
            }
            private class CharacterSettings
            {
                public float range, attackWaitTime, speed, attackRange;
                public int maxHP, attackTextureID;
                public bool isAggressive;
                public CharacterSettings(float range, float attackRange, float attackWaitTime, float speed, int HP, int attackTextureID, bool isAggressive = true)
                {
                    this.range = range;
                    this.attackWaitTime = attackWaitTime;
                    this.speed = speed;
                    maxHP = HP;
                    this.attackTextureID = attackTextureID;
                    this.isAggressive = isAggressive;
                    this.attackRange = attackRange;
                }
            }
            private CharacterSettings SetRange(string name)
            {
                switch (name)
                {
                    case "person1":
                        return new CharacterSettings(120, 240, 1, 2, 5, 1);
                    case "slime":
                        return new CharacterSettings(60, 120, 0.5f, 1, 2, 2);
                    case "bigSlime":
                        return new CharacterSettings(150, 300, 0.3f, 1, 10, 2);
                    case "chest":
                        return new CharacterSettings(0, 0, 0, 0, 3, 0, false);
                    case "rockman":
                        return new CharacterSettings(150, 500, 5, 0.1f, 10, 2);
                }
                return new CharacterSettings(0, 0, 0, 0, 0, 1, false);
            }
            private void ChaseTarget(Vector2 targetPosition)
            {
                currentAnimation = AnimationType.walk;
                if (MathF.Abs(velocity.X) < speed && MathF.Abs(velocity.Y) < speed)
                    velocity += GetNormalizedDirection(targetPosition, footPosition, speed * 0.2f);
            }
            public static float GetDistance(Vector2 a, Vector2 b)
            {
                return MathF.Sqrt(MathF.Pow(a.X - b.X, 2) + MathF.Pow(a.Y - b.Y, 2));
            }
            private void BasicAttackBehaviour(GameTime gameTime)
            {
                if (GetDistance(position, g.player.footPosition) > range && hasAttacked == false)
                {
                    ChaseTarget(g.player.footPosition);
                }
                else
                {
                    if (hasAttacked == false)
                    {
                        deny.Play(0.2f, 0, 0);
                        reflectPosition = g.player.position;
                        hasAttacked = true;
                        isForward = true;
                        currentAnimation = AnimationType.attack;
                    }
                    else if (needAttackRest)
                    {
                        if (isPush)
                        {
                            if (gameTime.TotalGameTime.TotalSeconds - attackChargingTime > 0.3f)
                            {
                                needAttackRest = false;
                                hasAttacked = false;
                                isPush = false;
                            }
                        }
                        else
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
            public override void Update(GameTime gameTime)
            {
                if (g.gameState == Game1.GameState.playMode)
                {
                    base.Update(gameTime);
                    if (gridIndex < 0 || gridIndex > g.currentMap.x * g.currentMap.y - 1)
                    {
                        g.Components.Remove(this);
                        g.mapCharacters.Remove(this);
                        Dispose();
                    }
                    if (maxHP != 0)
                    {
                        if (currentColor == Color.White)
                        {
                            for (int i = 0; i < g.allyAttacks.Length; i++)
                            {
                                if (g.allyAttacks[i].isActive && Game1.IsWithinRectangle(position, g.allyAttacks[i].attackRect))
                                {
                                    if (HP > 0)
                                        HP--;
                                    currentColor = hitColor;
                                    invincibleTime = gameTime.TotalGameTime.TotalSeconds;
                                    velocity = -GetNormalizedDirection(g.allyAttacks[i].currentPosition, position) * speed;
                                    Random rand = new Random();
                                    if (rand.Next(2) == 0)
                                    {
                                        hit1.Play(0.5f, 0, 0);
                                    }
                                    else
                                    {
                                        hit2.Play(0.5f, 0, 0);
                                    }
                                    break;
                                }
                            }
                        }
                        if (currentColor == hitColor && gameTime.TotalGameTime.TotalSeconds - invincibleTime > 0.5f)
                        {
                            currentColor = Color.White;
                        }
                        if (HP < 1)
                        {
                            Random rand = new Random();
                            if (fileName == "chest")
                            {
                                for(int i = 0; i < 4; i++)
                                {
                                    Vector2 coinPosition = position + new Vector2(i, 0) * g.currentMap.size;
                                    if (Game1.IsWithinRectangle(coinPosition, g.mapRect))
                                    {
                                        g.mapCharacters.Add(new CharacterBehaviour(g, 0, coinPosition));
                                        g.Components.Add(g.mapCharacters[g.mapCharacters.Count - 1]);
                                    }
                                }
                            }
                            else
                            {
                                g.mapCharacters.Add(new CharacterBehaviour(g, 0, position));
                                g.Components.Add(g.mapCharacters[g.mapCharacters.Count - 1]);
                                g.mapCharacters[g.mapCharacters.Count - 1].velocity = -GetNormalizedDirection(g.player.position, position) * ((float)rand.NextDouble() * 5);
                            }
                            g.mapCharacters.Remove(this);
                            g.Components.Remove(this);
                            Dispose();
                        }
                        HPRect.X = (int)(position.X - center.X);
                        HPRect.Y = (int)(position.Y - center.Y - 10);
                        HPRect.Width = (int)((float)HP / (float)maxHP * frameRect.Width);
                        barRect.X = HPRect.X - 1;
                        barRect.Y = HPRect.Y - 1;
                    }
                    if (fileName == "coin")
                    {
                        if (GetDistance(position, g.player.footPosition) < 50 && isDelayReady)
                        {
                            g.score.AddScore(1);
                            Random rand = new Random();
                            int randIndex = rand.Next(4);
                            if (randIndex == 0)
                                money1.Play(0.5f, 0, 0);
                            else if (randIndex == 1)
                                money2.Play(0.5f, 0, 0);
                            else if (randIndex == 2)
                                money3.Play(0.5f, 0, 0);
                            g.mapCharacters.Remove(this);
                            g.Components.Remove(this);
                            Dispose();
                        }
                    }
                    else if (fileName == "portal1")
                    {
                        if (GetDistance(position, g.player.footPosition) < 42 && isDelayReady)
                        {
                            if (extraSettings.Length == 2)
                            {
                                hasEnteredPortal = true;
                                g.hintWords = "Press Space to Enter";
                                rotate = (rotate + 0.1f) % 360;
                                KeyboardState ks = Keyboard.GetState();
                                if (ks.IsKeyDown(Keys.Space) && !lastKeyboardState.IsKeyDown(Keys.Space))
                                {
                                    bool isClear = false;
                                    int loopNumber = g.mapCharacters.Count;
                                    int count = 0;
                                    for (int i = 0; i < loopNumber; i++)
                                    {
                                        if (g.mapCharacters[i].isAggressive && g.mapCharacters[i].maxHP > 0)
                                        {
                                            break;
                                        }
                                        count++;
                                    }
                                    if (count == loopNumber)
                                        isClear = true;
                                    if (isClear)
                                    {
                                        tick.Play(0.5f, 0, 0);
                                        g.CreateNewMap(extraSettings[0]);
                                        int index = int.Parse(extraSettings[1]);
                                        Vector2 playerPosition = new Vector2(g.currentMap.GetTileX(index), g.currentMap.GetTileY(index)) * g.currentMap.size + g.mapOffset;
                                        g.player.position = playerPosition;
                                        g.player.pastStandablePosition = playerPosition;
                                    }
                                    else
                                    {
                                        deny.Play(0.5f, 0, 0);
                                    }
                                }
                                lastKeyboardState = ks;
                            }
                        }
                        else if (hasEnteredPortal)
                        {
                            hasEnteredPortal = false;
                            g.hintWords = null;
                        }
                    }
                    else if (fileName == "person1")
                    {
                        BasicAttackBehaviour(gameTime);
                    }
                    else if (fileName == "slime")
                    {
                        BasicAttackBehaviour(gameTime);
                    }
                    else if (fileName == "bigSlime")
                    {
                        BasicAttackBehaviour(gameTime);
                    }
                    else if (fileName == "rockman")
                    {
                        if (GetDistance(position, g.player.footPosition) > range && hasAttacked == false)
                        {
                            currentAnimation = AnimationType.walk;
                                velocity += GetNormalizedDirection(g.player.position, footPosition, speed * 0.2f);
                        }
                        else
                        {
                            if (hasAttacked == false)
                            {
                                deny.Play(0.2f, 0, 0);
                                reflectPosition = g.player.position;
                                hasAttacked = true;
                                isForward = true;
                                currentAnimation = AnimationType.attack;
                            }
                            else if (needAttackRest)
                            {
                                if (isPush)
                                {
                                    if (gameTime.TotalGameTime.TotalSeconds - attackChargingTime > 0.3f)
                                    {
                                        needAttackRest = false;
                                        hasAttacked = false;
                                        isPush = false;
                                    }
                                }
                                else
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
    }
}
