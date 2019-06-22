using System;
using System.Runtime.InteropServices;

namespace BorderlessForm.Native {

	[StructLayout(LayoutKind.Sequential)]
	public struct WINDOWPOS {
		internal IntPtr hwnd;
		internal IntPtr hWndInsertAfter;
		internal int x;
		internal int y;
		internal int cx;
		internal int cy;
		internal uint flags;
	}
}
