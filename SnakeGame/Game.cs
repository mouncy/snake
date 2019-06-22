using SnakeGame.Util;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using static SnakeGame.GameDirection;
using static SnakeGame.Util.DrawUtil;
using static SnakeGame.Util.ImageUtil;

namespace SnakeGame {

	public class Game : Timer {

		public string[] StartText { get; set; }
		public string[] ResumeText { get; set; }
		public string[] RestartText { get; set; }

		public FontFamily PrimaryFontFamily { get; set; }
		public FontFamily SecondaryFontFamily { get; set; }

		public Color PrimaryGridColor { get; set; }
		public Color SecondaryGridColor { get; set; }
		public float GameObjectSize { get; private set; }

		public IGameContext Context { get; }

		public List<GameObject> Apples { get; }
		public int AddAppleCount { get; set; }
		
		public Snake Snake { get; }
		public int SnakeStartSegments { get; set; }
		public GameDirection SnakeStartDirection { get; set; }
		public int SnakeSpeed { get => Interval; set => Interval = value; }

		public int Score { get; private set; }
		public int BestScore { get; private set; }

		public bool FullScreen { get; set; }
		public bool Dead { get; private set; }
		public bool Playing => Enabled;

		public bool AutoIncreaseSpeed { get; set; }

		private new bool Enabled { get => base.Enabled; set => base.Enabled = value; }

		private bool ignoreKeys;

		private Animator animator;
		private float appleAnimation;
		private Action additionalPaintAction;

		private SolidBrush brush;

		private GameDirection oldDirection;
		private bool addSegment;

		public event GameEventHandler SnakeDead;

		public event GameEventHandler GameStop;
		public event GameEventHandler GameStart;

		public event GameEventHandler ScoreChanged;
		public event GameEventHandler BestScoreChanged;

		public Game(IGameContext context) {
			Context = context ?? throw new ArgumentException(nameof(context));
			Apples = new List<GameObject>();
			Snake = new Snake();

			animator = new Animator();
			Snake.SnakeDraw += HandleSnakeMove;

			InternalInitGame();
		}

		private void InternalInitGame() {
			PrimaryFontFamily = SystemFonts.DefaultFont.FontFamily;
			SecondaryFontFamily = SystemFonts.DefaultFont.FontFamily;
			PrimaryGridColor = Color.FromArgb(162, 209, 73);
			SecondaryGridColor = Color.FromArgb(170, 215, 81);

			SnakeStartSegments = 5;
			SnakeStartDirection = Right;
			AutoIncreaseSpeed = true;

			AddAppleCount = 1;
			BestScore = -1;

			brush = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
		}

		public void InitGame(int gameObjectSize = 32) {
			if (gameObjectSize < 16 || gameObjectSize > 64)
				throw new ArgumentException(nameof(gameObjectSize));

			SnakeSpeed = 100;
			GameObjectSize = gameObjectSize;
			Score = 0;
			OnScoreChanged();

			GameObject head = new GameObject(160, 224, GameObjectSize);
			Snake.Init(head, SnakeStartSegments, SnakeStartDirection);
			oldDirection = SnakeStartDirection;
			Snake.Direction = Stopped;

			RemoveApples();
			AddApple(AddAppleCount);
		}

		public new void Start() {
			StartGame();
		}

		public void StartGame(bool firstTime = false) {
			if (Playing) return;

			Graphics graphics = Context.GetGraphics();
			void Draw(int alpha = 200, float opacity = 1f) {
				DrawGrid(Context, PrimaryGridColor, SecondaryGridColor, GameObjectSize);
				DrawApples(graphics, 100, opacity);
				Snake.Draw(graphics, opacity, false);
				DrawFullScreenScore(graphics);
			}

			if ((firstTime || Dead) && StartText != null) {
				Dead = false;
				float offset = (Context.GetHeight() / 2) - 25 * (StartText.Length - 1);
				float[] yOffsets = Enumerable.Repeat(offset, StartText.Length).ToArray();
				Font font = new Font(PrimaryFontFamily, 20, FontStyle.Regular);

				animator.DoAnimation(frame => {
					Draw(frame * 20, frame / 10f);
					DrawText(Context, StartText, font, CreateBrush(brush, frame * 20), yOffsets: yOffsets);
					Context.Redraw();
				}, () => {
					Enabled = true;
					OnStart();

					animator.CreateTimer(timer => {
						if (Snake.Direction != Stopped || !Playing) {
							timer.Enabled = false;

							animator.DoAnimation(frame => {
								additionalPaintAction = () => DrawText(Context, StartText, font,
									CreateBrush(brush, 200 - (frame * 20)), yOffsets: yOffsets);
							}, () => {
								additionalPaintAction = null;
							});
						} else {
							Draw();
							DrawText(Context, StartText, font, brush, yOffsets: yOffsets);
							Context.Redraw();
						}
					});
				});
			} else {
				Enabled = true;
				OnStart();

				animator.CreateTimer(timer => {
					if (Snake.Direction != Stopped || !Playing)
						timer.Enabled = false;
					else {
						Draw();
						Context.Redraw();
					}
				});
			}
		}

		public void RestartGame(int gameObjectSize = 32) {
			if (Playing || !Dead || ignoreKeys) return;

			ignoreKeys = true;
			InitGame(gameObjectSize);
			StartGame();
		}

		public void ResumeGame() {
			if (Playing || Dead || ignoreKeys) return;

			Graphics graphics = Context.GetGraphics();
			Font font = new Font(PrimaryFontFamily, 30, FontStyle.Regular);

			animator.DoAnimation(frame => {
				DrawGrid(Context, PrimaryGridColor, SecondaryGridColor, GameObjectSize);
				DrawApples(graphics, 100, frame / 10f);
				Snake.Draw(graphics, frame / 10f, false);
				DrawFullScreenScore(graphics);
				DrawText(Context, ResumeText, font, CreateBrush(brush, 200 - (frame * 20)));
				Context.Redraw();
			}, () => StartGame());
		}

		public new void Stop() {
			StopGame();
		}

		public void StopGame(bool paused = false) {
			if (!Playing || ignoreKeys) return;
			Enabled = false;

			if (paused) {
				Font font = new Font(PrimaryFontFamily, 30, FontStyle.Regular);
				Graphics graphics = Context.GetGraphics();
				Snake.Direction = Stopped;

				animator.Stop();
				DrawGrid(Context, PrimaryGridColor, SecondaryGridColor, GameObjectSize);
				DrawFullScreenScore(graphics);
				DrawText(Context, ResumeText, font, brush);
				Context.Redraw();
			}
			OnStop();
		}

		public void SetDirection(GameDirection direction) {
			if (!Playing || Dead || ignoreKeys) return;

			switch (direction) {
				case Up:
					if (oldDirection != Down)
						Snake.Direction = direction;
					break;
				case Down:
					if (oldDirection != Up)
						Snake.Direction = direction;
					break;
				case Left:
					if (oldDirection != Right)
						Snake.Direction = direction;
					break;
				case Right:
					if (oldDirection != Left)
						Snake.Direction = direction;
					break;
				case Stopped:
					Snake.Direction = direction;
					break;
				default:
					throw new ArgumentException(nameof(direction));
			}
		}

		public void Resize(float width, float height) {
			ignoreKeys = false;

			StopGame(true);
			RepositionApples();

			GameObject head = Snake.Segments[0];
			float x = head.X;
			float y = head.Y;

			if (x >= width)
				Snake.Translate(-(x - (width / 2)), 0);
			if (y >= height)
				Snake.Translate(0, -(y - (height / 2)));
			Snake.Translate(-(head.X % head.Width), -(head.Y % head.Height));

			Font font = new Font(PrimaryFontFamily, 30, FontStyle.Regular);

			animator.Stop();
			DrawGrid(Context, PrimaryGridColor, SecondaryGridColor, GameObjectSize);
			DrawFullScreenScore(Context.GetGraphics());
			DrawText(Context, ResumeText, font, brush);

			Context.Redraw();
		}

		protected override void OnTick(EventArgs e) {
			if (Snake.Direction == Stopped) return;

			Graphics graphics = Context.GetGraphics();
			DrawGrid(Context, PrimaryGridColor, SecondaryGridColor, GameObjectSize);
			DrawApples(graphics, Interval);

			Snake.Move(addSegment);
			if (addSegment) addSegment = false;
			Snake.Draw(graphics);

			DrawFullScreenScore(graphics);
			additionalPaintAction?.Invoke();

			Context.Redraw();
			oldDirection = Snake.Direction;
		}

		private void DrawFullScreenScore(Graphics graphics, int alpha = 200, float opacity = 1f) {
			if (!FullScreen) return;

			int y = 5;
			Font font = new Font(SecondaryFontFamily, 14, FontStyle.Regular);
			float lineHeight = graphics.MeasureString(Score.ToString(), font).Height;

			Image apple = Properties.Resources.Apple;
			Image crown = Properties.Resources.Crown;

			RectangleF appleSrcRect = new RectangleF(0, 0, apple.Width, apple.Height);
			RectangleF appleDstRect = new RectangleF(10, y, GameObjectSize, GameObjectSize);
			RectangleF scoreRect = new RectangleF(appleDstRect.X + appleDstRect.Width + 5, y + appleDstRect.Height - lineHeight, 60, appleDstRect.Height);

			RectangleF crownSrcRect = new RectangleF(0, 0, crown.Width, crown.Height);
			RectangleF crownDstRect = new RectangleF(scoreRect.X + scoreRect.Width + 5, y, GameObjectSize, GameObjectSize);
			RectangleF bestScoreRect = new RectangleF(crownDstRect.X + crownDstRect.Width + 5, y + crownDstRect.Height - lineHeight, 60, crownDstRect.Height);

			RectangleF bounds;
			if (BestScore == -1)
				bounds = new RectangleF(0, 0, scoreRect.X + scoreRect.Width + 5, GameObjectSize + y * 2);
			else
				bounds = new RectangleF(0, 0, bestScoreRect.X + bestScoreRect.Width + 5, GameObjectSize + y * 2);
			bool overlaps = Snake.ContainsRectangle(Rectangle.Ceiling(bounds)) && !Dead;

			apple = overlaps ? apple.SetImageOpacity(.3f) : apple.SetImageOpacity(opacity);
			Brush brushClone = overlaps ? CreateBrush(brush, 100) : CreateBrush(brush, alpha);

			graphics.DrawImage(apple, appleDstRect, appleSrcRect, GraphicsUnit.Pixel);
			graphics.DrawString(Score.ToString(), font, brushClone, scoreRect);

			if (BestScore != -1) {
				crown = overlaps ? crown.SetImageOpacity(.3f) : crown.SetImageOpacity(opacity);

				graphics.DrawImage(crown, crownDstRect, crownSrcRect, GraphicsUnit.Pixel);
				graphics.DrawString(BestScore.ToString(), font, brushClone, bestScoreRect);
				bounds.Width = bestScoreRect.X + bestScoreRect.Width + 5;
			}
		}

		private void DrawApples(Graphics graphics, int interval, float opacity = 1f) {
			float max = Math.Max(1000 / (interval * 2), 1);
			if (appleAnimation >= max)
				appleAnimation = -max;

			Image image = Properties.Resources.Apple;
			float value = Math.Abs(appleAnimation);
			value *= interval / 100f;

			Apples.ForEach(apple => {
				graphics.DrawImage(image.SetImageOpacity(opacity), apple.Extend(value).ToRectangle());
			});

			appleAnimation++;
		}

		public void AddApple(int count = 1) {
			Random r = new Random();
			int width = Context.GetWidth();
			int height = Context.GetHeight();

			for (int i = 0; i < count; i++) {
				GameObject apple = new GameObject();
				bool overlaps = true;

				while (overlaps) {
					apple = new GameObject(r.Next(width), r.Next(height), GameObjectSize);
					overlaps = Snake.ContainsObject(apple);

					if (!overlaps) {
						Rectangle rect = apple.ToRectangle();
						foreach (GameObject obj in Apples) {
							if (obj.ToRectangle().IntersectsWith(rect)) {
								overlaps = true;
								break;
							}
						}
					}
				}
				apple.X -= apple.X % apple.Width;
				apple.Y -= apple.Y % apple.Height;
				Apples.Add(apple);
			}
		}

		public void RemoveApples() {
			Apples.Clear();
			Context.Redraw();
		}

		private void RepositionApples() {
			int count = Apples.Count;
			Apples.RemoveAll(apple => apple.X >= Context.GetWidth() || apple.Y >= Context.GetHeight());

			count -= Apples.Count;
			if (count > 0)
				AddApple(count);
		}

		private void HandleSnakeMove(Snake s, SnakeEventArgs e) {
			GameObject head = e.Segments[0];

			int count = Apples.Count;
			Apples.RemoveAll(apple => head.ToRectangle().IntersectsWith(apple.ToRectangle()));

			if (count != Apples.Count)
				UpdateScore();
			if (Apples.Count == 0) {
				AddApple(AddAppleCount);

				if (AutoIncreaseSpeed)
					SnakeSpeed -= SnakeSpeed / 80;

			}
			bool dead = head.X < 0 || head.Y < 0 || head.X >= Context.GetWidth() || head.Y >= Context.GetHeight();
			if (!dead) {
				for (int i = 1; i < e.Segments.Count; i++) {
					GameObject seg = e.Segments[i];

					if (head.ToRectangle().IntersectsWith(seg.ToRectangle())) {
						dead = true;
						break;
					}
				}
			}
			if (dead)
				OnDead();
		}

		protected virtual void OnDead() {
			Dead = true;
			Enabled = false;
			ignoreKeys = true;

			Snake.Direction = Stopped;
			SnakeDead?.Invoke(this, EventArgs.Empty);

			if (Score >= BestScore) {
				BestScore = Score;
				OnBestScoreChanged();
			}

			Graphics graphics = Context.GetGraphics();
			Font font = new Font(PrimaryFontFamily, 30, FontStyle.Regular);

			animator.DoAnimation(frame => {
				DrawGrid(Context, PrimaryGridColor, SecondaryGridColor, GameObjectSize);
				DrawApples(graphics, 100, (9 - frame) / 10f);
				Snake.Draw(graphics, (9 - frame) / 10f, false);
				DrawFullScreenScore(graphics);
				DrawText(Context, RestartText, font, CreateBrush(brush, frame * 20));
				Context.Redraw();
			}, () => ignoreKeys = false);
		}

		protected virtual void OnStart() {
			GameStart?.Invoke(this, EventArgs.Empty);
			ignoreKeys = false;
		}

		protected virtual void OnStop() {
			GameStop?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnScoreChanged() {
			ScoreChanged?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnBestScoreChanged() {
			BestScoreChanged?.Invoke(this, EventArgs.Empty);
		}

		private void UpdateScore() {
			addSegment = true;

			Score++;
			OnScoreChanged();

			if (Score > BestScore && BestScore != -1) {
				BestScore = Score;
				OnBestScoreChanged();
			}
		}

		private SolidBrush CreateBrush(SolidBrush parent, int alpha) {
			Color color = parent.Color;
			return new SolidBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
		}
	}

	public delegate void GameEventHandler(Game sender, EventArgs e);
}
