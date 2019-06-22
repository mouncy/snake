using System;
using System.Runtime.InteropServices;

namespace BorderlessForm.Native {

	[StructLayout(LayoutKind.Sequential)]
	public struct APPBARDATA {
		public int cbSize;
		public IntPtr hWnd;
		public uint uCallbackMessage;
		public uint uEdge;
		public RECT rc;
		public int lParam;
	}
}
