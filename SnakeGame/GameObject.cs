using System.Drawing;

namespace SnakeGame {

	public class GameObject {

		public float X { get; set; }
		public float Y { get; set; }

		public float Width { get; set; }
		public float Height { get; set; }

		public bool IsEmpty => Width <= 0 || Height <= 0;

		public GameObject() {

		}

		public GameObject(float x, float y) {
			X = x;
			Y = y;
		}

		public GameObject(float x, float y, float size) : this(x, y, size, size) {
			
		}

		public GameObject(float x, float y, float w, float h) : this(x, y) {
			Width = w;
			Height = h;
		}

		public GameObject Extend(float value) {
			return Extend(value, value);
		}

		public GameObject Extend(float xw, float yh) {
			return new GameObject(X - xw, Y - yh, Width + xw, Height + yh);
		}

		public GameObject Reduce(float value) {
			return Reduce(value, value);
		}

		public GameObject Reduce(float xw, float yh) {
			return new GameObject(X + xw, Y + yh, Width - xw, Height - yh);
		}

		public Point ToPoint() {
			return Point.Round(ToPointF());
		}

		public PointF ToPointF() {
			return new PointF(X, Y);
		}

		public Rectangle ToRectangle() {
			return Rectangle.Round(ToRectangleF());
		}

		public RectangleF ToRectangleF() {
			return new RectangleF(X, Y, Width, Height);
		}

		public static GameObject FromPoint(Point p) {
			return new GameObject(p.X, p.Y);
		}

		public static GameObject FromPointF(PointF p) {
			return new GameObject(p.X, p.Y);
		}

		public static GameObject FromRectangle(Rectangle rect) {
			return new GameObject(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static GameObject FromRectangleF(RectangleF rect) {
			return new GameObject(rect.X, rect.Y, rect.Width, rect.Height);
		}
	}
}
