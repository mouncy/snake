﻿using System.Runtime.InteropServices;

namespace BorderlessForm.Native {

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct BLENDFUNCTION {
		public byte BlendOp;
		public byte BlendFlags;
		public byte SourceConstantAlpha;
		public byte AlphaFormat;
	}
}
