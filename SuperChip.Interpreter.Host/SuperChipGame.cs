using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SuperChip11Interpreter.V3;
using System.Diagnostics;
using Color = Microsoft.Xna.Framework.Color;

namespace SuperChip.Interpreter.Host
{
    internal class SuperChipGame : Game
    {
        private GraphicsDeviceManager graphics;
        public SpriteBatch SpriteBatch { get; private set; }

        private Chip8Display display;
        private bool[] chip8board;

        SoundEffect se;
        private Chip8Interpreter interpreter;
        private SoundEffectInstance playingSound;
        private bool playSound;
        private TimeSpan soundDuration;


        public SuperChipGame()
        {
            this.graphics = new GraphicsDeviceManager(this);

            // E:\code\Chip8.CmdHost\Chip8.Files\progs\chip8-roms\games\Brick (Brix hack, 1990).ch8
        }



        private void ConfigureInterpreter(string fileName)
        {

            interpreter = new Chip8Interpreter(700, false);

            interpreter.Drawing += Interpreter_Drawing;
            interpreter.SoundOn += Interpreter_SoundOn;
            interpreter.SoundOff += Interpreter_SoundOff;
            var file = File.ReadAllBytes(fileName);
            interpreter.Load(file);

        }
        protected override void Initialize()
        {
            base.Initialize();
            this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.display = new Chip8Display(64, 32, new Vector2(0, 0), 10, 10, Color.BurlyWood, this.SpriteBatch);
            var file = File.ReadAllBytes("Content/Beep.wav");
            // this.ConfigureInterpreter(@"E:\code\Chip8.CmdHost\Chip8.Interpreter.V2.Host\progs\chip8-roms\games\Space Invaders [David Winter].ch8");
            this.ConfigureInterpreter(@"E:\code\Chip8.CmdHost\Chip8.Files\progs\chip8-roms\programs\Clock Program [Bill Fisher, 1981].ch8"); // @"E:\code\Chip8.CmdHost\Chip8.Files\progs\chip8-roms\games\Brick (Brix hack, 1990).ch8");
            //this.ConfigureInterpreter(@"E:\code\Chip8.CmdHost\Chip8.Files\progs\chip8-roms\programs\IBM Logo.ch8");

            se = new SoundEffect(file, 16000, AudioChannels.Stereo);
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
