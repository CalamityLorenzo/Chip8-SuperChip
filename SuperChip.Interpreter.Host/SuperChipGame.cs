using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SuperChip11Interpreter.V3;
using System.Diagnostics;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input;

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


        public SuperChipGame(SuperChipSettings settings)
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.SuperChipSettings = settings;
            // E:\code\Chip8.CmdHost\Chip8.Files\progs\chip8-roms\games\Brick (Brix hack, 1990).ch8
        }



        private void ConfigureInterpreter(string fileName)
        {

            interpreter = new SuperChipInterpreter(0, 7, this.SuperChipSettings.Switches.LoadStoreQuirk,
                 this.SuperChipSettings.Switches.ShiftQuirk, 
                 this.SuperChipSettings.Switches.JumpQuirk);

            interpreter.Drawing += Interpreter_Drawing;
            interpreter.SoundOn += Interpreter_SoundOn;
            interpreter.SoundOff += Interpreter_SoundOff;
            var file = File.ReadAllBytes(fileName);

                        byte[] program = new byte[]
              {
                    0x00,0xF,  // Enable hi-res
                    0x60,0x05,
                    0x61,0x05, // Position 25
                    0xA0,000,
                    0xD0,0x11,
                    0x60,0x07,
                    0xD0,0x11,
               };
            interpreter.Load(program);
    
        }
        protected override void Initialize()
        {
            base.Initialize();
            this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.display = new Chip8Display(64, 32, new Vector2(0, 0), 10, 10, Color.BurlyWood, this.SpriteBatch);
            // var file = File.ReadAllBytes("/home/pi/Chip8-SuperChip/SuperChip.Interpreter.Host/Content/beep.wav");
            var beepWav = File.ReadAllBytes(this.SuperChipSettings.Sound);
            // this.ConfigureInterpreter(@"E:\code\Chip8.CmdHost\Chip8.Interpreter.V2.Host\progs\chip8-roms\games\Space Invaders [David Winter].ch8");
            //var fileName = "/home/pi/Chip8-SuperChip/Chip8.Files/progs/chip8-roms/games/Bowling [Gooitzen van der Wal].ch8";
            // var fileName="/home/pi/Chip8-SuperChip/Chip8.Files/progs/chip8-roms/hires/Hires Maze [David Winter, 199x].ch8";
            var fileName = this.SuperChipSettings.Rom;
            this.ConfigureInterpreter(fileName);
            //this.ConfigureInterpreter(@"E:\code\Chip8.CmdHost\Chip8.Files\progs\chip8-roms\programs\IBM Logo.ch8");
            se = new SoundEffect(beepWav, 16000, AudioChannels.Mono);
            this.playingSound = se.CreateInstance();

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

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var state = Keyboard.GetState();
            var pressedKeys = state.GetPressedKeys();

            if (pressedKeys.Contains(Keys.Keys.Escape))
                this.Exit();

            char? pressedChar = pressedKeys.Length > 0 ? (char)pressedKeys[0] : null;

            if (interpreter != null) interpreter.PressedKey(pressedChar);
            this.display.Update(gameTime, this.chip8board);

            if (interpreter != null) interpreter.Tick();

            if (this.playSound) CreateBeep();
        }

        protected override void Draw(GameTime gameTime)
        {
            this.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.OrangeRed);
            this.SpriteBatch.Begin();

            this.display.Draw(gameTime);
            this.SpriteBatch.End();
        }
    }
}
