using System.Drawing;
using System.Drawing.Imaging;

namespace SnakeGame.Util {

	public static class ImageUtil {

		public static Image SetImageOpacity(this Image image, float opacity) {
			if (opacity < 0 || opacity > 1)
				return image;

			Bitmap bmp = new Bitmap(image.Width, image.Height);

			using (Graphics graphics = Graphics.FromImage(bmp)) {
				ColorMatrix matrix = new ColorMatrix {
					Matrix33 = opacity
				};
				ImageAttributes attributes = new ImageAttributes();
				attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

				Rectangle dstRect = new Rectangle(0, 0, bmp.Width, bmp.Height);
				graphics.DrawImage(image, dstRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
			}
			return bmp;
		}
	}
}
