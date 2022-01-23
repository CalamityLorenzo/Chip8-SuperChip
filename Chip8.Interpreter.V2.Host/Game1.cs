using Chip8.Interpreter.V2.Host;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using Color = Microsoft.Xna.Framework.Color;

namespace Chip8.Interpreter.V2
{
    public class Game1 : Game
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

        public Game1()
        {
            this.graphics = new GraphicsDeviceManager(this);
        }


        private void ConfigureInterpreter(string fileName)
        {
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
            //SetBoard();

            //var ibmBits = File.ReadAllBytes("progs/test_opcode.ch8");
            //var ibmBits = File.ReadAllBytes("progs/chip8-roms/games/Submarine [Carmelo Cortez, 1978].ch8");
            //var ibmBits = File.ReadAllBytes("progs/DrawFont.ch8");
            //var ibmBits = File.ReadAllBytes("progs/chip8-roms/demos/Stars [Sergey Naydenov, 2010].ch8");
            var file = File.ReadAllBytes("Content/Beep.wav");
            this.ConfigureInterpreter(@"E:\code\Chip8.CmdHost\Chip8.Interpreter.V2.Host\progs\chip8-roms\games\Space Invaders [David Winter].ch8");
            this.ConfigureInterpreter(@"E:\code\Chip8.CmdHost\Chip8.Interpreter.V2.Host\progs\chip8-roms\games\Space Invaders [David Winter].ch8");
            // E:\code\Chip8.CmdHost\Chip8.Files\progs\chip8-roms\programs\IBM Logo.ch8
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

        protected override void LoadContent()
        {
            base.LoadContent();
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

        private void CreateBeep()
        {
            playingSound.Play();
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
