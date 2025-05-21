using EntitySystem;
using Geometry_Smash;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Entity 
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;
    public float Scale;
    
    public Texture2D Texture;
    public int TextureIndex;

    public Color Color = Color.White;

    public List<Component> Components = new List<Component>();

    public bool Hidden = false;

    public Entity(Vector2 position, int textureIndex = -1, Texture2D texture = null, float scale = 1f, float rotation = 0f)
    {
        if (texture != null) { Texture = texture; TextureIndex = -1; }
        else { Texture = Game1.Blocks[textureIndex]; TextureIndex = textureIndex; }

        Position = position;
        Scale = scale;
        Rotation = rotation;
        Color = Color.White;
    }
    
    public void Draw(SpriteBatch _spriteBatch, Vector2 CamPos)
    {
        ColliderComponent C = GetComponent<ColliderComponent>();
    
        if (!Hidden) 
        {
            _spriteBatch.Draw(Texture, Position + CamPos, null, Color, Rotation, new Vector2(Texture.Width / 2, Texture.Height / 2), Scale, SpriteEffects.None, 0f);
            
            if (C != null && Game1.Debug) 
            {
                var rectangleF = C.Hitbox.GetValueOrDefault();
            
                _spriteBatch.DrawRectangle(new RectangleF(new Vector2(rectangleF.Location.X, rectangleF.Location.Y) + Position + CamPos - new Vector2(Texture.Width / 2 + 16, Texture.Height / 2 + 16), new SizeF(rectangleF.Size.Width, rectangleF.Size.Height)), Color.Red, 1);
            }
        }
    }
    
    public T GetComponent<T>() where T : Component 
    {
        for (int i = 0; i < Components.Count; i++) 
        {
            if (Components[i] is T Component) 
            {
                return Component;
            }
        }

        Console.WriteLine("Couldn't find Component " + typeof(T).Name);
        return null;
    }
    
    public void AddComponent(Component Component) 
    {
        if (this.Components.Contains(Component)) { Console.WriteLine("Entity already has Component " + Component); return; }
    
        if (Component is ColliderComponent) { Game1.CurrLevel.Collider.Add(Component as ColliderComponent); }
        Components.Add(Component);
    }
    
    public void Tick() 
    {
        if (!Hidden) 
        {
            TickComponents();

            Position += Velocity;
            Velocity *= 0.9f;
        }
    }
    
    public void TickComponents() 
    {
        for (int i = 0; i < Components.Count; i++) 
        {
            if (Components[i].NeedsUpdate) 
            {
                Components[i].UpdateFunction.Invoke();
            }
        }
    }
}

namespace EntitySystem
{
    public class EntityUtils
    {
        public static void DrawEntities(SpriteBatch _spriteBatch, Vector2 CamPos) 
        {
            for (int i = 0; i < Game1.CurrLevel.Entities.Count; i++) 
            {
                Game1.CurrLevel.Entities[i].Draw(_spriteBatch, CamPos);
            }
        }
    
        public static Entity CreateEntity(Vector2 position, int textureIndex = -1, Texture2D texture = null, float scale = 1f, float rotation = 0f)
        {
            Game1.CurrLevel.Entities.Add(new Entity(position, textureIndex, texture, scale, rotation));
            return Game1.CurrLevel.Entities[Game1.CurrLevel.Entities.Count - 1];
        }
        
        public static void TickEntities() 
        {
            for (int i = 0; i < Game1.CurrLevel.Entities.Count; i++) 
            {
                Game1.CurrLevel.Entities[i].Tick();
            }
        }
        
        public static void RemoveEntity(Entity Entity) 
        {
            for (int i = 0; i < Entity.Components.Count; i++) 
            {
                if (Entity.Components[i] is ColliderComponent ColliderComponent) 
                {
                    Game1.CurrLevel.Collider.Remove(ColliderComponent);
                }
            }
        
            Entity.Components.Clear();
            Game1.CurrLevel.Entities.Remove(Entity);
        }
    }
}