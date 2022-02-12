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
        public bool Enabled { get; set; } = true;
    }

    public class FileSelectedEventArgs : EventArgs
    {
        public FileSelectedEventArgs(string fileName)
        {
            this.Filename = fileName;
        }

        public string Filename { get; }
    }

    public class FilePicker : IScrollablePanelContent
    {

        public SpriteFont Font { get; }
        private SpriteBatch spriteBatch;
        private Texture2D texture;
        public Point MousePos { get; private set; }


        public Rectangle DisplayRect { get; private set; }
        public Vector2 Offset {get; private set;} = Vector2.Zero;

        public int DrawOrder => throw new NotImplementedException();

        public bool Visible { get; set; } = true;
        public ScrollablePanel displayPanel { get; }

        private string currentDirectory;
        public FileSystemEntry[] FolderEntries;

        public event EventHandler<FileSelectedEventArgs> OnFileSelected;
        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;

        public FilePicker(SpriteBatch spriteBatch, SpriteFont font, Rectangle startingDimensions, string directory)
        {
            this.currentDirectory = directory;
            this.spriteBatch = spriteBatch;
            this.Font = font;

            this.texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            texture.SetData<Color>(Enumerable.Range(0, 1).Select(a => Color.DarkBlue).ToArray());
            this.displayPanel = new ScrollablePanel(spriteBatch, true, startingDimensions);
            this.SetFolderInfo();
            this.displayPanel.AddContent(this);
            //this.DisplayRect= new Rectangle(40,40, displayPanel.DisplayArea.Width, displayPanel.DisplayArea.Height);
        }

        private void UpdateDisplayRect()
        {
            var maxi = FolderEntries.Max(a => a.Size.X);
            if (maxi < this.DisplayRect.Width) maxi = this.DisplayRect.Width;
            // Total size of the displayable area.
            this.DisplayRect = new Rectangle(this.displayPanel.DisplayArea.X, this.displayPanel.DisplayArea.Y, (int)maxi, (int)FolderEntries.Sum(a => a.Size.Y));

            // Reset the offSet
             this.Offset = Vector2.Zero;
        }

        private void FileSelected(string fullPath)
        {
            if (this.OnFileSelected != null)
                this.OnFileSelected(this, new FileSelectedEventArgs(fullPath));
        }

        private void SetFolderInfo()
        {
            var drives = DriveInfo.GetDrives().ToList();

            DirectoryInfo di = new DirectoryInfo(this.currentDirectory);

            List<FileInfo> files;
            try
            {
                files = di.GetFiles().Where(a => (a.Attributes & FileAttributes.System) == 0).ToList();
            }
            catch (System.UnauthorizedAccessException ex)
            {
                return;
            }

            var folders = di.GetDirectories().Where(a => ((a.Attributes & FileAttributes.System & FileAttributes.Hidden)) == 0).ToList();

            FolderEntries = new FileSystemEntry[files.Count + folders.Count + 1 + drives.Count];
            FolderEntries[0] = new FileSystemEntry
            {
                FsType = "Directory",
                FullPath = di.Parent?.FullName ?? null,
                Name = ".. (Parent)",
                Size = this.Font.MeasureString(".. (Parent)"),
                Enabled = !(di.Parent == null)
            };
            for (var x = 0; x < drives.Count; ++x)
            {
                FolderEntries[x + 1] = new FileSystemEntry
                {
                    FsType = "Drive",
                    FullPath = drives[x].RootDirectory.FullName,
                    Name = drives[x].Name,
                    Size = this.Font.MeasureString(drives[x].Name),
                    Enabled = true,
                };
            }

            for (var x = 0; x < folders.Count; ++x)
            {
                FolderEntries[x + 1 + drives.Count ] = new FileSystemEntry
                {
                    FsType = "Directory",
                    FullPath = folders[x].FullName,
                    Name = folders[x].Name,
                    Size = this.Font.MeasureString(folders[x].Name)
                };
            }

            for (var x = 0; x < files.Count; ++x)
            {
                FolderEntries[x + 1 + folders.Count + drives.Count] = new FileSystemEntry
                {
                    FsType = "File",
                    FullPath = files[x].FullName,
                    Name = files[x].Name,
                    Size = this.Font.MeasureString(files[x].Name)
                };
            }


            //this.displayPanel.UpdateDimensions(DisplayRect);
            this.UpdateDisplayRect();
        }

        public void Click(int X, int Y)
        {
            var MouseRect = new Rectangle(X, Y, 0, 0);
            var yPos = this.DisplayRect.Y + this.Offset.Y;
            if (this.displayPanel.DisplayArea.Intersects(MouseRect))
            {

                if (displayPanel.vScrollbarDimensions.Intersects(MouseRect))
                {
                    // This method is effectively paging.
                    // It mooves 1 unit in the Up/Down direction.
                    // 1 unit of content being displayed on screen.
                    var scrollDirection =  displayPanel.UpdateVerticalScrollPosition(X, Y);
                    if(scrollDirection!=ScrollDirection.Unknown){
                        // Change the offSetRect Vector
                        
                        if(scrollDirection==ScrollDirection.Up){
                            this.Offset =new Vector2(this.Offset.X, this.Offset.Y- this.displayPanel.DisplayArea.Height);
                        }
                        if(scrollDirection == ScrollDirection.Down)
                            this.Offset =new Vector2(this.Offset.X, this.Offset.Y+ this.displayPanel.DisplayArea.Height);
                    }

                    return;
                }

                for (var x = 0; x < this.FolderEntries.Length; ++x)
                {
                    var textPosition = new Vector2(this.DisplayRect.X, yPos);
                    var textRect = new Rectangle(textPosition.ToPoint(), new Point(this.DisplayRect.Width, (int)this.FolderEntries[x].Size.Y));

                    if (textRect.Intersects(MouseRect))
                    {
                        var folderItem = FolderEntries[x];

                        // Debug.WriteLine(Folder[x].FullPath);
                        if (folderItem.FsType != "File" && folderItem.Enabled)
                        {
                            this.currentDirectory = folderItem.FullPath;
                            this.SetFolderInfo();
                            break;
                        }

                        if (folderItem.FsType == "File" && folderItem.Enabled)
                        {
                            FileSelected(folderItem.FullPath);
                        }
                    }
                    yPos += (int)FolderEntries[x].Size.Y;
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
            this.displayPanel.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            this.displayPanel.Draw(gameTime);
        }
        // this is actually fired from inside t he Panel control.
        // Which handles the clipping correctly.
        public void DrawPanel(GameTime gameTime)
        {
            var VisibleRect = DisplayRect;
            // VisibleRect.Y += 200;
            this.spriteBatch.Draw(this.texture, this.DisplayRect, null, Color.DarkBlue);
            var yPos = this.Offset.Y;
            for (var x = 0; x < this.FolderEntries.Length; ++x)
            {
                var col = Color.White;
                var textPosition = new Vector2(0, yPos) + new Vector2(DisplayRect.X, DisplayRect.Y);
                var textRect = new Rectangle(textPosition.ToPoint(), new Point(this.DisplayRect.Width, (int)this.FolderEntries[x].Size.Y));
                if (textRect.Intersects(new Rectangle(MousePos, Point.Zero)))
                {
                    col = Color.Yellow;
                    this.spriteBatch.Draw(this.texture, textPosition, col);
                }
                if (!FolderEntries[x].Enabled) { col = Color.DimGray; }
                this.spriteBatch.DrawString(this.Font, FolderEntries[x].Name, textPosition, col);
                yPos += (int)this.FolderEntries[x].Size.Y;
            }

        }

        public void SetSpriteBatch(SpriteBatch spritebatch)
        {
            this.spriteBatch = spritebatch;
        }

        public Rectangle ContentDimensions()
        {
            return this.DisplayRect;
        }
    }
}