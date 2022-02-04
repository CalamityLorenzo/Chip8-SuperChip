﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SuperChip11Interpreter.V3;
using System.Diagnostics;
using System.Text;
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

            if (!this.SuperChipSettings.SuperChipEnabled){
                interpreter = new SuperChipInterpreter(0, 7, this.SuperChipSettings.Switches.LoadStoreQuirk,
                     this.SuperChipSettings.Switches.ShiftQuirk,
                     this.SuperChipSettings.Switches.JumpQuirk);
            }
            else
                interpreter = new SuperChipInterpreter(0, 7, true);

            interpreter.Drawing += Interpreter_Drawing;
            interpreter.SoundOn += Interpreter_SoundOn;
            interpreter.SoundOff += Interpreter_SoundOff;

            this.display = this.SuperChipSettings.SuperChipEnabled 
                                        ? new Chip8Display(128, 64, new Vector2(0, 0), 5, 5, Color.BurlyWood, this.SpriteBatch)
                                        : new Chip8Display(64, 32, new Vector2(0, 0), 10, 10, Color.BurlyWood, this.SpriteBatch);

            var file = File.ReadAllBytes(fileName);

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
            this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);
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

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var state = Keyboard.GetState();
            var pressedKeys = state.GetPressedKeys();

            if (pressedKeys.Contains(Keys.Keys.Escape))
                this.Exit();

            if (pressedKeys.Contains(Keys.Keys.Left))
                this.DumpDisplay();

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
