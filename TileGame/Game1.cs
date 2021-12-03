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
        public TileMap currentMap;

        public Texture2D[] tileCostumes;
        private int totalTile = 10, storingTileID;

        private Texture2D selectGrid, whiteRectangle;
        public Vector2 mouseGridPosition, mapOffset;
        public Vector2 mousePosition;
        private ButtonState LastMouseButtonState = ButtonState.Released;

        public bool clicked, isWithinMap;
        public List<Button> UIButtons = new List<Button>();
        public Rectangle mapRect;
        public enum GameState { tileMode, hitboxMode, playMode, entityMode, entityRemoveMode, entityPlantMode, characterMode, characterPlantMode, characterRemoveMode, createNewMapMode};
        public GameState gameState;
        public delegate void ClickAction();
        public static event ClickAction OnClick;

        public EntityTable entityTable;
        private int index, plantingEntityID;
        private Entity plantingEntity;
        private Character plantingCharacter;
        public List<Entity> mapEntities = new List<Entity>();

        public CharacterTable characterTable;
        public List<Character.CharacterBehaviour> mapCharacters = new List<Character.CharacterBehaviour>();

        public Player player;
        private SortingLayer[] sortingLayers;

        public Attack[] enemyAttacks;

        public InputField inputField;
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
            UIButtons.Add(new Button(this, "ToCreateNewMapMode", "To Create New Map Mode", new Vector2(5, 290)));
            Components.Add(UIButtons[7]);

            entityTable = new EntityTable(this);
            Components.Add(entityTable);

            characterTable = new CharacterTable(this);
            Components.Add(characterTable);

            player = new Player(this, new Vector2(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.4f));
            Components.Add(player);

            SetButtonActive(gameState, UIButtons[1].name);

            enemyAttacks = new Attack[8];
            for(int i = 0; i < enemyAttacks.Length; i++)
            {
                enemyAttacks[i] = new Attack(this);
                Components.Add(enemyAttacks[i]);
            }
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

            CreateNewMap("ONE", 20, 20);
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
        public void ClearMap()
        {
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
            for (int i = 0; i < mapCharacters.Count; i++)
            {
                Components.Remove(mapCharacters[i]);
                mapCharacters[i].Dispose();
            }
            if (mapCharacters.Count > 0)
                mapCharacters.Clear();
        }
        public void CreateNewMap(string mapName, int x = 10, int y = 10)
        {
            ClearMap();
            if (File.Exists(mapName + ".txt"))
            {
                TileMap tempMap = LoadTileMap(mapName);
                currentMap = new TileMap(this, 20, 20, mapName);
            }
            else
            {
                currentMap = new TileMap(this, x, y, mapName);
            }
            SetMapOffset();
        }
        public int GetInactiveAttack(ref Attack[] attacks)
        {
            for(int i = 0; i < attacks.Length; i++)
            {
                if ( attacks[i].isActive == false)
                    return i;
            }
            return -1;
        }
        private void SetMapOffset()
        {
            LoadMapEntity();
            for(int i = 0; i < currentMap.characterData.Count; i++)
            {
                mapCharacters.Add(new Character.CharacterBehaviour(this, currentMap.characterData[i].ID, new Vector2(currentMap.characterData[i].x, currentMap.characterData[i].y), currentMap.characterData[i]));
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
            mouseGridPosition = TileMap.ToGrid(mousePosition, currentMap.size);

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
                else if (gameState == GameState.characterPlantMode && plantingCharacter != null)
                {
                    plantingCharacter.position = mouseGridPosition;
                    if (clicked && index < currentMap.tileCostume.Count && index > -1)
                    {
                        Components.Remove(plantingCharacter);
                        currentMap.characterData.Add(new TileMap.CharacterData(plantingCharacter.ID, plantingCharacter.position.X, plantingCharacter.position.Y));
                        plantingCharacter.Dispose();
                        mapCharacters.Add(new Character.CharacterBehaviour(this, plantingCharacter.ID, plantingCharacter.position, currentMap.characterData[currentMap.characterData.Count - 1]));
                        Components.Add(mapCharacters[mapCharacters.Count - 1]);
                        if (plantingCharacter.fileName == "portal1")
                        {
                            inputField = new InputField(this, plantingCharacter.position + new Vector2(0, 1) * 25, GameState.characterMode, mapCharacters[mapCharacters.Count - 1]);
                            Components.Add(inputField);
                        }
                        else
                        {
                            SetButtonActive(GameState.characterMode, "ToCharacterModeButton");
                            characterTable.SetCharacterPage(characterTable.page);
                        }
                        plantingCharacter = null;
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
                if (gameState == GameState.entityPlantMode || gameState == GameState.characterPlantMode || gameState == GameState.createNewMapMode)
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
        public class SortingLayer
        {
            public float layerDepth;
            public enum ListName { entity, character, player}
            public ListName name;
            public SortingLayer(float layerDepth, ListName listName)
            {
                this.layerDepth = layerDepth;
                name = listName;
            }
        }
        private int SetSortingLayers()
        {
            float mapSize = currentMap.x * currentMap.y;
            int currentIndex = 0, totalObject = currentMap.entityData.Count + mapCharacters.Count + 1;
            float depth = 0;
            sortingLayers = new SortingLayer[totalObject];
            for(int i = 0; i < currentMap.entityData.Count; i++)
            {
                depth = (float)currentMap.entityData[i].indexPosition / mapSize;
                sortingLayers[currentIndex] = new SortingLayer(depth, SortingLayer.ListName.entity);
                currentIndex++;
            }
            for (int i = 0; i < mapCharacters.Count; i++)
            {
                depth = (float)mapCharacters[i].gridIndex / mapSize;
                sortingLayers[currentIndex] = new SortingLayer(depth, SortingLayer.ListName.character);
                currentIndex++;
            }
            depth = (float)player.gridIndex / mapSize;
            sortingLayers[currentIndex] = new SortingLayer(depth, SortingLayer.ListName.player);
            return totalObject;
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            // Draw Tiles
            for (int i = 0; i < currentMap.x * currentMap.y; i++)
            {
                int tileID = currentMap.tileCostume[i];
                _spriteBatch.Draw(tileCostumes[tileID], new Vector2(currentMap.GetTileX(i), currentMap.GetTileY(i)) * currentMap.size + mapOffset, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            }
            if (gameState == GameState.entityRemoveMode && mapEntities.Count > 0)
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
                        removeIndex = i;
                        break;
                    }
                }
                if (clicked && removeIndex != -1)
                {
                    currentMap.entityData.RemoveAt(removeIndex);
                }
            }
            else if (gameState == GameState.hitboxMode)
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
            else if (gameState == GameState.characterRemoveMode)
            {
                int removeIndex = -1;
                for(int i = 0; i < mapCharacters.Count; i++)
                {
                    Rectangle removeRect = new Rectangle((int)(mapCharacters[i].position.X - mapCharacters[i].center.X), (int)(mapCharacters[i].position.Y - mapCharacters[i].center.Y), mapCharacters[i].frameRect.Width, mapCharacters[i].frameRect.Height);
                    if (IsWithinRectangle(mousePosition, removeRect))
                    {
                        _spriteBatch.Draw(whiteRectangle, removeRect, Color.Salmon);
                        removeIndex = i;
                        break;
                    }
                }
                if (clicked && removeIndex != -1)
                {
                    currentMap.characterData.Remove(mapCharacters[removeIndex].respectiveData);
                    Components.Remove(mapCharacters[removeIndex]);
                    mapCharacters[removeIndex].Dispose();
                    mapCharacters[removeIndex] = null;
                    mapCharacters.Remove(mapCharacters[removeIndex]);
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
                    _spriteBatch.Draw(plantingEntity.entityDetails.entityWholeTexture, mouseGridPosition - plantingEntity.entityDetails.partialCenter - new Vector2(0, 1) * plantingEntity.entityDetails.entityWholeTexture.Height * 0.4f, plantingEntity.entityDetails.frameRect, Color.White);
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
                else if (gameState == GameState.characterPlantMode && plantingCharacter != null)
                {
                    _spriteBatch.Draw(plantingCharacter.texture, mouseGridPosition - plantingCharacter.center, plantingCharacter.frameRect, Color.White);
                }
            }
            _spriteBatch.Draw(whiteRectangle, mousePosition, null, Color.White, 0f, Vector2.Zero, 5, SpriteEffects.None, 0f);
            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.FrontToBack);
            int totalObject = SetSortingLayers();
            //Draw All Map Objects
            int currentEntityIndex = 0;
            int currentCharacterIndex = 0;
            for (int i = 0; i < totalObject; i++)
            {
                if (sortingLayers[i].name == SortingLayer.ListName.entity && mapEntities.Count > 0)
                {
                    int entityIndex = FindIndexWithNameInList(EntityTable.GetEntityStatus(currentMap.entityData[currentEntityIndex].ID).name, ref mapEntities);
                    Vector2 entityPosition = new Vector2(currentMap.GetTileX(currentMap.entityData[currentEntityIndex].indexPosition), currentMap.GetTileY(currentMap.entityData[currentEntityIndex].indexPosition)) * currentMap.size - mapEntities[entityIndex].entityDetails.partialCenter - new Vector2(0, 1) * mapEntities[entityIndex].entityDetails.entityWholeTexture.Height * 0.4f + mapOffset;
                    _spriteBatch.Draw(mapEntities[entityIndex].entityDetails.entityWholeTexture, entityPosition, mapEntities[entityIndex].entityDetails.frameRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, sortingLayers[i].layerDepth);
                    currentEntityIndex++;
                }
                else if (sortingLayers[i].name == SortingLayer.ListName.character)
                {
                    if (mapCharacters[currentCharacterIndex].isRight)
                    {
                        _spriteBatch.Draw(mapCharacters[currentCharacterIndex].texture, mapCharacters[currentCharacterIndex].position, mapCharacters[currentCharacterIndex].frameRect, Color.White, mapCharacters[currentCharacterIndex].rotate, mapCharacters[currentCharacterIndex].center, 1, SpriteEffects.None, sortingLayers[i].layerDepth);
                    }
                    else
                    {
                        _spriteBatch.Draw(mapCharacters[currentCharacterIndex].texture, mapCharacters[currentCharacterIndex].position, mapCharacters[currentCharacterIndex].frameRect, Color.White, mapCharacters[currentCharacterIndex].rotate, mapCharacters[currentCharacterIndex].center, 1, SpriteEffects.FlipHorizontally, sortingLayers[i].layerDepth);
                    }
                    currentCharacterIndex++;
                }
                else if (sortingLayers[i].name == SortingLayer.ListName.player)
                {
                    if (player.isRight)
                    {
                        _spriteBatch.Draw(player.texture, player.position - player.center, player.frameRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, sortingLayers[i].layerDepth);
                    }
                    else
                    {
                        _spriteBatch.Draw(player.texture, player.position - player.center, player.frameRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, sortingLayers[i].layerDepth);
                    }
                }
            }
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
