namespace Chip8.Interpreter.V2
{

    public class Chip8DrawingEventArgs : EventArgs
    {
        public bool[] Display;
        public Chip8DrawingEventArgs(bool[] display)
        {
            Display = display;
        }
    }

    public partial class Chip8Interpreter
    {
        public void ClearDisplay()
        {
            this.Display = new bool[this.Display.Length];
            OnDrawingEventRaised();
        }

        private void Initialise()
        {
            // init the registers to 0
            for (var x = 0; x < 16; ++x)
            {
                this.Registers.Add((byte)x, 0);
            }

            // I regiser
            IndexRegister = 0;


            // BuIld out the font.
            // This can be stored anywhere in the first 512 bytes.
            var zero = new byte[] { 0xF0, 0x90, 0x90, 0x90, 0xF0, }; // 0
            var one = new byte[] { 0x20, 0x60, 0x20, 0x20, 0x70 }; // 1
            var two = new byte[] { 0xF0, 0x10, 0xF0, 0x80, 0xF0 }; // 2
            var three = new byte[] { 0xF0, 0x10, 0xF0, 0x10, 0xF0 }; // 3
            var four = new byte[] { 0x90, 0x90, 0xF0, 0x10, 0x10 }; // 4
            var five = new byte[] { 0xF0, 0x80, 0xF0, 0x10, 0xF0 }; // 5
            var six = new byte[] { 0xF0, 0x80, 0xF0, 0x90, 0xF0 }; // 6
            var seven = new byte[] { 0xF0, 0x10, 0x20, 0x40, 0x40 }; // 7
            var eight = new byte[] { 0xF0, 0x90, 0xF0, 0x90, 0xF0 }; // 8
            var nine = new byte[] { 0xF0, 0x90, 0xF0, 0x10, 0xF0 }; // 9
            var ten = new byte[] { 0xF0, 0x90, 0xF0, 0x90, 0x90 }; // A
            var Aa = new byte[] { 0xE0, 0x90, 0xE0, 0x90, 0xE0 }; // B
            var be = new byte[] { 0xF0, 0x80, 0x80, 0x80, 0xF0 }; // C
            var ce = new byte[] { 0xE0, 0x90, 0x90, 0x90, 0xE0 }; // D
            var de = new byte[] { 0xF0, 0x80, 0xF0, 0x80, 0xF0 }; // E
            var eff = new byte[] { 0xF0, 0x80, 0xF0, 0x80, 0x80 };  // F

            Array.Copy(zero, 0, Memory, 0, zero.Length);
            Array.Copy(one, 0, Memory, 5, one.Length);
            Array.Copy(two, 0, Memory, 10, two.Length);
            Array.Copy(three, 0, Memory, 15, three.Length);
            Array.Copy(four, 0, Memory, 20, four.Length);
            Array.Copy(five, 0, Memory, 25, five.Length);
            Array.Copy(six, 0, Memory, 30, six.Length);
            Array.Copy(seven, 0, Memory, 35, seven.Length);
            Array.Copy(eight, 0, Memory, 40, eight.Length);
            Array.Copy(nine, 0, Memory, 45, nine.Length);
            Array.Copy(ten, 0, Memory, 50, ten.Length);
            Array.Copy(Aa, 0, Memory, 55, Aa.Length);
            Array.Copy(be, 0, Memory, 60, be.Length);
            Array.Copy(ce, 0, Memory, 65, ce.Length);
            Array.Copy(de, 0, Memory, 70, de.Length);
            Array.Copy(eff, 0, Memory, 75, eff.Length);

            this.keyMappings = new Dictionary<char, byte>
                    {
                        {'1', 1 },
                        {'2', 2 },
                        {'3', 3 },
                        {'4', 0xC },
                        {'Q', 4 },
                        {'W', 5 },
                        {'E', 6 },
                        {'R', 0xD },
                        {'A', 7 },
                        {'S', 8 },
                        {'D', 9 },
                        {'F', 0xE },
                        {'Z', 0xA },
                        {'X', 0x0 },
                        {'C', 0xB },
                        {'V', 0xF },
                    };

        }

        public void Draw(byte xPos, byte yPos, byte spriteHeight)
        {
            int screenWidth = 64;
            int screenHeight = 32;

            // the x and y can easily be over rated, we need to moduo them back,
            var x = xPos % screenWidth;
            var y = yPos % screenHeight;
            this.Registers[0xF] = 0;

            // pixel to draw
            for (var spriteRow = 0; spriteRow < spriteHeight; ++spriteRow)
            {
                var rowData = this.Memory[this.IndexRegister + spriteRow];
                // iterate over the display memory in rows
                for (var i = 0; i < 8; ++i)
                {
                    if (x > screenWidth - 1) break;
                    if (x + i < screenWidth && y+spriteRow< screenHeight)
                    {
                        var idx = (x + i) + ((y + spriteRow) * 64);
                        var screenPixel = this.Display[idx];
                        var rowPixel = (rowData & (1 << 7 - i)) != 0;
                        if (rowPixel & screenPixel)
                        {
                            this.Registers[0xf] = 1;
                            this.Display[idx] = false;
                        }
                        if (!screenPixel && rowPixel == true)
                            this.Display[idx] = true;
                    }
                }
            }

            this.OnDrawingEventRaised();
        }

        /// <summary>
        /// Tells the interpreter which keys are being pressed at the moment.
        /// </summary>
        /// <param name="pressedChar"></param>
        public void PressedKey(char? pressedChar)
        {
            if (pressedChar is not null && this.keyMappings.ContainsKey(pressedChar.Value))
            {
                this.CurrentKey = this.keyMappings[pressedChar.Value];
            }
            else
            {
                this.CurrentKey = null;

            }
        }

        private void OnSoundOnEventRasied()
        {
            if (this.SoundOn != null)
            {
                
                this.SoundOn(this, new EventArgs());
            }

            SoundOnLength = new TimeSpan(DateTime.Now.Ticks);
        }

        private void OnSoundOffEventRasied()
        {
            if (this.SoundOff != null)
            {
                this.SoundOff(this, new EventArgs());
            }
            SoundOnLength = new TimeSpan(DateTime.Now.Ticks) - SoundOnLength;
        }

        private void OnSoundOffEventRaised()
        {

        }

        private void OnDrawingEventRaised()
        {
            if (this.Drawing != null)
            {
                bool[] displayCopy = new bool[this.Display.Length];
                this.Display.CopyTo(displayCopy, 0);
                this.Drawing(this, new Chip8DrawingEventArgs(displayCopy));
            }
        }
    }
}
