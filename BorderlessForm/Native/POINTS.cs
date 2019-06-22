using System.Runtime.InteropServices;

namespace BorderlessForm.Native {

	[StructLayout(LayoutKind.Sequential)]
	public struct POINTS {
		public short X;
		public short Y;

		public POINTS(short x, short y) {
			X = x;
			Y = y;
		}
	}
}
