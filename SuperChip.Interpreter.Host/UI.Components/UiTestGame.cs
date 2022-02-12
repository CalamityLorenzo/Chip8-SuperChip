namespace SuperChip.Interpreter.UI
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using SuperChip.Interpreter.Host;
    using SuperChip.Interpreter.Host.UI;

    public class UITest : Game{
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public ScrollablePanel Panel { get; private set; }

        private SpriteFont sprMono;
        private SpriteFont sprArial;

        public SuperChipSettings SuperChipSettings { get; }
        public string MouseCoords { get; private set; }

        public UITest(SuperChipSettings settings){
            this.graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "./Content";
            this.SuperChipSettings = settings;
        }


        override protected void Initialize(){
            this.IsMouseVisible = true;
            this.graphics.IsFullScreen = false;
            this.graphics.PreferredBackBufferWidth = 800;
            this.graphics.PreferredBackBufferHeight = 600;
            this.graphics.ApplyChanges();

            this.sprMono = Content.Load<SpriteFont>("bin/Windows/consolas");
            this.sprArial = Content.Load<SpriteFont>("bin/Windows/Arial");

            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.Panel = new ScrollablePanel(this.spriteBatch, true, new Rectangle(100, 50, 300, 300));
        }

        private void UpdateMouseCoords(MouseState state){
            this.MouseCoords=$"X : {state.X} Y : {state.Y}";
        }

        override protected void Update(GameTime gameTime){
            var mState = Mouse.GetState();
            UpdateMouseCoords(mState);

            Panel.Update(gameTime);
        }

        override protected void Draw(GameTime gameTime){
            this.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.DarkGoldenrod);
            this.spriteBatch.Begin();

            Panel.Draw(gameTime);
            this.spriteBatch.DrawString(this.sprArial, MouseCoords, new Vector2(this.graphics.PreferredBackBufferWidth-200, 0), Color.Yellow);
            this.spriteBatch.End();
        }
    }
}