using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace TileGame
{
    public class TileMap
    {
        public int x{ get; set; }
        public int y { get; set; }
        public enum MapState { undefined, defined};
        public MapState mapState;
        public string mapName;
        public List<int> tileCostume { get; set; }
        public List<int> hitbox { get; set; }
        public float size;
        private Game1 g;
        public List<EntityData> entityData { get; set; }
        public List<CharacterData> characterData { get; set; }
        public class EntityData
        {
            public int ID { get; set; }
            public int indexPosition { get; set; }

            public EntityData (int ID, int indexPosition)
            {
                this.ID = ID;
                this.indexPosition = indexPosition;
            }
            public EntityData() { }
        }
        public class CharacterData
        {
            public int ID { get; set; }
            public float x { get; set; }
            public float y { get; set; }
            public string extra { get; set;}
            public CharacterData(int ID, float x, float y)
            {
                this.ID = ID;
                this.x = x;
                this.y = y;

            }
            public CharacterData() { }
        }
        public TileMap(Game1 g , int x, int y, string mapName)
        {
            this.x = x;
            this.y = y;
            this.mapName = mapName;
            this.g = g;
            size = g.tileCostumes[0].Width;
            entityData = new List<EntityData>();
            characterData = new List<CharacterData>();
            if (System.IO.File.Exists(mapName + ".txt"))
            {
                mapState = MapState.defined;
                TileMap map = g.LoadTileMap(mapName);
                if (map.x != x || map.y != y)
                {
                    tileCostume = new List<int>(new int[x * y]);
                    hitbox = new List<int>(new int[x * y]);
                }
                else
                {
                    tileCostume = map.tileCostume;
                    hitbox = map.hitbox;
                    if (map.entityData != null)
                    {
                        entityData = map.entityData;
                    }
                    if(map.characterData != null)
                    {
                        characterData = map.characterData;
                    }
                }
            }
            else
            {
                mapState = MapState.undefined;
                tileCostume = new List<int>(new int[x * y]);
                hitbox = new List<int>(new int[x * y]);
            }
        }
        // For Json Deserialization
        public TileMap() { }

        public static Vector2 ToGrid(Vector2 position, float size)
        {
            return new Vector2 (MathF.Round(position.X / size) * size, MathF.Round(position.Y / size) * size);
        }

        public int GetTileX(int index)
        {
            return index % x;
        }

        public int GetTileY(int index)
        {
            return (int)MathF.Floor(index / x);
        }
        public int GetTileIndexFromPosition(float xPos, float yPos, float offsetX, float offsetY)
        {
            int xIndex = (int)((xPos - offsetX) / size);
            int yIndex = (int)((yPos - offsetY) / size);
            return yIndex * x + xIndex;
        }
        public int GetTileIndexFromNormalizedPosition(float x, float y)
        {
            return (int)(y * this.x + x);
        }
    }
}
