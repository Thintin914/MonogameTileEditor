using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text.Json;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace TileGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private TileMap currentMap;

        public Texture2D[] tileCostumes;
        private int totalTile = 10, storingTileID;

        private Texture2D selectGrid, whiteRectangle;
        private Vector2 mouseGridPosition, mapOffset;
        public Vector2 mousePosition;
        private ButtonState LastMouseButtonState = ButtonState.Released;

        private bool clicked, isWithinMap;
        public List<Button> UIButtons = new List<Button>();
        private Rectangle mapRect;
        public enum GameState { tileMode, hitboxMode, playMode, entityMode, entityRemoveMode, entityPlantMode, characterMode, characterPlantMode, characterRemoveMode};
        public GameState gameState;
        public delegate void ClickAction();
        public static event ClickAction OnClick;

        public EntityTable entityTable;
        private int index, plantingEntityID, plantCharacterID;
        private Entity plantingEntity;
        private Character plantingCharacter;
        public List<Entity> mapEntities = new List<Entity>();

        public CharacterTable characterTable;
        public List<Character.CharacterBehaviour> mapCharacters = new List<Character.CharacterBehaviour>();
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            if (GraphicsDevice == null)
            {
                _graphics.ApplyChanges();
            }
            _graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            gameState = GameState.tileMode;

            UIButtons.Add(new Button(this, "ToPlayModeButton", "To Play Mode", new Vector2(5, 10)));
            Components.Add(UIButtons[0]);
            UIButtons.Add(new Button(this, "ToTileModeButton", "To Tile Mode", new Vector2(5, 50)));
            Components.Add(UIButtons[1]);
            UIButtons.Add(new Button(this, "ToHitboxModeButton", "To Hitbox Mode", new Vector2(5, 90)));
            Components.Add(UIButtons[2]);
            UIButtons.Add(new Button(this, "ToEntityModeButton", "To Entity Mode", new Vector2(5, 130)));
            Components.Add(UIButtons[3]);
            UIButtons.Add(new Button(this, "ToEntityRemoveModeButton", "To Entity Remove Mode", new Vector2(5, 170)));
            Components.Add(UIButtons[4]);
            UIButtons.Add(new Button(this, "ToCharacterMode", "To Character Mode", new Vector2(5, 210)));
            Components.Add(UIButtons[5]);
            UIButtons.Add(new Button(this, "ToCharacterRemoveMode", "To Character Remove Mode", new Vector2(5, 250)));
            Components.Add(UIButtons[6]);

            entityTable = new EntityTable(this);
            Components.Add(entityTable);

            characterTable = new CharacterTable(this);
            Components.Add(characterTable);

            SetButtonActive(gameState, UIButtons[1].name);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            selectGrid = Content.Load<Texture2D>("Tiles\\select");
            whiteRectangle = new Texture2D(GraphicsDevice, 1, 1);
            whiteRectangle.SetData(new[] { Color.White});

            tileCostumes = new Texture2D[totalTile];
            tileCostumes[0] = Content.Load<Texture2D>("Tiles\\empty");
            tileCostumes[1] = Content.Load<Texture2D>("Tiles\\grass");
            tileCostumes[2] = Content.Load<Texture2D>("Tiles\\water");
            tileCostumes[3] = Content.Load<Texture2D>("Tiles\\waterBottomLeft");
            tileCostumes[4] = Content.Load<Texture2D>("Tiles\\waterBottomRight");
            tileCostumes[5] = Content.Load<Texture2D>("Tiles\\waterTopLeft");
            tileCostumes[6] = Content.Load<Texture2D>("Tiles\\waterTopRight");
            tileCostumes[7] = Content.Load<Texture2D>("Tiles\\dirt");
            tileCostumes[8] = Content.Load<Texture2D>("Tiles\\dirtPath");
            tileCostumes[9] = Content.Load<Texture2D>("Tiles\\deepWater");

            currentMap = new TileMap(this, 10,10, "1");
            SetMapOffset();
        }
        private void LoadMapEntity()
        {
            List<int> loadedEntityID = new List<int>();
            for (int i = 0; i < mapEntities.Count; i++)
            {
                if (mapEntities[i] != null)
                {
                    Components.Remove(mapEntities[i]);
                    mapEntities[i].Dispose();
                }
            }
            if (mapEntities.Count > 0)
                mapEntities.Clear();
            for (int i = 0; i < currentMap.entityData.Count; i++)
            {
                if (!loadedEntityID.Contains(currentMap.entityData[i].ID))
                {
                    mapEntities.Add(new Entity(this, EntityTable.GetEntityStatus(currentMap.entityData[i].ID)));
                    Components.Add(mapEntities[i]);
                }
            }
        }
        private void SetMapOffset()
        {
            LoadMapEntity();
            for(int i = 0; i < currentMap.characterData.Count; i++)
            {
                mapCharacters.Add(new Character.CharacterBehaviour(this, currentMap.characterData[i].ID, new Vector2(currentMap.characterData[i].x, currentMap.characterData[i].y)));
                Components.Add(mapCharacters[mapCharacters.Count - 1]);
            }
            mapOffset = new Vector2(GraphicsDevice.Viewport.Width * 0.5f - currentMap.x * currentMap.size * 0.5f, GraphicsDevice.Viewport.Height * 0.5f - currentMap.y * currentMap.size * 0.5f);
            mapOffset.X = MathF.Round(mapOffset.X / currentMap.size) * currentMap.size;
            mapOffset.Y = MathF.Round(mapOffset.Y / currentMap.size) * currentMap.size;
            mapRect = new Rectangle((int)mapOffset.X, (int)mapOffset.Y, (int)(currentMap.size * (currentMap.x - 1)), (int)(currentMap.size * (currentMap.y - 1)));
        }
        protected override void UnloadContent()
        {
            base.UnloadContent();
            _spriteBatch.Dispose();
            whiteRectangle.Dispose();
        }
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Save(currentMap);
                Exit();
            }

            // TODO: Add your update logic here
            MouseState ms = Mouse.GetState();
            KeyboardState ks = Keyboard.GetState();
            mousePosition.X = ms.X;
            mousePosition.Y = ms.Y;
            mouseGridPosition.X = MathF.Round(ms.X / currentMap.size) * currentMap.size;
            mouseGridPosition.Y = MathF.Round(ms.Y / currentMap.size) * currentMap.size;

            if (IsWithinRectangle(mouseGridPosition, mapRect))
            {
                isWithinMap = true;
            }
            else
            {
                isWithinMap = false;
            }
            if (ms.LeftButton == ButtonState.Pressed && LastMouseButtonState == ButtonState.Released)
            {
                clicked = true;
                if (OnClick != null)
                {
                    OnClick.Invoke();
                }
            }
            else
            {
                clicked = false;
            }
            LastMouseButtonState = ms.LeftButton;

            if (gameState == GameState.characterPlantMode)
            {
                plantingCharacter.position = mouseGridPosition;
            }
            if (isWithinMap)
            {
                index = currentMap.GetTileIndexFromPosition(mouseGridPosition.X, mouseGridPosition.Y, mapOffset.X, mapOffset.Y);
                if (gameState == GameState.tileMode)
                {
                    if (ks.IsKeyDown(Keys.E))
                    {
                        storingTileID = currentMap.tileCostume[currentMap.GetTileIndexFromPosition(mouseGridPosition.X, mouseGridPosition.Y, mapOffset.X, mapOffset.Y)];
                    }
                    if (ks.IsKeyDown(Keys.Space))
                    {
                        if (index < currentMap.tileCostume.Count && index > -1)
                        {
                            currentMap.tileCostume[index] = storingTileID;
                        }
                    }
                    if (clicked && index < currentMap.tileCostume.Count && index > -1)
                    {
                        currentMap.tileCostume[index] = (currentMap.tileCostume[index] + 1) % totalTile;
                    }
                }
                else if (gameState == GameState.hitboxMode)
                {
                    if (clicked && index < currentMap.tileCostume.Count && index > -1)
                    {
                        currentMap.hitbox[index] = (currentMap.hitbox[index] + 1) % 2;
                    }
                }
                else if (gameState == GameState.characterPlantMode)
                {
                    if (clicked && index < currentMap.tileCostume.Count && index > -1)
                    {
                        Components.Remove(plantingCharacter);
                        currentMap.characterData.Add(new TileMap.CharacterData(plantingCharacter.ID, plantingCharacter.position.X, plantingCharacter.position.Y));
                        plantingCharacter.Dispose();
                        mapCharacters.Add(new Character.CharacterBehaviour(this, plantingCharacter.ID, plantingCharacter.position));
                        Components.Add(mapCharacters[mapCharacters.Count - 1]);
                        plantingCharacter = null;
                        SetButtonActive(GameState.characterMode, "ToCharacterModeButton");
                        characterTable.SetCharacterPage(characterTable.page);
                    }
                }
            }

            base.Update(gameTime);
        }
        public static bool IsWithinRectangle(Vector2 targetPosition, Rectangle rect)
        {
            if (targetPosition.X >= rect.X && targetPosition.X <= rect.X + rect.Width && targetPosition.Y >= rect.Y && targetPosition.Y <= rect.Y + rect.Height)
            {
                return true;
            }
            return false;
        }
        public void SetButtonActive(GameState gameState, string inactiveName)
        {
            for(int i = 0; i < UIButtons.Count; i++)
            {
                if (UIButtons[i].name == inactiveName)
                {
                    UIButtons[i].isActive = false;
                }
                else
                {
                    UIButtons[i].isActive = true;
                }
                if (gameState == GameState.entityPlantMode || gameState == GameState.characterPlantMode)
                {
                    UIButtons[i].isHidden = true;
                }
                else
                {
                    UIButtons[i].isHidden = false;
                }
                if (UIButtons[i].name == "ToEntityUpButton" || UIButtons[i].name == "ToEntityDownButton")
                {
                    if (gameState != GameState.entityMode)
                    {
                        UIButtons[i].isHidden = true;
                    }
                    else
                    {
                        UIButtons[i].isHidden = false;
                    }
                }
                if (UIButtons[i].name == "ToCharacterUpButton" || UIButtons[i].name == "ToCharacterDownButton")
                {
                    if (gameState != GameState.characterMode)
                    {
                        UIButtons[i].isHidden = true;
                    }
                    else
                    {
                        UIButtons[i].isHidden = false;
                    }
                }
            }
            this.gameState = gameState;
        }
        public void PlantEntityOnTileMap(int plantEntityIndex)
        {
            plantingEntityID = plantEntityIndex;
            if (plantingEntity != null)
            {
                plantingEntity.Dispose();
            }
            plantingEntity = new Entity(this, EntityTable.GetEntityStatus(plantingEntityID));
            plantingEntity.entityDetails.entityWholeTexture = Content.Load<Texture2D>("GameEntities\\" + plantingEntity.entityDetails.fileName);
        }
        public void PlantCharacterOnTileMap(int plantCharacterIndex)
        {
            plantCharacterID = plantCharacterIndex;
            if (plantingCharacter != null)
            {
                plantingCharacter.Dispose();
            }
            plantingCharacter = new Character(this, plantCharacterIndex, mousePosition);
            Components.Add(plantingCharacter);
        }
        private int FindIndexWithNameInList(string name , ref List<Entity> entities)
        {
            for(int i = 0; i < entities.Count; i++)
            {
                if (entities[i].entityDetails.fileName == name)
                    return i;
            }
            return 0;
        }
        private bool IsMapEntitiesReloadRequired(ref List<Entity> mapEntities, int requiredID)
        {
            for(int i = 0; i < mapEntities.Count; i++)
            {
                if (mapEntities[i].entityDetails.ID == requiredID)
                    return false;
            }
            return true;
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            for (int i = 0; i < currentMap.x * currentMap.y; i++)
            {
                int tileID = currentMap.tileCostume[i];
                _spriteBatch.Draw(tileCostumes[tileID], new Vector2(currentMap.GetTileX(i), currentMap.GetTileY(i)) * currentMap.size + mapOffset, Color.White);
            }
            if (gameState == GameState.entityRemoveMode)
            {
                int removeIndex = -1;
                for(int i = 0; i < currentMap.entityData.Count; i++)
                {
                    int entityIndex = FindIndexWithNameInList(EntityTable.GetEntityStatus(currentMap.entityData[i].ID).name, ref mapEntities);
                    Vector2 cornerPosition = new Vector2(currentMap.GetTileX(currentMap.entityData[i].indexPosition), currentMap.GetTileY(currentMap.entityData[i].indexPosition)) * currentMap.size + mapOffset - mapEntities[entityIndex].entityDetails.partialCenter - new Vector2(0,1) * mapEntities[entityIndex].entityDetails.frameRect.Height * 0.4f;
                    Rectangle removeRect = new Rectangle((int)cornerPosition.X, (int)cornerPosition.Y, mapEntities[entityIndex].entityDetails.frameRect.Width, mapEntities[entityIndex].entityDetails.frameRect.Height);
                    if (IsWithinRectangle(mousePosition, removeRect))
                    {
                        _spriteBatch.Draw(whiteRectangle, removeRect, Color.Salmon);
                        if (clicked)
                        {
                            removeIndex = i;
                        }
                    }
                }
                if (removeIndex != -1)
                {
                    currentMap.entityData.RemoveAt(removeIndex);
                }
            }
            for (int i = 0; i < currentMap.entityData.Count; i++)
            {
                int entityIndex = FindIndexWithNameInList(EntityTable.GetEntityStatus(currentMap.entityData[i].ID).name, ref mapEntities);
                Vector2 entityPosition = new Vector2(currentMap.GetTileX(currentMap.entityData[i].indexPosition), currentMap.GetTileY(currentMap.entityData[i].indexPosition)) * currentMap.size - mapEntities[entityIndex].entityDetails.partialCenter - new Vector2(0, 1) * mapEntities[entityIndex].entityDetails.entityWholeTexture.Height * 0.4f;
                _spriteBatch.Draw(mapEntities[entityIndex].entityDetails.entityWholeTexture, entityPosition + mapOffset, mapEntities[entityIndex].entityDetails.frameRect, Color.White);
            }
            if (gameState == GameState.hitboxMode)
            {
                for (int i = 0; i < currentMap.x * currentMap.y; i++)
                {
                    int tileID = currentMap.hitbox[i];
                    if (tileID == 1)
                    {
                        _spriteBatch.Draw(selectGrid, new Vector2(currentMap.GetTileX(i), currentMap.GetTileY(i)) * currentMap.size + mapOffset + Vector2.One * currentMap.size * 0.5f, null, Color.Salmon, 0f, Vector2.One * currentMap.size * 0.5f, MathF.Abs(MathF.Sin((float)gameTime.TotalGameTime.TotalSeconds * 2) * 1.1f), SpriteEffects.None, 0f);
                    }
                }
            }
            if (isWithinMap)
            {
                if (gameState == GameState.tileMode || gameState == GameState.hitboxMode)
                {
                    _spriteBatch.Draw(selectGrid, mouseGridPosition, Color.White);
                }
                if (gameState == GameState.entityPlantMode)
                {
                    _spriteBatch.Draw(plantingEntity.entityDetails.entityWholeTexture, mouseGridPosition - plantingEntity.entityDetails.partialCenter - new Vector2(0,1) * plantingEntity.entityDetails.entityWholeTexture.Height * 0.4f, plantingEntity.entityDetails.frameRect, Color.White);
                    if (clicked)
                    {
                        currentMap.entityData.Add(new TileMap.EntityData(plantingEntityID, index));
                        if (IsMapEntitiesReloadRequired(ref mapEntities, plantingEntityID))
                        {
                            LoadMapEntity();
                        }
                        Components.Remove(plantingCharacter);
                        plantingEntity.Dispose();
                        plantingEntity = null;
                        SetButtonActive(GameState.entityMode, "ToEntityModeButton");
                    }
                }
            }
            _spriteBatch.Draw(whiteRectangle, mousePosition, null, Color.White, 0f, Vector2.Zero, 5, SpriteEffects.None, 0f);
            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void Save(TileMap map)
        {
            string serializedText = JsonSerializer.Serialize<TileMap>(map);
            File.WriteAllText(map.mapName + ".txt", serializedText);
        }

        public TileMap LoadTileMap(string mapName)
        {
            var fileContents = File.ReadAllText(mapName + ".txt");
            return JsonSerializer.Deserialize<TileMap>(fileContents);
        }
    }
}
