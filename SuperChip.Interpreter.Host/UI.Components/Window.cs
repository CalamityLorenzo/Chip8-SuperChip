using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Point = Microsoft.Xna.Framework.Point;
using Color = Microsoft.Xna.Framework.Color;
using System.Diagnostics;

namespace SuperChip.Interpreter.Host.UI
{
    public class WindowContainer
    {
        private readonly SpriteBatch spriteBatch;
        private readonly Vector2 currentPosition;

        public Vector2 TitleSize { get; }

        private readonly Vector2 currentDimensions;
        private readonly string title;
        private readonly SpriteFont font;
        private readonly int padding;
        private readonly int border;
        private readonly bool displayTitle;
        private readonly Texture2D backgroundTexture;
        private readonly Texture2D titleTexture;
        private Color titleColour;

        public WindowContainer(SpriteBatch spriteBatch, Vector2 startPosition, Vector2 dimensions, string title, SpriteFont font, Color background, Color titleColourBackground, Color titleColour, int padding, int border, bool displayTitle)
        {
            this.spriteBatch = spriteBatch;
            this.currentPosition = startPosition;
            this.currentDimensions = dimensions;
            this.title = title;
            this.font = font;
            this.padding = padding;
            this.border = border;
            this.displayTitle = displayTitle;
            this.backgroundTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            backgroundTexture.SetData<Color>(Enumerable.Range(0, 1).Select(a => background).ToArray());
            this.titleTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            titleTexture.SetData<Color>(Enumerable.Range(0, 1).Select(a => titleColourBackground).ToArray());
            this.titleColour = titleColour;
            if (this.displayTitle)
            {
                this.TitleSize = font.MeasureString(title);
                this.currentDimensions = new Vector2(currentDimensions.X, currentDimensions.Y + TitleSize.Y);
            }

        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(GameTime gameTime)
        {
            // Draw the whole area.
            var totalArea = new Rectangle(this.currentPosition.ToPoint(), this.currentDimensions.ToPoint());
            spriteBatch.Draw(backgroundTexture, totalArea, totalArea, Color.White);
            // Title Area if required
            if(this.displayTitle){
                // Container
                var titleArea = new Rectangle(this.currentPosition.ToPoint(), new Point((int)this.currentDimensions.X, (int)this.TitleSize.Y));
                spriteBatch.Draw(titleTexture, titleArea, titleArea, Color.White );
                // Text
                spriteBatch.DrawString(this.font, this.title, this.currentPosition, titleColour);
            }
        }
    }
}