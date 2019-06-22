using System.Drawing;
using System.Runtime.InteropServices;

namespace BorderlessForm.Native {

	[StructLayout(LayoutKind.Sequential)]
	public struct RECT {
		public int left;
		public int top;
		public int right;
		public int bottom;

		public RECT(int left, int top, int right, int bottom) {
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}

		public Rectangle ToRectangle() {
			return new Rectangle(left, top, right - left + 1, bottom - top + 1);
		}

		public static RECT FromXYWH(int x, int y, int width, int height) {
			return new RECT(x, y, x + width, y + height);
		}
	}
}
