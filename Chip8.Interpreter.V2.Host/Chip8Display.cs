using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Chip8.Interpreter.V2.Host
{
    internal class Chip8Display
    {
        private readonly int width;
        private readonly int height;
        private readonly Vector2 position;
        private readonly int cellWidth;
        private readonly int cellHeight;
        private readonly SpriteBatch spriteBatch;
        private readonly Texture2D cell;
        public bool[] DisplayArray { get; private set; }

        public Chip8Display(int width, int height, Vector2 position, int cellWidth, int cellHeight, Color cellColor, SpriteBatch spriteBatch)
        {
            this.width = width;
            this.height = height;
            this.position = position;
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
            this.spriteBatch = spriteBatch;
            this.cell = new Texture2D(spriteBatch.GraphicsDevice, cellWidth, cellHeight);
            cell.SetData<Color>(Enumerable.Range(0, cellWidth * cellHeight).Select(a => cellColor).ToArray());
            this.DisplayArray = new bool[width * height];
        }

        public void Update(GameTime gameTime, bool[] display)
        {
            this.DisplayArray = display;
        }

        public void Draw(GameTime gameTime)
        {
            if(DisplayArray!=null)
            for (var y = 0; y < height; y ++)
                for (var x = 0; x < width; x ++)
                {
                    if (DisplayArray[x + (y*64)] == true)
                        spriteBatch.Draw(this.cell, Vector2.Add(position, new Vector2(x*cellWidth, y*cellHeight)), Color.White);
                }
        }

    }
}
