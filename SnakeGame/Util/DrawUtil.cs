using System;
using System.Drawing;

namespace SnakeGame.Util {

	public static class DrawUtil {

		public static void DrawGrid(IGameContext context, Color primaryColor, Color secondaryColor, float cellSize) {
			if (context == null) return;

			Graphics graphics = context.GetGraphics();
			graphics.Clear(primaryColor);

			if (primaryColor == secondaryColor) return;
			SolidBrush brush = new SolidBrush(secondaryColor);

			for (float i = 0, j; i * cellSize < context.GetWidth(); i++) {
				j = i % 2;

				while (j * cellSize < context.GetHeight()) {
					graphics.FillRectangle(brush, i * cellSize, j * cellSize, cellSize, cellSize);
					j += 2;
				}
			}
		}

		public static void DrawText(IGameContext context, string[] lines, Font font, Brush brush,
			float[] xOffsets = null, float[] yOffsets = null, bool tryCenter = true) {

			if (context == null || lines == null || lines.Length == 0 || font == null) return;
			Graphics graphics = context.GetGraphics();
			Rectangle bounds = context.GetClientArea();

			if (xOffsets == null) xOffsets = new float[0];
			if (yOffsets == null) yOffsets = new float[0];
			if (brush == null) brush = Brushes.Black;

			int center = (int) Math.Ceiling(lines.Length / 2f);
			float lineHeight = graphics.MeasureString(lines[0], font).Height;

			for (int i = 0; i < lines.Length; i++) {
				SizeF size = graphics.MeasureString(lines[i], font);
				float x = (bounds.Width - size.Width) / 2f;
				float y = (bounds.Height - size.Height) / 2f;

				PointF p = new PointF(x + 5, y + (lineHeight * i));
				if (tryCenter && lines.Length > 1)
					p.Y -= lineHeight * center;

				p.X += (i < xOffsets.Length ? xOffsets[i] : 0);
				p.Y += (i < yOffsets.Length ? yOffsets[i] : 0);

				graphics.DrawString(lines[i], font, brush, p);
			}
		}
	}
}
