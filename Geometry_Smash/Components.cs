using System;
using System.Drawing;
using Geometry_Smash;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public class Component 
{
    public Entity Parent;

    public bool NeedsUpdate;
    public Action UpdateFunction;

    public Component(Entity parent, bool needsUpdate = false, Action updateFunction = null)
    {
        Parent = parent;

        NeedsUpdate = needsUpdate;
        UpdateFunction = updateFunction;
    }
}

public class GravityComponent : Component 
{
    public float Acceleration;
    public float YVel;

    public GravityComponent(Entity parent, float acceleration) : base(parent, true, null)
    {
        Acceleration = acceleration;
        UpdateFunction = Update;
    }
    
    public void Update() 
    {
        YVel += Acceleration;
        Parent.Velocity.Y += YVel;
    }
}

public class ColliderComponent : Component 
{
    public bool isStatic;
    public bool OnGround;

    public bool AlwaysKill;

    public Action ResetFunction;
    public RectangleF? Hitbox;
    
    public ColliderComponent(Entity parent, Action resetFunction, RectangleF? hitbox = null, bool alwaysKill = false, bool isstatic = true) : base(parent, !isstatic, null)
    {
        UpdateFunction = Update;
        isStatic = isstatic;
        ResetFunction = resetFunction;
        AlwaysKill = alwaysKill;
        
        if (hitbox == null) { Hitbox = new RectangleF(0f, 0f, Parent.Texture.Width * Parent.Scale, Parent.Texture.Height * Parent.Scale); }
        else { Hitbox = hitbox; }
    }

    private float previousBottom;
    private float previousRight;
    
    public void Update() 
    {
        OnGround = false;

        Vector2 thisPosition = Parent.Position;
        var thisHitbox = Hitbox.GetValueOrDefault();

        float thisBottom = thisHitbox.Bottom + Parent.Position.Y;
        float thisRight = thisHitbox.Right + Parent.Position.X;
        
        GravityComponent GravityComponente = Parent.GetComponent<GravityComponent>(); 

        for (int i = 0; i < Game1.CurrLevel.Collider.Count; i++) 
        {
            ColliderComponent other = Game1.CurrLevel.Collider[i];
            if (other == this) { continue; }
            
            Vector2 otherPosition = other.Parent.Position;

            RectangleF otherRectangle = other.Hitbox.GetValueOrDefault();

            float otherBottom = otherRectangle.Bottom + other.Parent.Position.Y;
            float otherLeft = otherRectangle.Left + other.Parent.Position.X;
            float otherRight = otherRectangle.Right + other.Parent.Position.X;

            float otherTop = otherRectangle.Top + other.Parent.Position.Y;         
                        
            //float otherBottom = otherPosition.Y + other.Parent.Texture.Height * other.Parent.Scale;
            //float otherRight = otherPosition.X + other.Parent.Texture.Width * other.Parent.Scale;
            
            if (thisBottom > otherTop && thisRight > otherLeft && thisPosition.X < otherRight && thisPosition.Y < otherBottom) 
            {
                if (other.AlwaysKill)
                {
                    ResetFunction.Invoke();
                }

                float UpdatedRight = thisRight;
                float UpdatedBottom = thisBottom;
            
                if (previousBottom - 1 <= otherTop) 
                {
                    if (GravityComponente != null) 
                    {   
                        GravityComponente.YVel = 0f;
                    } 

                    Parent.Position.Y = otherTop - Parent.Texture.Height * Parent.Scale; 
                    Parent.Velocity.Y = 0f;

                    UpdatedRight = otherTop - Parent.Texture.Height * Parent.Scale + (Parent.Texture.Width * Parent.Scale);
                    
                    OnGround = true;
                }

                if (UpdatedRight > otherTop && otherTop < UpdatedRight !& thisBottom - otherTop > 10f) 
                {
                    ResetFunction.Invoke();
                }
            }
        }

        previousBottom = thisBottom;
        previousRight = thisRight;
    }
}

public class CharacterControllerComponent : Component 
{
    public float JumpStrength;

    public CharacterControllerComponent(Entity parent, float jumpStrength) : base(parent, true, null) 
    {
        UpdateFunction = Update;
        JumpStrength = jumpStrength;
    }
    
    public void Update() 
    {
        ColliderComponent ColliderComponent = Parent.GetComponent<ColliderComponent>();
        
        if (ColliderComponent != null) 
        {
            KeyboardState KeyboardState = Keyboard.GetState();
        
            if (ColliderComponent.OnGround == true) 
            {
                if (KeyboardState.IsKeyDown(Keys.Space)) 
                {
                    Parent.Velocity.Y -= JumpStrength;
                }
            }
        }
    }
}