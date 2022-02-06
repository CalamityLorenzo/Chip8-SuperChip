using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Point = Microsoft.Xna.Framework.Point;
using Color = Microsoft.Xna.Framework.Color;
using System.Diagnostics;

namespace SuperChip.Interpreter.Host.UI
{
    public class FileSystemEntry
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public string FsType { get; set; }
        public Vector2 Size { get; set; }
        public bool Enabled { get; set; } =true;
    }

    public class FileSelectedEventArgs : EventArgs
    {
        public FileSelectedEventArgs(string fileName)
        {
            this.Filename = fileName;
        }

        public string Filename { get; }
    }

    public class FilePicker
    {
        private readonly SpriteBatch spriteBatch;
        private string currentDirectory;

        public SpriteFont Font { get; }

        private Texture2D texture;

        public Point MousePos { get; private set; }
        public Rectangle DisplayRect { get; private set; }

        public FileSystemEntry[] Folder;
        public bool IsActivated {get;set;}

        public event EventHandler<FileSelectedEventArgs> OnFileSelected;

        public FilePicker(SpriteBatch spriteBatch, SpriteFont font, string directory)
        {
            this.currentDirectory = directory;
            this.spriteBatch = spriteBatch;
            this.Font = font;

            this.texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            // texture.SetData<Color>(Enumerable.Range(0, 1).Select(a => Color.DarkBlue).ToArray());
            this.SetFolderInfo();
            this.IsActivated = true;
        }

        private void FileSelected(string fullPath){
            if(this.OnFileSelected!=null)
                this.OnFileSelected(this, new FileSelectedEventArgs(fullPath));
        }

        private void SetFolderInfo()
        {
            DirectoryInfo di = new DirectoryInfo(this.currentDirectory);
            var files = di.GetFiles().ToList();
            var folders = di.GetDirectories().ToList();

            Folder = new FileSystemEntry[files.Count + folders.Count + 1];

            Folder[0] = new FileSystemEntry
            {
                FsType = "Directory",
                FullPath = di.Parent?.FullName ?? null,
                Name = "..",
                Size = this.Font.MeasureString(".."),
                Enabled = !(di.Parent == null)
            };
            for (var x = 1; x < files.Count + 1; ++x)
            {
                Folder[x] = new FileSystemEntry
                {
                    FsType = "File",
                    FullPath = files[x - 1].FullName,
                    Name = files[x - 1].Name,
                    Size = this.Font.MeasureString(files[x - 1].Name)
                };
            }

            var currentFolder = 0;
            for (var x = 1 + files.Count; x < folders.Count + files.Count + 1; ++x)
            {
                Folder[x] = new FileSystemEntry
                {
                    FsType = "Directory",
                    FullPath = folders[currentFolder].FullName,
                    Name = folders[currentFolder].Name,
                    Size = this.Font.MeasureString(folders[currentFolder].Name)
                };
                currentFolder += 1;
            }


            var maxi = Folder.Max(a => a.Size.X);
            this.DisplayRect = new Rectangle(0, 0, (int)maxi, (int)Folder.Sum(a => a.Size.Y));
        }
        public void Click(int X, int Y)
        {
            var MouseRect = new Rectangle(X, Y, 0, 0);
            if (this.DisplayRect.Intersects(MouseRect))
            {
                var yPos = 0;
                for (var x = 0; x < this.Folder.Length; ++x)
                {
                    var textPosition = new Vector2(0, yPos);
                    var textRect = new Rectangle(textPosition.ToPoint(), new Point(this.DisplayRect.Width, (int)this.Folder[x].Size.Y));
                    if (textRect.Intersects(MouseRect))
                    {
                        var folderItem = Folder[x];
                        // Debug.WriteLine(Folder[x].FullPath);
                        if(folderItem.FsType=="Directory"){
                            this.currentDirectory = folderItem.FullPath;
                            this.SetFolderInfo();
                        }
                    }
                    yPos += (int)Folder[x].Size.Y;
                }
            }

        }
        public void Update(GameTime gameTime)
        {
            MouseState ms = Mouse.GetState();
            this.MousePos = new Point
            {
                X = ms.X,
                Y = ms.Y
            };
        }

        public void Draw(GameTime gameTime)
        {
            this.spriteBatch.Draw(this.texture, this.DisplayRect, this.DisplayRect, Color.White);

            var yPos = 0;
            for (var x = 0; x < this.Folder.Length; ++x)
            {
                if (this.Folder[x].Enabled)
                {
                    var col = Color.White;
                    var textPosition = new Vector2(0, yPos);
                    var textRect = new Rectangle(textPosition.ToPoint(), new Point(this.DisplayRect.Width, (int)this.Folder[x].Size.Y));
                    if (textRect.Intersects(new Rectangle(MousePos, Point.Zero)))
                    {
                        col = Color.Yellow;

                        this.spriteBatch.Draw(this.texture, new Vector2(textPosition.X, textPosition.Y), textRect, col);
                    }

                    this.spriteBatch.DrawString(this.Font, Folder[x].Name, textPosition, col);
                    yPos += (int)this.Folder[x].Size.Y;

                }
            }
        }

    }
}