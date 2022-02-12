using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SuperChip.Interpreter.Host.UI;
using SuperChip11Interpreter.V3;
using System.Diagnostics;
using System.Text;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input;
using Point = Microsoft.Xna.Framework.Point;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;

namespace SuperChip.Interpreter.Host
{
    internal class SuperChipGame : Game
    {
        private GraphicsDeviceManager graphics;

        public SuperChipSettings SuperChipSettings { get; }
        public SpriteBatch SpriteBatch { get; private set; }

        private Chip8Display display;
        private bool[] chip8board;

        SoundEffect se;
        private SuperChipInterpreter interpreter;
        private SoundEffectInstance playingSound;
        private bool playSound;
        private TimeSpan soundDuration;

        private string MouseCoords = "";
        SpriteFont spr;
        private SpriteFont sprSmall;
        SpriteFont sprArial;
        FilePicker fp;
        WindowContainer wc;
        private bool isReleased;

        public SuperChipGame(SuperChipSettings settings)
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "./Content";
            this.SuperChipSettings = settings;
            // E:\code\Chip8.CmdHost\Chip8.Files\progs\chip8-roms\games\Brick (Brix hack, 1990).ch8

        }


        private void ConfigureInterpreter()
        {

            if (!this.SuperChipSettings.SuperChipEnabled)
            {
                interpreter = new SuperChipInterpreter(0, 7, this.SuperChipSettings.Switches.LoadStoreQuirk,
                     this.SuperChipSettings.Switches.ShiftQuirk,
                     this.SuperChipSettings.Switches.JumpQuirk);
                this.display = new Chip8Display(64, 32, new Point(0, 40), 10, 10, Color.BurlyWood, this.SpriteBatch);
            }
            else
            {
                interpreter = new SuperChipInterpreter(0, 7, true);
                this.display = new Chip8Display(128, 64, new Point(0, 40), 5, 5, Color.BurlyWood, this.SpriteBatch);
            }
            interpreter.Drawing += Interpreter_Drawing;
            interpreter.SoundOn += Interpreter_SoundOn;
            interpreter.SoundOff += Interpreter_SoundOff;

            this.interpreter.Pause=true;
        }

        private void LoadFile(byte[] file)
        {
            byte[] program = new byte[]
            {
                    0x00,0xFF,  // Enable hi-res
                    0x60,0x00, // 5
                    0x61,0x00, // 5
                    0xA0,0x00,
                    0x63,0x00,
                    0xF3,0x30,
                    0xD0,0x1A,
                    0x60,0x1F, // 5
                    0x61,0x05, // 5
                    0x63,0x01,
                    0xF3,0x30,
                    0xD0,0x1A,
                    // 0x60,0x09, // 7
                    // 0x61,0x05,  // 6
                    // 0xD0,0x11,
                    0x12,0x18
                };
            interpreter.Load(file);
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.IsMouseVisible = true;
            this.graphics.IsFullScreen = false;
            this.graphics.PreferredBackBufferWidth = 800;
            this.graphics.PreferredBackBufferHeight = 800;
            this.graphics.ApplyChanges();
            this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);
            // var file = File.ReadAllBytes("/home/pi/Chip8-SuperChip/SuperChip.Interpreter.Host/Content/beep.wav");
            var beepWav = File.ReadAllBytes(this.SuperChipSettings.Sound);

            this.spr = Content.Load<SpriteFont>("bin/Windows/consolas");
            this.sprSmall = Content.Load<SpriteFont>("bin/Windows/ConsolasSmall");
            this.sprArial = Content.Load<SpriteFont>("bin/Windows/Arial");

            var fileName = this.SuperChipSettings.Rom;

            this.ConfigureInterpreter();
            if (!String.IsNullOrEmpty(fileName))
            {
                var file = File.ReadAllBytes(fileName);
                this.LoadFile(file);
            }
            //this.ConfigureInterpreter(@"E:\code\Chip8.CmdHost\Chip8.Files\progs\chip8-roms\programs\IBM Logo.ch8");
            se = new SoundEffect(beepWav, 16000, AudioChannels.Mono);
            this.playingSound = se.CreateInstance();
            fp = new FilePicker(this.SpriteBatch, this.sprSmall, new(40, 40, 300, 350),this.SuperChipSettings.FilePickerDirectory ?? System.Environment.CurrentDirectory  );
            fp.OnFileSelected += this.FileSelected;

            wc = new WindowContainer(this.SpriteBatch, new Vector2(this.graphics.PreferredBackBufferWidth / 2 - 200, 0), new Vector2(200, 800), "Windows Box", sprArial, Color.BlueViolet, Color.DarkGray, Color.White, 3, 2, true);
        }

        private void FileSelected(object? sender, FileSelectedEventArgs e)
        {
            if (e.Filename.EndsWith("ch8"))
            {
                var file = File.ReadAllBytes(e.Filename);
                this.interpreter.Load(file);
                this.fp.Visible = false;
                this.display.Visible = true;
                this.interpreter.Pause=false;
            }
            else
            {
                Debug.WriteLine("Not a chip8 game.");
            }
        }

        private void Interpreter_SoundOff(object? sender, EventArgs e)
        {
            this.playSound = false;
            this.playingSound.Stop(true);
            this.soundDuration = new TimeSpan(DateTime.Now.Ticks) - this.soundDuration;

            Debug.WriteLine($"Client length : {soundDuration}");
        }

        private void Interpreter_SoundOn(object? sender, EventArgs e)
        {
            this.playSound = true;
            this.soundDuration = new TimeSpan(DateTime.Now.Ticks);
        }

        private void Interpreter_Drawing(object? sender, Chip8DrawingEventArgs e)
        {
            if (chip8board == null) chip8board = new bool[e.Display.Length];
            e.Display.CopyTo(this.chip8board, 0);
        }

        private void CreateBeep()
        {
            playingSound.Play();
        }

        private void DumpDisplay()
        {

            var display = this.interpreter.Display;
            var sb = new StringBuilder();
            if (this.interpreter.SuperChipEnabled)
            {
                for (var x = 0; x < 64; ++x)
                {
                    var items = display.Skip(x * 128).Take(64);
                    sb.AppendJoin(",", items);
                    sb.AppendLine();
                }
            }

            File.WriteAllText(Path.Combine(System.Environment.CurrentDirectory, "mapdump.csv"), sb.ToString());
        }

        private void UpdateMouseCoords(int x, int y)
        {
            this.MouseCoords = $"X : {x} Y : {y}";
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var MouseState = Mouse.GetState();
            this.UpdateMouseCoords(MouseState.X, MouseState.Y);

            if (this.IsActive && MouseState.LeftButton == ButtonState.Pressed && isReleased == true)
            {
                isReleased = false;
                fp.Click(MouseState.X, MouseState.Y);
            }

            var state = Keyboard.GetState();
            var pressedKeys = state.GetPressedKeys();

            if (pressedKeys.Contains(Keys.Keys.Escape))
                this.Exit();

            if (pressedKeys.Contains(Keys.Keys.Left))
                this.DumpDisplay();

            if (pressedKeys.Contains(Keys.Keys.F1))
                this.DisplayFilePicker();                

            char? pressedChar = pressedKeys.Length > 0 ? (char)pressedKeys[0] : null;

            if (interpreter != null) interpreter.PressedKey(pressedChar);
            this.display.Update(gameTime, this.chip8board);

            if (interpreter != null) interpreter.Tick();

            if (this.playSound) CreateBeep();

            fp.Update(gameTime);
            wc.Update(gameTime);
            if (MouseState.LeftButton == ButtonState.Released && isReleased == false) { isReleased = true; }
        }

        private void DisplayFilePicker()
        {
            this.fp.Visible= true;
            this.interpreter.Pause = true;
            this.display.Reset();
            this.display.Visible=false;
        }

        protected override void Draw(GameTime gameTime)
        {
            this.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.OrangeRed);
            this.SpriteBatch.Begin();
            this.SpriteBatch.DrawString(this.sprArial, MouseCoords, new Vector2(this.graphics.PreferredBackBufferWidth - 200, 0), Color.Black);


            fp.Draw(gameTime);
            //  wc.Draw(gameTime);
            if (this.display.Visible)
                this.display.Draw(gameTime);
            this.SpriteBatch.End();
        }
    }
}
