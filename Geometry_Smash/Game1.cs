using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EntitySystem;
using System;
using System.Collections.Generic;
using System.IO;
using MonoGame.Extended;
using System.Text;

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

    public Vector2 EndPos;
    public Entity End;

    private Entity SelectingThing;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        //IsFixedTimeStep = false;
        //_graphics.SynchronizeWithVerticalRetrace = false;

        Window.AllowUserResizing = true;

        var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        _graphics.PreferredBackBufferWidth = displayMode.Width;
        _graphics.PreferredBackBufferHeight = displayMode.Height;

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

        End = new Entity(EndPos, -1, Content.Load<Texture2D>("GS End Better"), GlobalScale);

        SelectingThing = new Entity(new Vector2(), -1, Content.Load<Texture2D>("SelectThing"), GlobalScale);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        Blocks.Add(Content.Load<Texture2D>("DefaultBlock"));
        Blocks.Add(Content.Load<Texture2D>("GradientBlock"));
        Blocks.Add(Content.Load<Texture2D>("Spike"));
        Blocks.Add(Content.Load<Texture2D>("RandomBlock"));
        Blocks.Add(Content.Load<Texture2D>("ConnectedBlock3"));
        Blocks.Add(Content.Load<Texture2D>("ConnectedBlock5"));
        Blocks.Add(Content.Load<Texture2D>("ConnectedBlock6"));
        Blocks.Add(Content.Load<Texture2D>("ConnectedBlock7"));
        Blocks.Add(Content.Load<Texture2D>("ConnectedBlock8"));
        Blocks.Add(Content.Load<Texture2D>("ConnectedBlock9"));
        Blocks.Add(Content.Load<Texture2D>("BigGroundSpikes"));
        Blocks.Add(Content.Load<Texture2D>("SmallGroundSpikes"));

        font = Content.Load<SpriteFont>("font");
    }


    private MouseState PreviousMouseState;
    private KeyboardState PreviousKeyboardState;
    public static int StartDelay;

    string Name = "";
    private bool inputingText = false;

    private bool SavedLevel = false;

    private bool Win = false;
    private bool StopCam = false;

    protected override void Update(GameTime gameTime)
    {
        MouseState MouseState = Mouse.GetState();
        KeyboardState KeyboardState = Keyboard.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape) && !inputingText)
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

            if (PreviousMouseState.LeftButton == ButtonState.Pressed)
            {
                if (KeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    PlaceBlock(MouseState.Position.X, MouseState.Position.Y);
                }

                if (MouseState.LeftButton == ButtonState.Released)
                {
                    PlaceBlock(MouseState.Position.X, MouseState.Position.Y);
                }
            }

            if (PreviousMouseState.RightButton == ButtonState.Pressed)
            {
                if (KeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    RemoveBlock(MouseState.Position.X, MouseState.Position.Y);
                }

                if (MouseState.RightButton == ButtonState.Released)
                {
                    RemoveBlock(MouseState.Position.X, MouseState.Position.Y);
                }
            }

            if (MouseState.MiddleButton == ButtonState.Released && PreviousMouseState.MiddleButton == ButtonState.Pressed)
            {
                Cube.Position.X = MouseState.Position.X - CamPos.X;
                Cube.Position.Y = MouseState.Position.Y - CamPos.Y;
            }
        }

        if (KeyboardState.IsKeyUp(Keys.Z) && PreviousKeyboardState.IsKeyDown(Keys.Z) && !inputingText)
        {
            Debug = !Debug;
        }

        if (KeyboardState.IsKeyUp(Keys.C) && PreviousKeyboardState.IsKeyDown(Keys.C) && !inputingText)
        {
            LevelSelect = !LevelSelect;
        }

        if (KeyboardState.IsKeyUp(Keys.F) && PreviousKeyboardState.IsKeyDown(Keys.F) && !inputingText)
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
        else if (LevelSelect && !inputingText)
        {
            string dir = "";

            if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Levels")))
            {
                dir = Path.Combine(Directory.GetCurrentDirectory(), "Levels");
            }
            else
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/Levels");
                Console.WriteLine("machdirecotry");

                dir = Path.Combine(Directory.GetCurrentDirectory(), "Levels");
            }

            int FileAmount = Directory.GetFiles(dir).Length;

            if (KeyboardState.IsKeyUp(Keys.Down) && PreviousKeyboardState.IsKeyDown(Keys.Down) && !inputingText)
            {
                if (SelectedLevel + 1 != FileAmount)
                {
                    SelectedLevel++;
                }
            }
            if (KeyboardState.IsKeyUp(Keys.Up) && PreviousKeyboardState.IsKeyDown(Keys.Up) && !inputingText)
            {
                if (SelectedLevel - 1 != -1)
                {
                    SelectedLevel--;
                }
            }

            var Files = Directory.GetFiles(dir);

            if (KeyboardState.IsKeyUp(Keys.Enter) && PreviousKeyboardState.IsKeyDown(Keys.Enter) && !inputingText)
            {
                CurrLevel = SaveLoadStuff.LoadLevel(Path.GetFileNameWithoutExtension(Files[SelectedLevel]));
                CurrLevelName = Path.GetFileNameWithoutExtension(Files[SelectedLevel]);

                LevelSelect = false;
                LevelEditor = true;
            }

            if (KeyboardState.IsKeyUp(Keys.X) && PreviousKeyboardState.IsKeyDown(Keys.X) && !inputingText)
            {
                if (File.Exists(Files[SelectedLevel]))
                {
                    File.Delete(Files[SelectedLevel]);
                }
            }

            if (KeyboardState.IsKeyUp(Keys.D) && PreviousKeyboardState.IsKeyDown(Keys.D) && !inputingText)
            {
                if (File.Exists(Files[SelectedLevel]) && !File.Exists(Path.Combine(dir, Path.GetFileNameWithoutExtension(Files[SelectedLevel]) + " Copy.json")))
                {
                    File.Copy(Files[SelectedLevel], Path.Combine(dir, Path.GetFileNameWithoutExtension(Files[SelectedLevel]) + " Copy.json"));
                }
            }

            if (KeyboardState.IsKeyUp(Keys.N) && PreviousKeyboardState.IsKeyDown(Keys.N) && !inputingText)
            {
                inputingText = true;
            }
        }

        if (KeyboardState.IsKeyUp(Keys.U) && PreviousKeyboardState.IsKeyDown(Keys.U) && !inputingText)
        {
            if (CurrLevelName != String.Empty)
            {
                LevelSerializer.SaveLevel(CurrLevel, CurrLevelName);
                SavedLevel = true;
            }
            else
            {
                LevelSerializer.SaveLevel(CurrLevel, "Unnamed");
                SavedLevel = true;
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
                Cube.Velocity.X += 1.2f;

                if (!Win)
                {
                    if (CamPos.Y > -Cube.Position.Y + _graphics.PreferredBackBufferHeight / 2 + 40)
                    {
                        CamPos.Y -= 1f * MathF.Abs(-Cube.Position.Y + _graphics.PreferredBackBufferHeight / 2 - CamPos.Y) / 10;
                    }
                    if (CamPos.Y < -Cube.Position.Y + _graphics.PreferredBackBufferHeight / 2 - 40)
                    {
                        CamPos.Y += 1f * MathF.Abs(-Cube.Position.Y + _graphics.PreferredBackBufferHeight / 2 - CamPos.Y) / 10;
                    }
                }
            }

            if (!StopCam) { CamPos.X = -Cube.Position.X + 100; }
        }
        else
        {
            Cube.Hidden = true;
        }

        if (inputingText)
        {
            if (PreviousKeyboardState.IsKeyDown(Keys.Escape) && KeyboardState.IsKeyUp(Keys.Escape))
            {
                inputingText = false;
                Name = "";
            }

            Name = GetKeyboardInput(KeyboardState, PreviousKeyboardState, new StringBuilder(Name));

            if (KeyboardState.IsKeyUp(Keys.Enter) && PreviousKeyboardState.IsKeyDown(Keys.Enter))
            {
                inputingText = false;
                CurrLevel = new Level(new System.Numerics.Vector2(0, 0), new Dictionary<Vector2, Entity>(), new List<Entity>(), new List<ColliderComponent>());
                LevelSerializer.SaveLevel(CurrLevel, Name);
                CurrLevelName = Name;

                Name = String.Empty;
                LevelSelect = false;
            }
        }

        End.Position.X = EntityUtils.GetRightMostBlockPosition() + End.Texture.Width / 2 * GlobalScale;
        End.Position.Y = -CamPos.Y + End.Texture.Height / 2 * GlobalScale;

        if (!LevelEditor && !LevelSelect)
        {
            if (Cube.Position.X > End.Position.X - End.Texture.Width / 1.2 * GlobalScale)
            {
                StopCam = true;
            }

            if (Cube.Position.X > End.Position.X - End.Texture.Width / 2 * GlobalScale)
            {
                Win = true;
            }
        }

        if (Win && KeyboardState.IsKeyUp(Keys.Space) && PreviousKeyboardState.IsKeyDown(Keys.Space))
        {
            ResetLevel();
        }

        SelectingThing.Position = GetGridPos(MouseState.Position.X, MouseState.Position.Y);

        PreviousMouseState = MouseState;
        PreviousKeyboardState = KeyboardState;
        base.Update(gameTime);
    }

    private int SaveCounter = 0;

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

        if (SavedLevel)
        {
            SaveCounter = 120;
            SavedLevel = false;
        }

        if (SaveCounter > 0)
        {
            SaveCounter--;
            _spriteBatch.DrawString(font, "Saved Level", new Vector2(20, 120), Color.White);
        }

        if (!LevelSelect)
        {
            EntityUtils.DrawEntities(_spriteBatch, CamPos);

            if (!LevelEditor)
            {
                End.Draw(_spriteBatch, CamPos);
                if (Win)
                {
                    _spriteBatch.DrawString(font, "Win", new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2), Color.LimeGreen, 0f, new Vector2(font.MeasureString("Win").X / 2, font.MeasureString("Win").Y / 2), 10f, SpriteEffects.None, 0);
                    _spriteBatch.DrawString(font, "Press space to restart", new Vector2(GraphicsDevice.Viewport.Width / 2 - font.MeasureString("Press space to restart").X / 2 * 2, GraphicsDevice.Viewport.Height / 2 + 120), Color.LimeGreen, 0f, new Vector2(), 2f, SpriteEffects.None, 0);
                }
            }

            if (LevelEditor)
            {
                _spriteBatch.Draw(Content.Load<Texture2D>("Gometry"), CurrLevel.StartPos + CamPos, null, Color.White, 0f, new Vector2(), GlobalScale, SpriteEffects.None, 0f);

                _spriteBatch.DrawString(font, "Level Editor", new Vector2(20, 20), Color.White);

                _spriteBatch.Draw(Blocks[CurrBlock], new Vector2(20, 55), null, Color.White, 0f, new Vector2(0, 0), 3f, SpriteEffects.None, 0f);
                _spriteBatch.DrawString(font, CurrBlock.ToString(), new Vector2(80, 60), Color.White);

                SelectingThing.Draw(_spriteBatch, CamPos);
            }
        }
        else
        {
            string dir = Path.Combine(Directory.GetCurrentDirectory(), "Levels");
            var Files = Directory.GetFiles(dir);

            _spriteBatch.DrawString(font, "Enter - Load Level", new Vector2(150, GraphicsDevice.Viewport.Height - 100), Color.White);
            _spriteBatch.DrawString(font, "D - Duplicate Level", new Vector2(GraphicsDevice.Viewport.Width / 2 - 200 - font.MeasureString("D - Duplicate Level").X / 2, GraphicsDevice.Viewport.Height - 100), Color.White);
            _spriteBatch.DrawString(font, "N - New Level", new Vector2(GraphicsDevice.Viewport.Width / 2 + 200 - font.MeasureString("N - New Level").X / 2, GraphicsDevice.Viewport.Height - 100), Color.White);
            _spriteBatch.DrawString(font, "X - Delete Level", new Vector2(GraphicsDevice.Viewport.Width - 300 - font.MeasureString("X - Delete Level").X / 2, GraphicsDevice.Viewport.Height - 100), Color.White);

            if (inputingText)
            {
                _spriteBatch.DrawString(font, "Input new level's name", new Vector2(GraphicsDevice.Viewport.Width / 2 - font.MeasureString("Input new level's name").X / 2, GraphicsDevice.Viewport.Height / 2 - 50), Color.White);

                _spriteBatch.DrawString(font, Name, new Vector2(GraphicsDevice.Viewport.Width / 2 - font.MeasureString(Name.ToString()).X / 2, GraphicsDevice.Viewport.Height / 2), Color.White);
                _spriteBatch.DrawRectangle(new RectangleF(GraphicsDevice.Viewport.Width / 2 - font.MeasureString(Name.ToString()).X / 2 - 5, GraphicsDevice.Viewport.Height / 2 - 5, font.MeasureString(Name).X + 10, font.MeasureString(Name).Y + 10), Color.White, 2);

                _spriteBatch.DrawString(font, "Press escape to cancel", new Vector2(GraphicsDevice.Viewport.Width / 2 - font.MeasureString("Input new level's name").X / 2, GraphicsDevice.Viewport.Height / 2 + 50), Color.White);
            }

            for (int i = 0; i < Files.Length; i++)
            {
                string Name = Path.GetFileNameWithoutExtension(Files[i]);
                _spriteBatch.DrawString(font, Name, new Vector2(GraphicsDevice.Viewport.Width / 2 - font.MeasureString(Name).X / 2, 100 + i * 30), Color.White);

                if (i == SelectedLevel && !inputingText)
                {
                    _spriteBatch.DrawRectangle(new RectangleF(GraphicsDevice.Viewport.Width / 2 - font.MeasureString(Name).X / 2, 100 + i * 30, font.MeasureString(Path.GetFileNameWithoutExtension(Files[i])).X, font.MeasureString(Path.GetFileNameWithoutExtension(Files[i])).Y), Color.White, 2);
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
                CreatedEntity.AddComponent(new ColliderComponent(CreatedEntity, ResetLevel, new System.Drawing.RectangleF(25f, 24f, 30, 45), true));
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

        Win = false;
        StopCam = false;

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

    public string GetKeyboardInput(KeyboardState CurrentKeyboardState, KeyboardState OldKeyboardState, StringBuilder Current)
    {
        StringBuilder InputText = Current;

        foreach (var key in CurrentKeyboardState.GetPressedKeys())
        {
            if (!OldKeyboardState.IsKeyDown(key))
            {
                if (key == Keys.Back && InputText.Length > 0)
                {
                    InputText.Length--;
                }
                else if (key == Keys.Space)
                {
                    InputText.Append(' ');
                }
                else if (key >= Keys.A && key <= Keys.Z)
                {
                    bool shift = CurrentKeyboardState.IsKeyDown(Keys.LeftShift) || CurrentKeyboardState.IsKeyDown(Keys.RightShift);
                    char c = (char)(key - Keys.A + (shift ? 'A' : 'a'));
                    InputText.Append(c);
                }
                else if (key >= Keys.D0 && key <= Keys.D9)
                {
                    InputText.Append((char)(key - Keys.D0 + '0'));
                }
            }
        }

        return InputText.ToString();
    }

    public Vector2 GetGridPos(float x, float y)
    {
        float gridSize = 16 * GlobalScale;
        float adjustedX = (float)Math.Floor((x - CamPos.X) / gridSize) * gridSize + 8 * GlobalScale;
        float adjustedY = (float)Math.Floor((y - CamPos.Y) / gridSize) * gridSize + 8 * GlobalScale;

        return new Vector2(adjustedX, adjustedY);
    }
}
