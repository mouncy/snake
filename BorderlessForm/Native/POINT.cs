using System.Runtime.InteropServices;

namespace BorderlessForm.Native {

	[StructLayout(LayoutKind.Sequential)]
	public struct POINT {
		public int X;
		public int Y;

		public POINT(int x, int y) {
			X = x;
			Y = y;
		}
	}
}
