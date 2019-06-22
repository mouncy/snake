using System.Runtime.InteropServices;

namespace BorderlessForm.Native {

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ARGB {
		public readonly byte Blue;
		public readonly byte Green;
		public readonly byte Red;
		public readonly byte Alpha;
	}
}
