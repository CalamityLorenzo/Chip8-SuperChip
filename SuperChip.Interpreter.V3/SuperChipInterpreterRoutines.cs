using System.Diagnostics;

namespace SuperChip11Interpreter.V3
{

    public class Chip8DrawingEventArgs : EventArgs
    {
        public bool[] Display;
        public Chip8DrawingEventArgs(bool[] display)
        {
            Display = display;
        }
    }

    public partial class SuperChipInterpreter
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

            for (var x = 0; x < 8; ++x)
            {
                this.RPLFlags.Add((byte)x, 0);
            }

            // I regiser
            IndexRegister = 0;


            // BuIld out the 4x5 font.
            // This can be stored anywhere in the first 512 bytes.

            Array.Copy(new byte[] { 0xF0, 0x90, 0x90, 0x90, 0xF0, }, 0, Memory, 0, 5); // 0
            Array.Copy(new byte[] { 0x20, 0x60, 0x20, 0x20, 0x70 }, 0, Memory, 5, 5); //1
            Array.Copy(new byte[] { 0xF0, 0x10, 0xF0, 0x80, 0xF0 }, 0, Memory, 10, 5); //2
            Array.Copy(new byte[] { 0xF0, 0x10, 0xF0, 0x10, 0xF0 }, 0, Memory, 15, 5); //3
            Array.Copy(new byte[] { 0x90, 0x90, 0xF0, 0x10, 0x10 }, 0, Memory, 20, 5);
            Array.Copy(new byte[] { 0xF0, 0x80, 0xF0, 0x10, 0xF0 }, 0, Memory, 25, 5);
            Array.Copy(new byte[] { 0xF0, 0x80, 0xF0, 0x90, 0xF0 }, 0, Memory, 30, 5);
            Array.Copy(new byte[] { 0xF0, 0x10, 0x20, 0x40, 0x40 }, 0, Memory, 35, 5);
            Array.Copy(new byte[] { 0xF0, 0x90, 0xF0, 0x90, 0xF0 }, 0, Memory, 40, 5);
            Array.Copy(new byte[] { 0xF0, 0x90, 0xF0, 0x10, 0xF0 }, 0, Memory, 45, 5);
            Array.Copy(new byte[] { 0xF0, 0x90, 0xF0, 0x90, 0x90 }, 0, Memory, 50, 5);
            Array.Copy(new byte[] { 0xE0, 0x90, 0xE0, 0x90, 0xE0 }, 0, Memory, 55, 5);
            Array.Copy(new byte[] { 0xF0, 0x80, 0x80, 0x80, 0xF0 }, 0, Memory, 60, 5);
            Array.Copy(new byte[] { 0xE0, 0x90, 0x90, 0x90, 0xE0 }, 0, Memory, 65, 5);
            Array.Copy(new byte[] { 0xF0, 0x80, 0xF0, 0x80, 0xF0 }, 0, Memory, 70, 5);
            Array.Copy(new byte[] { 0xF0, 0x80, 0xF0, 0x80, 0x80 }, 0, Memory, 75, 5);


            // 8*10 font
            Array.Copy(new byte[] { 0x3C, 0x7E, 0xC3, 0xC3, 0xC3, 0xC3, 0xC3, 0xC3, 0x7E, 0x3C }, 0, Memory, 80, 10); //0
            Array.Copy(new byte[] { 0x3C, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x58, 0x38, 0x18 }, 0, Memory, 90, 10); //1
            Array.Copy(new byte[] { 0xFF, 0xFF, 0x60, 0x30, 0x18, 0x0C, 0x06, 0xC3, 0x7F, 0x3E }, 0, Memory, 100, 10); //2
            Array.Copy(new byte[] { 0x3C, 0x7E, 0xC3, 0x03, 0x0E, 0x0E, 0x03, 0xC3, 0x7E, 0x3C }, 0, Memory, 110, 10); //3
            Array.Copy(new byte[] { 0x06, 0x06, 0xFF, 0xFF, 0xC6, 0x66, 0x36, 0x1E, 0x0E, 0x06 }, 0, Memory, 120, 10); //4
            Array.Copy(new byte[] { 0x3C, 0x7E, 0xC3, 0x03, 0xFE, 0xFC, 0xC0, 0xC0, 0xFF, 0xFF }, 0, Memory, 130, 10); //5
            Array.Copy(new byte[] { 0x3C, 0x7E, 0xC3, 0xC3, 0xFE, 0xFC, 0xC0, 0xC0, 0x7C, 0x3E }, 0, Memory, 140, 10); //6
            Array.Copy(new byte[] { 0x60, 0x60, 0x60, 0x30, 0x18, 0x0C, 0x06, 0x03, 0xFF, 0xFF }, 0, Memory, 150, 10); //7
            Array.Copy(new byte[] { 0x3C, 0x7E, 0xC3, 0xC3, 0x7E, 0x7E, 0xC3, 0xC3, 0x7E, 0x3C }, 0, Memory, 160, 10); //8
            Array.Copy(new byte[] { 0x7C, 0x3E, 0x03, 0x03, 0x3F, 0x7F, 0xC3, 0xC3, 0x7E, 0x3C }, 0, Memory, 170, 10); //9

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
        public void DrawSuperChip(byte xPos, byte yPos, byte spriteHeight)
        {

            if (!this.SuperChipEnabled) throw new Exception("SuperChip not enabled.");
            int screenWidth = 128;
            int screenHeight = 64;

            // the display is 128*64. When not in high res, we scale up by a factor of 2.
            // So each pixel is 2*2. makes some calculations a little odd.
            int increment = this.HighResolutionMode ? 1 : 2;

            int spriteWidth = spriteHeight < 16 ? 8 : 16;
            // the x and y can easily be over rated, we need to moduo them back,
            var x = (xPos * increment) % screenWidth;
            var y = (yPos * increment) % screenHeight;
            this.Registers[0xF] = 0;

            // pixel to draw
            for (var spriteRow = 0; spriteRow < spriteHeight; ++spriteRow)
            {
                var rowData = this.Memory[this.IndexRegister + spriteRow];
                // iterate over the display memory in rows
                for (var spritePixel = 0; spritePixel < spriteWidth; ++spritePixel)
                {
                    var currentXPos = x + (spritePixel * increment);
                    var currentYPos = y + (spriteRow * increment);
                    if (currentXPos < screenWidth && currentYPos < screenHeight)
                    {
                        var idx = currentXPos + (currentYPos * 128);
                        var screenPixel = this.Display[idx];
                        // Test to see if this particular bit is set.
                        var rowPixel = (rowData & (1 << (spriteWidth-1) - spritePixel)) != 0;
                        // are we updating an already set bit.
                        if (rowPixel & screenPixel)
                        {
                            this.Registers[0xf] = 1;
                            this.Display[idx] = false;
                        }
                        else
                            this.Display[idx] = screenPixel ^ rowPixel;

                        var currentBit = this.Display[idx];
                        // Non-hires
                        if (!this.HighResolutionMode && currentXPos + 1 < screenWidth)
                        {
                            this.Display[idx + 1] = currentBit;
                        }
                        else{
                            Debug.WriteLine($"X: out of range {currentXPos}");
                        }
                        if (!this.HighResolutionMode && currentYPos + 1 < screenWidth)
                        {
                            this.Display[idx + 128] = currentBit;
                            if (currentXPos + 1 < screenWidth)
                                this.Display[idx + 129] = currentBit;

                        }
                        else{
                            Debug.WriteLine($"Y: out of range {currentYPos}");
                        }
                    }
                }

            }
            this.OnDrawingEventRaised();

        }

        public void DrawChip8(byte xPos, byte yPos, byte spriteHeight)
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
                for (var spritePixel = 0; spritePixel < 8; ++spritePixel)
                {
                    if (x > screenWidth - 1) break;
                    if (x + spritePixel < screenWidth && y + spriteRow < screenHeight)
                    {
                        var idx = (x + spritePixel) + ((y + spriteRow) * 64);
                        var screenPixel = this.Display[idx];
                        var rowPixel = (rowData & (1 << 7 - spritePixel)) != 0;
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
