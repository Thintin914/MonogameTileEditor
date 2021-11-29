using System;
using System.Collections.Generic;
using System.Text;

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
        public TileMap(Game1 g , int x, int y, string mapName)
        {
            this.x = x;
            this.y = y;
            this.mapName = mapName;
            this.g = g;
            size = g.tileCostumes[0].Width;
            entityData = new List<EntityData>();
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

        public int GetTileX(int index)
        {
            return index % x;
        }

        public int GetTileY(int index)
        {
            return (int)MathF.Floor(index / x);
        }
        public int GetTileIndexFromPosition(float x, float y, float offsetX, float offsetY)
        {
            int xIndex = (int)((x - offsetX) / size);
            int yIndex = (int)((y - offsetY) / size);
            return yIndex * this.x + xIndex;
        }
        public int GetTileIndexFromNormalizedPosition(float x, float y)
        {
            return (int)(y * this.x + x);
        }
    }
}
