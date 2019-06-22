using System;

namespace BorderlessForm.Native {

	public static class NativeConstants {
		public const int SM_CXSIZEFRAME = 32;
		public const int SM_CYSIZEFRAME = 33;
		public const int SM_CXPADDEDBORDER = 92;

		public const int GWL_ID = -12;
		public const int GWL_STYLE = -16;
		public const int GWL_EXSTYLE = -20;

		public const int WM_NCLBUTTONDOWN = 0x00A1;
		public const int WM_NCRBUTTONUP = 0x00A5;

		public const uint TPM_LEFTBUTTON = 0x0000;
		public const uint TPM_RIGHTBUTTON = 0x0002;
		public const uint TPM_RETURNCMD = 0x0100;

		public static readonly IntPtr TRUE = (IntPtr) 1;
		public static readonly IntPtr FALSE = (IntPtr) 0;

		public const uint ABM_GETSTATE = 0x4;
		public const int ABS_AUTOHIDE = 0x1;

		public const int ULW_COLORKEY = 0x00000001;
		public const int ULW_ALPHA = 0x00000002;
		public const int ULW_OPAQUE = 0x00000004;

		public const byte AC_SRC_OVER = 0x00;
		public const byte AC_SRC_ALPHA = 0x01;
	}
}
