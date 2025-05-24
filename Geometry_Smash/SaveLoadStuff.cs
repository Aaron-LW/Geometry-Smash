using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Geometry_Smash;

public class LevelSerializer
{
    private readonly Game1 game1;

    public LevelSerializer(Game1 game2)
    {
        game1 = game2;
    }

    public static void SaveLevel(Level level, string fileName)
    {
        string dir = Path.Combine(Directory.GetCurrentDirectory(), "Levels");
        Directory.CreateDirectory(dir);

        LevelData levelData = new LevelData
        {
            Startpos = level.StartPos,
            Blocks = new List<BlockData>()
        };

        foreach (var block in level.BlockMap)
        {
            int blockType = block.Value.TextureIndex;
            levelData.Blocks.Add(new BlockData { Position = block.Value.Position, BlockType = blockType });
        }

        string filePath = Path.Combine(dir, fileName + ".json");

        var Json = JsonConvert.SerializeObject(levelData, Formatting.Indented);
        File.WriteAllText(filePath, Json);

        Console.WriteLine("Saved Level " + fileName);
    }

    public Level LoadLevel(string fileName)
    {
        string dir = Path.Combine(Directory.GetCurrentDirectory(), "Levels");
        string filePath = Path.Combine(dir, fileName + ".json");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found " + filePath);
        }

        var Json = File.ReadAllText(filePath);
        LevelData data = JsonConvert.DeserializeObject<LevelData>(Json);

        var blockMap = new Dictionary<Vector2, Entity>();
        var entities = new List<Entity>();
        var collider = new List<ColliderComponent>();

        entities.Add(game1.CreatePlayer());

        foreach (var block in data.Blocks)
        {
            Texture2D tex = Game1.Blocks[block.BlockType];
            entities.Add(new Entity(block.Position, block.BlockType, null, Game1.GlobalScale));

            ColliderComponent Collider = null;

            if (block.BlockType == 2)
            {
                Collider = new ColliderComponent(entities[entities.Count - 1], game1.ResetLevel, new System.Drawing.RectangleF(14f, 14f, 20, 30), true);
            }
            else
            {
                Collider = new ColliderComponent(entities[entities.Count - 1], game1.ResetLevel);
            }

            collider.Add(Collider);
            entities[entities.Count - 1].AddComponent(Collider);

            blockMap[new Vector2(block.Position.X, block.Position.Y)] = entities[entities.Count - 1];
        }

        Console.WriteLine("Loaded Level " + fileName);
        return new Level(new System.Numerics.Vector2(data.Startpos.X, data.Startpos.Y), blockMap, entities, collider);
    }
}

public class LevelData
{
    public Vector2 Startpos;
    public List<BlockData> Blocks = new();
}

public class BlockData
{
    public Vector2 Position;
    public int BlockType;
}