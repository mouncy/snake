using BorderlessForm;
using SnakeGame.Native;

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SnakeGame {

	public class GameForm : FormBase, IGameContext {

		private readonly Color borderColor = Color.FromArgb(0, 75, 0);
		private readonly Color primaryColor = Color.FromArgb(83, 138, 52);
		private readonly Color secondaryColor = Color.FromArgb(74, 117, 44);

		private Canvas canvas;

		private PictureBox appleBox;
		private PictureBox crownBox;

		private Label scoreLbl;
		private Label bestScoreLbl;

		private FormWindowState oldWindowState;

		public FontFamily PrimaryFontFamily { get; private set; }
		public FontFamily SecondaryFontFamily { get; private set; }

		private BufferedGraphics bufferedGraphics;
		private Graphics graphics;

		public Game Game { get; private set; }
		private bool initialized;

		public GameForm() : base() {
			Icon = Properties.Resources.SnakeIcon;
			Text = "Snake";

			TitleBar.Height = 50;
			TitleBar.BackColor = secondaryColor;
			TitleBar.MaxLblForeColor = Color.White;
			TitleBar.MinLblForeColor = Color.White;
			TitleBar.CloseLblForeColor = Color.White;

			TitleBar.CloseLblHoverForeColor = Color.White;
			TitleBar.CloseLblBackColor = Color.DarkGreen;

			TitleBar.MaxLblHoverForeColor = Color.White;
			TitleBar.MaxLblBackColor = Color.DarkGreen;

			TitleBar.MinLblHoverForeColor = Color.White;
			TitleBar.MinLblBackColor = Color.DarkGreen;

			ShowBorder = true;
			BorderColor = borderColor;

			ApplyFonts();
			CreateGame();
			HookListeners();
		}

		protected override void OnShown(EventArgs e) {
			base.OnShown(e);
			CreateControls();
			AllocateGraphics();

			Game.InitGame();
			Game.StartGame(true);
			initialized = true;
		}

		private void ApplyFonts() {
			PrivateFontCollection collection = new PrivateFontCollection();
			AddFont(collection, Properties.Resources.OpenSans_Semibold);
			AddFont(collection, Properties.Resources.OpenSans_Regular);

			PrimaryFontFamily = collection.Families[0];
			SecondaryFontFamily = collection.Families[1];
		}

		private void AddFont(PrivateFontCollection collection, byte[] data) {
			int length = data.Length;

			IntPtr hFont = Marshal.AllocHGlobal(length);
			Marshal.Copy(data, 0, hFont, length);

			uint pcFonts = 0;
			NativeMethods.AddFontMemResourceEx(hFont, (uint) length, IntPtr.Zero, ref pcFonts);
			collection.AddMemoryFont(hFont, length);

			Marshal.FreeHGlobal(hFont);
		}

		private void CreateGame() {
			Game = new Game(this) {
				StartText = new string[] { Properties.Strings.StartText_01, Properties.Strings.StartText_02 },
				ResumeText = new string[] { Properties.Strings.ResumeText },
				RestartText = new string[] { Properties.Strings.RestartText_01, Properties.Strings.RestartText_02 },

				PrimaryFontFamily = PrimaryFontFamily,
				SecondaryFontFamily = SecondaryFontFamily
			};

			Game.ScoreChanged += (game, args) => {
				scoreLbl.Text = game.Score.ToString();
			};
			Game.BestScoreChanged += (game, args) => {
				if (!bestScoreLbl.Visible) {
					bestScoreLbl.Visible = true;
					crownBox.Visible = true;
				}
				bestScoreLbl.Text = game.Score.ToString();
			};
		}

		private void CreateControls() {
			Size = new Size(557, 543);
			MinimumSize = Size;

			Content = new Panel {
				Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
				Size = new Size(Width - 1, Height - 1),
				Location = new Point(1, 1),
				BackColor = primaryColor,
				Parent = this
			};

			canvas = new Canvas {
				Location = new Point(5, TitleBar.Bottom + 5),
				Size = new Size(544, 480),
				Parent = Content
			};

			appleBox = new PictureBox {
				SizeMode = PictureBoxSizeMode.StretchImage,
				Image = Properties.Resources.Apple,
				Location = new Point(10, 9),
				Size = new Size(32, 32),
				Parent = TitleBar
			};
			TitleBar.AddTitleBarListeners(appleBox);

			scoreLbl = new Label {
				Font = new Font(SecondaryFontFamily, 14, FontStyle.Regular),
				TextAlign = ContentAlignment.BottomLeft,
				Location = new Point(appleBox.Right + 5, 9),
				Size = new Size(60, 32),
				ForeColor = Color.White,
				AutoSize = false,
				Parent = TitleBar,
				Text = "0"
			};
			TitleBar.AddTitleBarListeners(scoreLbl);

			crownBox = new PictureBox {
				SizeMode = PictureBoxSizeMode.StretchImage,
				Image = Properties.Resources.Crown,
				Location = new Point(scoreLbl.Right + 5, 9),
				Size = new Size(32, 32),
				Parent = TitleBar,
				Visible = false
			};
			TitleBar.AddTitleBarListeners(crownBox);

			bestScoreLbl = new Label {
				Font = new Font(SecondaryFontFamily, 14, FontStyle.Regular),
				TextAlign = ContentAlignment.BottomLeft,
				Location = new Point(crownBox.Right + 5, 9),
				Size = new Size(60, 32),
				ForeColor = Color.White,
				AutoSize = false,
				Parent = TitleBar,
				Visible = false
			};
			TitleBar.AddTitleBarListeners(bestScoreLbl);

			CultureInfo info = new CultureInfo(NativeMethods.GetUserDefaultLCID());
			System.Threading.Thread.CurrentThread.CurrentUICulture = info;
		}

		private void HookListeners() {
			Resize += HandleResize;
			KeyDown += HandleKeyDown;

			Deactivate += (s, e) => Game?.StopGame(true);
		}

		private void AllocateGraphics() {
			BufferedGraphicsContext context = BufferedGraphicsManager.Current;
			bufferedGraphics = context.Allocate(canvas.CreateGraphics(), canvas.ClientRectangle);
			graphics = bufferedGraphics.Graphics;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;

			canvas.PaintAction = graphics => {
				bufferedGraphics.Render(graphics);

				GC.Collect();
				GC.WaitForPendingFinalizers();
			};
		}

		private void HandleResize(object sender, EventArgs e) {
			if (!initialized) return;

			int width = ClientRectangle.Width;
			int height = ClientRectangle.Height - TitleBar.Height + 1;

			int widthOffset = width % (int) Game.GameObjectSize;
			int heightOffset = height % (int) Game.GameObjectSize;

			canvas.Top = (heightOffset / 2) + TitleBar.Height;
			canvas.Left = widthOffset / 2;

			canvas.Width = width - widthOffset;
			canvas.Height = height - heightOffset;

			AllocateGraphics();
			Game.Resize(canvas.Width, canvas.Height);
		}

		private void HandleKeyDown(object sender, KeyEventArgs e) {
			switch (e.KeyCode) {
				case Keys.W:
				case Keys.Up:
					Game.SetDirection(GameDirection.Up);
					break;
				case Keys.S:
				case Keys.Down:
					Game.SetDirection(GameDirection.Down);
					break;
				case Keys.A:
				case Keys.Left:
					Game.SetDirection(GameDirection.Left);
					break;
				case Keys.D:
				case Keys.Right:
					Game.SetDirection(GameDirection.Right);
					break;
				case Keys.Space:
					if (Game.Dead)
						Game.RestartGame();
					else
						Game.ResumeGame();
					break;
				case Keys.F11:
				case Keys.Escape when Game.FullScreen:
					ToggleFullScreen();
					break;
			}
		}

		private void ToggleFullScreen() {
			if (Game.FullScreen) {
				WindowState = oldWindowState;
				TitleBar.Height = 50;
				TopMost = false;
			} else {
				oldWindowState = WindowState;
				WindowState = FormWindowState.Maximized;
				TitleBar.Height = 0;
				TopMost = true;
			}
			Game.FullScreen = !Game.FullScreen;
			HandleResize(this, EventArgs.Empty);
		}

		public Graphics GetGraphics() {
			return graphics;
		}

		public void Redraw() {
			canvas.Invalidate();
		}

		public int GetHeight() {
			return canvas.Height;
		}

		public int GetWidth() {
			return canvas.Width;
		}

		public Rectangle GetClientArea() {
			return canvas.ClientRectangle;
		}
	}
}
