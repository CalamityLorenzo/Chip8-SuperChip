using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;

namespace SuperChip.Interpreter.Host.UI
{



    public interface IScrollablePanelContent : IDrawable
    {
        void SetSpriteBatch(SpriteBatch spritebatch);
        Rectangle ContentDimensions();
        void DrawPanel(GameTime time);
    }

    public enum ScrollDirection
    {
        Unknown = 0,
        Up,
        Down,
        Left,
        Right
    }
    public class ScrollablePanel
    {
        private readonly SpriteBatch sb;
        // This is our scissor rect
        public Rectangle DisplayArea { get; private set; } = Rectangle.Empty;
        // The total area of the content to be displayed.
        // Used to calculate the scollbar button size.
        private Rectangle ContentDimensions = Rectangle.Empty;
        public Rectangle vScrollbarDimensions { get; private set; } = Rectangle.Empty;
        private Rectangle vScrollbarButtonDimensons = Rectangle.Empty;

        public bool VerticalScrollbar { get; }
        public IScrollablePanelContent Content { get; private set; }
        public Rectangle TotalArea { get; private set; }

        private RasterizerState rasterState;
        private Texture2D scrollbarTexture;
        private Texture2D scrollbarNubTexture;

        public ScrollablePanel(SpriteBatch sb, bool verticalScrollbar, Rectangle startingDimensions)
        {

            this.sb = new SpriteBatch(sb.GraphicsDevice);
            this.VerticalScrollbar = verticalScrollbar;

            // This is the panel content area
            this.UpdateDisplayArea(startingDimensions);

            // Scrollbar and butotn
            this.scrollbarTexture = new Texture2D(this.sb.GraphicsDevice, 1, 1);
            this.scrollbarNubTexture = new Texture2D(this.sb.GraphicsDevice, 1, 1);
            scrollbarTexture.SetData<Color>((Enumerable.Range(0, 1).Select(a => Color.Gray).ToArray()));
            scrollbarNubTexture.SetData<Color>((Enumerable.Range(0, 1).Select(a => Color.DarkGray).ToArray()));

            // Rasterizer State (Do we actually need this?)
            this.rasterState = new RasterizerState();
            rasterState.MultiSampleAntiAlias = true;
            rasterState.ScissorTestEnable = true;
            rasterState.FillMode = FillMode.Solid;
            rasterState.CullMode = CullMode.CullCounterClockwiseFace;
            rasterState.DepthBias = 0;
            rasterState.SlopeScaleDepthBias = 0;

            UpdateTotalArea();
        }

        // The display area combined with the vertical scrollbar.
        private void UpdateTotalArea()
        {
            this.TotalArea = new Rectangle(DisplayArea.X, DisplayArea.Y, DisplayArea.Width + vScrollbarDimensions.X, DisplayArea.Height);
        }

        public void UpdateDisplayArea(Rectangle dimensions)
        {
            if (this.DisplayArea != dimensions)
            {
                this.DisplayArea = dimensions;
                FormatScrollbar();
                this.UpdateTotalArea();
            }
        }

        public void AddContent(IScrollablePanelContent content)
        {
            this.Content = content;
            this.Content.SetSpriteBatch(this.sb);
            // The full size of the content
            this.ContentDimensions = this.Content.ContentDimensions();
            FormatScrollbar();
        }
        // Set the size of the scrollbar button in relation to the content being displayed
        private void FormatScrollbar()
        {
            if (this.VerticalScrollbar)
            {
                float factor = (float)ContentDimensions.Height >= DisplayArea.Height ? (float)ContentDimensions.Height / DisplayArea.Height : 1;
                this.vScrollbarDimensions = new(DisplayArea.X + DisplayArea.Width - 20, DisplayArea.Y, 22, DisplayArea.Height);
                this.vScrollbarButtonDimensons = new(DisplayArea.X + DisplayArea.Width - 20, DisplayArea.Y, 22, 22);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (this.Content != null)
            {
                Rectangle contentDimensions = this.Content.ContentDimensions();
                if (this.ContentDimensions != contentDimensions)
                {
                    // well do something!
                    this.ContentDimensions = contentDimensions;
                    FormatScrollbar();
                }
                //if (contentDimensions != this.DisplayArea) UpdateDimensions(contentDimensions);
            }
        }

        public void Draw(GameTime gameTime)
        {
            this.sb.Begin(SpriteSortMode.Deferred, null, null, null, this.rasterState, null, null);
            this.sb.GraphicsDevice.ScissorRectangle = this.DisplayArea;
            // Draw content here...Maybe?
            // this.sb.Draw(this.scrollbarTexture, this.ContentArea, null, Color.Aqua);
            if (this.Content != null && Content.Visible)
            {
                Content.DrawPanel(gameTime);
                // Draw scrollbar here
                if (this.VerticalScrollbar)
                {

                    // this.sb.Draw(this.scrollbarTexture, new Vector2(this.DisplayArea.X + this.DisplayArea.Width - 20, this.DisplayArea.Y), new Rectangle(0, 0, 20, (int)this.DisplayArea.Height), Color.White);
                    this.sb.Draw(this.scrollbarTexture, new Vector2(vScrollbarDimensions.X, vScrollbarDimensions.Y), new Rectangle(0, 0, 20, (int)this.DisplayArea.Height), Color.White);
                    this.sb.Draw(this.scrollbarNubTexture, new Vector2(vScrollbarButtonDimensons.X, vScrollbarButtonDimensons.Y), new Rectangle(0, 0, vScrollbarButtonDimensons.Width, vScrollbarButtonDimensons.Height), Color.White);
                }
            }
            this.sb.End();
        }

        internal ScrollDirection UpdateVerticalScrollPosition(int x, int y)
        {
            // X and Y have beenc lieced.
            // do we move the button up or down?

            // Stepping distnace
            var factor = (float)this.ContentDimensions.Height / this.DisplayArea.Height;
            var distance = (float)this.DisplayArea.Height / factor;
            // Clicked after the halfway point of  the button, so button down and scroll up
            if (y > this.vScrollbarButtonDimensons.Y + this.vScrollbarButtonDimensons.Height / 2)
            {
                if (this.vScrollbarButtonDimensons.Y + 22 == this.vScrollbarDimensions.Height) return ScrollDirection.Unknown;

                if (this.vScrollbarButtonDimensons.Y + 22 < this.vScrollbarDimensions.Height)
                {
                    this.vScrollbarButtonDimensons.Y += (int)distance;
                    if (this.vScrollbarButtonDimensons.Y + 22 > this.vScrollbarDimensions.Height)
                        this.vScrollbarButtonDimensons.Y = this.vScrollbarDimensions.Height - 22;
                    return ScrollDirection.Up;
                }
            }
            else
            {
                if (y < this.vScrollbarButtonDimensons.Y)
                {
                    var newPos = (this.vScrollbarButtonDimensons.Y - (int)distance);
                    if (newPos < 0) newPos = 0;
                    this.vScrollbarButtonDimensons.Y = newPos;
                    return ScrollDirection.Down;
                }
            }
            return ScrollDirection.Unknown;
        }
    }
}