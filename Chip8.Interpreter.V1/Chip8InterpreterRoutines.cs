using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.Interpreter.V1
{
    public partial class Chip8Interpreter
    {
        public void ClearDisplay()
        {
            this.Display = new bool[this.Display.Length];
            OnDrawingEventRaised();
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
                    var screenPixel = this.Display[(x + i) * (y+spriteRow)];
                    var rowPixel = (rowData & (1 << 7 - i)) != 0;
                    if (rowPixel & screenPixel)
                    {
                        this.Registers[0xf] = 1;
                        this.Display[(x + i) + ((y+spriteRow) * 64)] = false;
                    }
                    if (!screenPixel && rowPixel == true)
                        this.Display[(x + i) + ((y + spriteRow) * 64)] = true;
                }
            }

            this.OnDrawingEventRaised();
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
