using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EntitySystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using MonoGame.Extended;

namespace Geometry_Smash;

public class Game1 : Game
{
    public static List<Texture2D> Blocks = new List<Texture2D>();
    private int CurrBlock = 0;

    public static bool Debug = false;

    public Vector2 CamPos;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Entity Cube;

    private double _elapsedTime;
    private int _frameCounter;
    private int _fps;

    private SpriteFont font;

    public static Level CurrLevel;

    private bool LevelEditor = true;

    private readonly LevelSerializer SaveLoadStuff;

    private bool LevelSelect = false;
    private int SelectedLevel;
    private string CurrLevelName = "";

    public static float GlobalScale = 5f;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        //IsFixedTimeStep = false;
        //_graphics.SynchronizeWithVerticalRetrace = false;

        Window.AllowUserResizing = true;

        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();

        SaveLoadStuff = new LevelSerializer(this);
    }

    protected override void Initialize()
    {
        CurrLevel = new Level(new System.Numerics.Vector2(0, 0), new Dictionary<Vector2, Entity>(), new List<Entity>(), new List<ColliderComponent>());

        Cube = EntityUtils.CreateEntity(new Vector2(0, 0), -1, Content.Load<Texture2D>("Gometry"), GlobalScale);
        Cube.AddComponent(new GravityComponent(Cube, 0.2f));
        Cube.AddComponent(new ColliderComponent(Cube, ResetLevel, null, false, false));
        Cube.AddComponent(new CharacterControllerComponent(Cube, 25));

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        Blocks.Add(Content.Load<Texture2D>("DefaultBlock"));
        Blocks.Add(Content.Load<Texture2D>("GradientBlock"));
        Blocks.Add(Content.Load<Texture2D>("Spike"));
        Blocks.Add(Content.Load<Texture2D>("RandomBlock"));
        Blocks.Add(Content.Load<Texture2D>("SmolSpike"));

        font = Content.Load<SpriteFont>("font");
    }


    private MouseState PreviousMouseState;
    private KeyboardState PreviousKeyboardState;
    public static int StartDelay;

    protected override void Update(GameTime gameTime)
    {
        MouseState MouseState = Mouse.GetState();
        KeyboardState KeyboardState = Keyboard.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
        _frameCounter++;

        if (_elapsedTime >= 1.0)
        {
            _fps = _frameCounter;
            _frameCounter = 0;
            _elapsedTime = 0;

            //Console.WriteLine($"FPS: {_fps}");
        }

        if (LevelEditor && !LevelSelect)
        {
            if (KeyboardState.IsKeyDown(Keys.W))
            {
                CamPos.Y += 30f;
            }
            if (KeyboardState.IsKeyDown(Keys.S))
            {
                CamPos.Y -= 30f;
            }
            if (KeyboardState.IsKeyDown(Keys.A))
            {
                CamPos.X += 30f;
            }
            if (KeyboardState.IsKeyDown(Keys.D))
            {
                CamPos.X -= 30f;
            }

            if (MouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)
            {
                PlaceBlock(MouseState.Position.X, MouseState.Position.Y);
            }

            if (MouseState.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed)
            {
                RemoveBlock(MouseState.Position.X, MouseState.Position.Y);
            }

            if (MouseState.MiddleButton == ButtonState.Released && PreviousMouseState.MiddleButton == ButtonState.Pressed)
            {
                Cube.Position.X = MouseState.Position.X - CamPos.X;
                Cube.Position.Y = MouseState.Position.Y - CamPos.Y;
            }
        }

        if (KeyboardState.IsKeyUp(Keys.Z) && PreviousKeyboardState.IsKeyDown(Keys.Z))
        {
            Debug = !Debug;
        }

        if (KeyboardState.IsKeyUp(Keys.C) && PreviousKeyboardState.IsKeyDown(Keys.C))
        {
            LevelSelect = !LevelSelect;
        }

        if (KeyboardState.IsKeyUp(Keys.F) && PreviousKeyboardState.IsKeyDown(Keys.F))
        {
            if (LevelEditor == true)
            {
                ResetLevel();
            }
            else
            {
                LevelEditor = true;
                CamPos = CurrLevel.StartPos + new System.Numerics.Vector2((_graphics.PreferredBackBufferWidth / 2) - 200, _graphics.PreferredBackBufferHeight / 2 - 50);
            }
        }

        if (LevelEditor && !LevelSelect)
        {
            if (KeyboardState.IsKeyUp(Keys.Up) && PreviousKeyboardState.IsKeyDown(Keys.Up))
            {
                if (CurrBlock + 1 != Blocks.Count)
                {
                    CurrBlock++;
                }
            }
            if (KeyboardState.IsKeyUp(Keys.Down) && PreviousKeyboardState.IsKeyDown(Keys.Down))
            {
                if (CurrBlock - 1 != -1)
                {
                    CurrBlock--;
                }
            }
        }
        else if (LevelSelect)
        {
            string dir = Path.Combine(Directory.GetCurrentDirectory(), "Levels");
            int FileAmount = Directory.GetFiles(dir).Length;

            if (KeyboardState.IsKeyUp(Keys.Down) && PreviousKeyboardState.IsKeyDown(Keys.Down))
            {
                if (SelectedLevel + 1 != FileAmount)
                {
                    SelectedLevel++;
                }
            }
            if (KeyboardState.IsKeyUp(Keys.Up) && PreviousKeyboardState.IsKeyDown(Keys.Up))
            {
                if (SelectedLevel - 1 != -1)
                {
                    SelectedLevel--;
                }
            }

            var Files = Directory.GetFiles(dir);

            if (KeyboardState.IsKeyUp(Keys.Enter) && PreviousKeyboardState.IsKeyDown(Keys.Enter))
            {
                CurrLevel = SaveLoadStuff.LoadLevel(Path.GetFileNameWithoutExtension(Files[SelectedLevel]));
                CurrLevelName = Path.GetFileNameWithoutExtension(Files[SelectedLevel]);

                LevelSelect = false;
                LevelEditor = true;
            }

            if (KeyboardState.IsKeyUp(Keys.X) && PreviousKeyboardState.IsKeyDown(Keys.X))
            {
                if (File.Exists(Files[SelectedLevel]))
                {
                    File.Delete(Files[SelectedLevel]);
                }
            }
        }

        if (KeyboardState.IsKeyUp(Keys.U) && PreviousKeyboardState.IsKeyDown(Keys.U))
        {
            if (CurrLevelName != String.Empty)
            {
                LevelSerializer.SaveLevel(CurrLevel, CurrLevelName);
            }
            else
            {
                LevelSerializer.SaveLevel(CurrLevel, "Unnamed");
            }
        }

        //Cube.Position.X = Mouse.GetState().Position.X;
        //Cube.Position.Y = Mouse.GetState().Position.Y;

        if (!LevelEditor)
        {
            if (StartDelay > 0)
            {
                StartDelay--;
            }
            else
            {
                EntityUtils.TickEntities();
                Cube.Velocity.X += 1f;

                if (CamPos.Y > -Cube.Position.Y + _graphics.PreferredBackBufferHeight + 40)
                {
                    CamPos.Y -= 1f * MathF.Abs(-Cube.Position.Y + _graphics.PreferredBackBufferHeight - CamPos.Y) / 10;
                }
                if (CamPos.Y < -Cube.Position.Y + _graphics.PreferredBackBufferHeight - 40)
                {
                    CamPos.Y += 1f * MathF.Abs(-Cube.Position.Y + _graphics.PreferredBackBufferHeight - CamPos.Y) / 10;
                }
            }

            CamPos.X = -Cube.Position.X + 100;
        }
        else
        {
            Cube.Hidden = true;
        }

        PreviousMouseState = MouseState;
        PreviousKeyboardState = KeyboardState;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (LevelSelect)
        {
            GraphicsDevice.Clear(Color.Gray);
        }
        else
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
        }

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        if (!LevelSelect)
        {
            EntityUtils.DrawEntities(_spriteBatch, CamPos);

            if (LevelEditor)
            {
                _spriteBatch.Draw(Content.Load<Texture2D>("Gometry"), CurrLevel.StartPos + CamPos, null, Color.White, 0f, new Vector2(), GlobalScale, SpriteEffects.None, 0f);

                _spriteBatch.DrawString(font, "Level Editor", new Vector2(20, 20), Color.White);

                _spriteBatch.Draw(Blocks[CurrBlock], new Vector2(20, 50), null, Color.White, 0f, new Vector2(0, 0), 3f, SpriteEffects.None, 0f);
                _spriteBatch.DrawString(font, CurrBlock.ToString(), new Vector2(80, 60), Color.White);
            }
        }
        else
        {
            string dir = Path.Combine(Directory.GetCurrentDirectory(), "Levels");
            var Files = Directory.GetFiles(dir);

            _spriteBatch.DrawString(font, SelectedLevel.ToString(), new Vector2(50, 50), Color.White);

            _spriteBatch.DrawString(font, "Enter - Load Level", new Vector2(150, GraphicsDevice.Viewport.Height - 100), Color.White);
            _spriteBatch.DrawString(font, "N - New Level", new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height - 100), Color.White);
            _spriteBatch.DrawString(font, "X - Delete Level", new Vector2(GraphicsDevice.Viewport.Width - 300, GraphicsDevice.Viewport.Height - 100), Color.White);

            for (int i = 0; i < Files.Length; i++)
            {
                _spriteBatch.DrawString(font, Path.GetFileNameWithoutExtension(Files[i]), new Vector2(GraphicsDevice.Viewport.Width / 2, 100 + i * 30), Color.White);

                if (i == SelectedLevel)
                {
                    _spriteBatch.DrawRectangle(new RectangleF(GraphicsDevice.Viewport.Width / 2, 100 + i * 30, font.MeasureString(Path.GetFileNameWithoutExtension(Files[i])).X, font.MeasureString(Path.GetFileNameWithoutExtension(Files[i])).Y), Color.White, 2);
                }
            }
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }


    public void PlaceBlock(float x, float y)
    {
        float gridSize = 16 * GlobalScale;
        float adjustedX = (float)Math.Floor((x - CamPos.X) / gridSize) * gridSize + 8 * GlobalScale;
        float adjustedY = (float)Math.Floor((y - CamPos.Y) / gridSize) * gridSize + 8 * GlobalScale;

        Vector2 Position = new Vector2(adjustedX, adjustedY);

        if (!CurrLevel.BlockMap.ContainsKey(Position))
        {
            Entity CreatedEntity = EntityUtils.CreateEntity(new Vector2(adjustedX, adjustedY), CurrBlock, null, GlobalScale);

            if (CurrBlock == 2)
            {
                CreatedEntity.AddComponent(new ColliderComponent(CreatedEntity, ResetLevel, new System.Drawing.RectangleF(14f, 14f, 20, 30), true));
            }
            else
            {
                CreatedEntity.AddComponent(new ColliderComponent(CreatedEntity, ResetLevel));
            }

            CurrLevel.BlockMap[Position] = CreatedEntity;
        }
    }

    public void RemoveBlock(float x, float y)
    {
        float gridSize = 16 * GlobalScale;
        float adjustedX = (float)Math.Floor((x - CamPos.X) / gridSize) * gridSize + 8 * GlobalScale;
        float adjustedY = (float)Math.Floor((y - CamPos.Y) / gridSize) * gridSize + 8 * GlobalScale;

        Vector2 Position = new Vector2(adjustedX, adjustedY);

        if (CurrLevel.BlockMap.ContainsKey(Position))
        {
            EntityUtils.RemoveEntity(CurrLevel.BlockMap[Position]);
            CurrLevel.BlockMap.Remove(Position);
        }
    }

    public void ResetLevel()
    {
        StartDelay = 100;
        LevelEditor = false;
        Cube.Hidden = false;

        Cube.Position = CurrLevel.StartPos + new System.Numerics.Vector2(8f * GlobalScale, 8f * GlobalScale);
        GravityComponent g = Cube.GetComponent<GravityComponent>();
        if (g != null)
        {
            g.YVel = 0f;
        }

        Cube.Velocity = Vector2.Zero;
    }

    public Entity CreatePlayer()
    {
        Cube = new Entity(new Vector2(), -1, Content.Load<Texture2D>("Gometry"), GlobalScale);

        Cube.AddComponent(new GravityComponent(Cube, 0.2f));
        Cube.AddComponent(new ColliderComponent(Cube, ResetLevel, null, false, false));
        Cube.AddComponent(new CharacterControllerComponent(Cube, 25));

        return Cube;
    }
}
