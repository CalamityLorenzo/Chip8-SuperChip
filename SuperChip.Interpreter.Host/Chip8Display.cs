using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace SuperChip.Interpreter.Host
{

    internal class Chip8Display : IDrawable
    {
        private readonly int width;
        private readonly int height;
        private readonly Point position;
        private readonly int cellWidth;
        private readonly int cellHeight;
        private readonly SpriteBatch spriteBatch;
        private readonly Texture2D cell;

        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;

        public bool[] DisplayArray { get; private set; }
        public int BorderWidth { get; }
        public int Padding { get; }

        public int DrawOrder => throw new NotImplementedException();

        public bool Visible {get;set;}

        public Chip8Display(int width, int height, Point position, int cellWidth, int cellHeight, Color cellColor, SpriteBatch spriteBatch)
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

            this.BorderWidth = 3;
            this.Padding = 3;
        }

        public void Update(GameTime gameTime, bool[] display)
        {
            // oh yeah, taste that lack of bounds checking...
            this.DisplayArray = display;
        }

        public void Draw(GameTime gameTime)
        {
            Microsoft.Xna.Framework.Rectangle sourceRect = new(50, 70, this.cellWidth * width, this.cellHeight * height);

            //  Border first
            spriteBatch.Draw(this.cell, new Rectangle(this.position.X, this.position.Y, 3, height * cellHeight), Color.White);
            spriteBatch.Draw(this.cell, new Rectangle(this.position.X, this.position.Y, this.width * cellWidth, 3), Color.White);
            spriteBatch.Draw(this.cell, new Rectangle(this.position.X + (width * cellWidth), this.position.Y, 3, height * cellHeight), Color.White);
            spriteBatch.Draw(this.cell, new Rectangle(this.position.X, this.position.Y + (height * cellHeight), this.width * cellWidth, 3), Color.White);
            var positionVector = position.ToVector2();
            if (DisplayArray != null)
                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                    {
                        if (DisplayArray[x + (y * width)] == true)
                            spriteBatch.Draw(this.cell, Vector2.Add(positionVector, new Vector2(x * cellWidth + (BorderWidth + Padding), y * cellHeight + (BorderWidth + Padding))), Color.White);
                    }
        }


        public void Reset(){
            this.DisplayArray = new bool[width * height];
        }
    }

}