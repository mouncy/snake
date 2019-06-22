using System;
using System.Collections.Generic;
using System.Drawing;

using static SnakeGame.Util.ImageUtil;

namespace SnakeGame {

	public class Snake {

		public List<GameObject> Segments { get; private set; }
		public GameDirection Direction { get; set; }

		private Image[,] textures;
		private const int TEXTURE_SIZE = 64;

		public event SnakeEventHandler SnakeMove;
		public event SnakeEventHandler SnakeDraw;

		public Snake() {
			Segments = new List<GameObject>();
			LoadTexture();
		}

		public bool ContainsRectangle(Rectangle rect) {
			if (rect == null || rect.IsEmpty) return false;

			foreach (GameObject seg in Segments) {
				if (rect.IntersectsWith(seg.ToRectangle()))
					return true;
			}
			return false;
		}

		public bool ContainsRectangle(RectangleF rect) {
			return ContainsRectangle(Rectangle.Ceiling(rect));
		}

		public bool ContainsObject(GameObject obj) {
			if (obj == null || obj.IsEmpty) return false;
			return ContainsRectangle(obj.ToRectangle());
		}

		public void Draw(Graphics graphics, float opacity = 1f, bool sendEvent = true) {
			if (graphics == null || Segments.Count < 2) return;

			for (int i = 0; i < Segments.Count; i++) {
				GameObject seg = Segments[i];

				int textureX = 0;
				int textureY = 0;

				if (i == 0) {
					GameObject nseg = Segments[i + 1];

					if (seg.Y < nseg.Y) { // Up
						textureX = 3;
						textureY = 0;
					} else if (seg.Y > nseg.Y) { // Down
						textureX = 4;
						textureY = 1;
					} else if (seg.X > nseg.X) { // Right
						textureX = 4;
						textureY = 0;
					} else if (seg.X < nseg.X) { // Left
						textureX = 3;
						textureY = 1;
					}
				} else if (i == Segments.Count - 1) {
					GameObject pseg = Segments[i - 1];

					if (pseg.Y < seg.Y) { // Up
						textureX = 3;
						textureY = 2;
					} else if (pseg.Y > seg.Y) { // Down
						textureX = 4;
						textureY = 3;
					} else if (pseg.X > seg.X) { // Right
						textureX = 4;
						textureY = 2;
					} else if (pseg.X < seg.X) { // Left
						textureX = 3;
						textureY = 3;
					}
				} else {
					GameObject nseg = Segments[i + 1];
					GameObject pseg = Segments[i - 1];

					if (pseg.X < seg.X && nseg.X > seg.X || nseg.X < seg.X && pseg.X > seg.X) { // Horizontal Left-Right
						textureX = 1;
						textureY = 0;
					} else if (pseg.Y < seg.Y && nseg.Y > seg.Y || nseg.Y < seg.Y && pseg.Y > seg.Y) { // Vertical Up-Down
						textureX = 2;
						textureY = 1;
					} else if (pseg.X < seg.X && nseg.Y > seg.Y || nseg.X < seg.X && pseg.Y > seg.Y) { // Angle Left-Down
						textureX = 2;
						textureY = 0;
					} else if (pseg.Y < seg.Y && nseg.X < seg.X || nseg.Y < seg.Y && pseg.X < seg.X) { // Angle Top-Left
						textureX = 2;
						textureY = 2;
					} else if (pseg.X > seg.X && nseg.Y < seg.Y || nseg.X > seg.X && pseg.Y < seg.Y) { // Angle Right-Up
						textureX = 0;
						textureY = 1;
					} else if (pseg.Y > seg.Y && nseg.X > seg.X || nseg.Y > seg.Y && pseg.X > seg.X) { // Angle Down-Right
						textureX = 0;
						textureY = 0;
					}
				}
				Image texture = textures[textureX, textureY];
				graphics.DrawImage(texture.SetImageOpacity(opacity), seg.ToRectangle());
			}
			if (sendEvent)
				SnakeDraw?.Invoke(this, new SnakeEventArgs(Segments, Direction));
		}

		private GameObject GetNextSegment() {
			GameObject head = Segments[0];
			float x, y;

			switch (Direction) {
				case GameDirection.Up:
					x = head.X;
					y = head.Y - head.Height;
					break;
				case GameDirection.Down:
					x = head.X;
					y = head.Y + head.Height;
					break;
				case GameDirection.Left:
					x = head.X - head.Width;
					y = head.Y;
					break;
				default:
					x = head.X + head.Width;
					y = head.Y;
					break;
			}
			return new GameObject(x, y, head.Width, head.Height);
		}

		public void Init(GameObject head, int segments = 1, GameDirection direction = GameDirection.Right) {
			Direction = direction;

			Segments.Clear();
			Segments.Add(head);

			GameObject pseg = head;
			for (int i = 0; i < segments; i++) {
				float x, y;

				switch (direction) {
					case GameDirection.Up:
						x = pseg.X;
						y = pseg.Y + pseg.Height;
						break;
					case GameDirection.Down:
						x = pseg.X;
						y = pseg.Y - pseg.Height;
						break;
					case GameDirection.Left:
						x = pseg.X + pseg.Width;
						y = pseg.Y;
						break;
					default:
						x = pseg.X - pseg.Width;
						y = pseg.Y;
						break;
				}
				GameObject nseg = new GameObject(x, y, pseg.Width, pseg.Height);
				Segments.Add(nseg);
				pseg = nseg;
			}
		}

		private void LoadTexture() {
			Image texture = Properties.Resources.SnakeTexture;
			textures = new Image[texture.Width / TEXTURE_SIZE, texture.Height / TEXTURE_SIZE];

			for (int x = 0; x * TEXTURE_SIZE < texture.Width; x++) {
				for (int y = 0; y * TEXTURE_SIZE < texture.Height; y++) {
					textures[x, y] = new Bitmap(TEXTURE_SIZE, TEXTURE_SIZE);
					Graphics graphics = Graphics.FromImage(textures[x, y]);

					Rectangle srcRect = new Rectangle(x * TEXTURE_SIZE, y * TEXTURE_SIZE, TEXTURE_SIZE, TEXTURE_SIZE);
					Rectangle dstRect = new Rectangle(0, 0, TEXTURE_SIZE, TEXTURE_SIZE);
					graphics.DrawImage(texture, dstRect, srcRect, GraphicsUnit.Pixel);
				}
			}
		}

		public void Move(bool addSegment = false, bool sendEvent = true) {
			if (Direction == GameDirection.Stopped || Segments.Count < 2) return;

			GameObject tail = Segments[Segments.Count - 1];
			GameObject nseg = GetNextSegment();

			float tailX = tail.X;
			float tailY = tail.Y;

			for (int i = Segments.Count - 1; i > 0; i--) {
				Segments[i].X = Segments[i - 1].X;
				Segments[i].Y = Segments[i - 1].Y;
			}

			if (addSegment)
				Segments.Add(new GameObject(tailX, tailY, tail.Width, tail.Height));

			Segments[0].X = nseg.X;
			Segments[0].Y = nseg.Y;

			if (sendEvent)
				SnakeMove?.Invoke(this, new SnakeEventArgs(Segments, Direction));
		}

		public void Translate(float xOffset, float yOffset) {
			for (int i = 0; i < Segments.Count; i++) {
				Segments[i].X += xOffset;
				Segments[i].Y += yOffset;
			}
		}
	}

	public delegate void SnakeEventHandler(Snake sender, SnakeEventArgs e);

	public class SnakeEventArgs : EventArgs {

		public List<GameObject> Segments { get; }
		public GameDirection Direction { get; }

		public SnakeEventArgs(List<GameObject> segments, GameDirection direction) {
			Segments = new List<GameObject>(segments);
			Direction = direction;
		}
	}
}
