using System;
using System.Runtime.InteropServices;

namespace SnakeGame.Native {

	public static class NativeMethods {

		[DllImport("gdi32.dll")]
		public static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);

		[DllImport("kernel32.dll")]
		public static extern int GetUserDefaultLCID();

	}
}
