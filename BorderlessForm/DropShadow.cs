using BorderlessForm.Native;

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BorderlessForm {

	public class DropShadow : Form {

		protected override CreateParams CreateParams {
			get {
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= (int) WindowStyleEx.WS_EX_LAYERED;
				return cp;
			}
		}

		public int BorderRadius { get; set; }

		public Color ShadowColor { get; set; }
		public int ShadowBorder { get; set; }

		public int ShadowAlpha {
			get => shadowAlpha;
			set {
				if (shadowAlpha != value) {
					shadowAlpha = value;
					RefreshShadow();
				}
			}
		}
		private int shadowAlpha;

		internal DropShadow(FormBase owner) {
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
			UpdateStyles();

			Owner = owner;
			ShowInTaskbar = false;
			FormBorderStyle = FormBorderStyle.None;

			int s = NativeMethods.GetWindowLong(Handle, NativeConstants.GWL_EXSTYLE);
			NativeMethods.SetWindowLong(Handle, NativeConstants.GWL_EXSTYLE, s | (int) WindowStyleEx.WS_EX_TRANSPARENT);
			shadowAlpha = 255;

			AddListeners();
		}

		private void AddListeners() {
			Owner.LocationChanged += (s, e) => UpdateLocation();
			Owner.SizeChanged += (s, e) => UpdateSize();

			Owner.FormClosing += (s, e) => Close();
			Owner.VisibleChanged += (s, e) => {
				if (Owner != null)
					Visible = Owner.Visible;
			};
		}

		public void UpdateLocation() {
			Point pos = Owner.Location;
			pos.Offset(-ShadowBorder, -ShadowBorder);
			Location = pos;
		}

		public void UpdateSize() {
			Size = Owner.Size + new Size(ShadowBorder, ShadowBorder);
			RefreshShadow();
		}

		public void RefreshShadow() {
			if (ShadowAlpha == 0 || ShadowBorder == 0) return;
			Bitmap btm = DrawShadow();

			SetBitmap(btm, (byte) ShadowAlpha);
			UpdateLocation();

			IntPtr hrgn;
			Region ownerRegion;

			if (Owner.Region == null) {
				hrgn = NativeMethods.CreateRectRgn(0, 0, Owner.ClientRectangle.Width, Owner.ClientRectangle.Height);
				ownerRegion = Region.FromHrgn(hrgn);
			} else
				ownerRegion = Owner.Region.Clone();

			hrgn = NativeMethods.CreateRoundRectRgn(0, 0, Width + 1, Height + 1, BorderRadius, BorderRadius);
			Region region = Region.FromHrgn(hrgn);

			ownerRegion.Translate(ShadowBorder, ShadowBorder);
			region.Exclude(ownerRegion);
			Region = region;
		}

		private void SetBitmap(Bitmap bitmap, byte alpha) {
			IntPtr hdc = NativeMethods.GetDC(IntPtr.Zero);
			IntPtr hdcSrc = NativeMethods.CreateCompatibleDC(hdc);
			IntPtr hBitmap = IntPtr.Zero;
			IntPtr oldBitmap = IntPtr.Zero;

			try {
				hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
				oldBitmap = NativeMethods.SelectObject(hdcSrc, hBitmap);

				SIZE size = new SIZE(bitmap.Width, bitmap.Height);
				POINT pos = new POINT((short) Left, (short) Top);
				POINT posSrc = new POINT(0, 0);

				BLENDFUNCTION blend = new BLENDFUNCTION {
					AlphaFormat = NativeConstants.AC_SRC_ALPHA,
					SourceConstantAlpha = alpha,
					BlendOp = NativeConstants.AC_SRC_OVER,
					BlendFlags = 0
				};
				NativeMethods.UpdateLayeredWindow(Handle, hdc, ref pos, ref size, hdcSrc, ref posSrc, 0, ref blend, NativeConstants.ULW_ALPHA);

			} finally {
				NativeMethods.ReleaseDC(IntPtr.Zero, hdc);
				if (hBitmap != IntPtr.Zero) {
					NativeMethods.SelectObject(hdcSrc, oldBitmap);
					NativeMethods.DeleteObject(hBitmap);
				}
				NativeMethods.DeleteDC(hdcSrc);
			}
		}

		private Bitmap DrawShadow() {
			int fw = Owner.Width + ShadowBorder * 2;
			int fh = Owner.Height + ShadowBorder * 2;
			int sr = ShadowBorder;

			Rectangle top = new Rectangle(sr, 0, Width - sr - 1, sr);
			Rectangle left = new Rectangle(0, sr, sr, Height - sr - 1);
			Rectangle right = new Rectangle(Width - 1, sr, sr, Height - sr - 1);
			Rectangle bottom = new Rectangle(sr, Height - 1, Width - sr - 1, sr);

			Rectangle topLeft = new Rectangle(0, 0, sr, sr);
			Rectangle topRight = new Rectangle(Width - 1, 0, sr, sr);
			Rectangle bottomLeft = new Rectangle(0, Height - 1, sr, sr);
			Rectangle bottomRight = new Rectangle(Width - 1, Height - 1, sr, sr);

			Bitmap bitmap = new Bitmap(fw, fh);
			Color surroundColor = Color.FromArgb(1, 0, 0, 0);

			using (Graphics g = Graphics.FromImage(bitmap)) {
				using (GraphicsPath gp = new GraphicsPath()) {
					gp.AddRectangle(topLeft);

					using (PathGradientBrush pgb = new PathGradientBrush(gp)) {
						pgb.CenterPoint = new PointF(topLeft.Width, topLeft.Height);
						pgb.SurroundColors = new Color[] { surroundColor };
						pgb.CenterColor = ShadowColor;
						g.FillPath(pgb, gp);
					}
				}
				using (GraphicsPath gp = new GraphicsPath()) {
					gp.AddRectangle(topRight);

					using (PathGradientBrush pgb = new PathGradientBrush(gp)) {
						pgb.CenterPoint = new PointF(topRight.X, topRight.Height);
						pgb.SurroundColors = new Color[] { surroundColor };
						pgb.CenterColor = ShadowColor;
						g.FillPath(pgb, gp);
					}
				}
				using (GraphicsPath gp = new GraphicsPath()) {
					gp.AddRectangle(bottomLeft);

					using (PathGradientBrush pgb = new PathGradientBrush(gp)) {
						pgb.CenterPoint = new PointF(bottomLeft.Width, bottomLeft.Y);
						pgb.SurroundColors = new Color[] { surroundColor };
						pgb.CenterColor = ShadowColor;
						g.FillPath(pgb, gp);
					}
				}
				using (GraphicsPath gp = new GraphicsPath()) {
					gp.AddRectangle(bottomRight);

					using (PathGradientBrush pgb = new PathGradientBrush(gp)) {
						pgb.CenterPoint = new PointF(bottomRight.X, bottomRight.Y);
						pgb.SurroundColors = new Color[] { surroundColor };
						pgb.CenterColor = ShadowColor;
						g.FillPath(pgb, gp);
					}
				}

				using (LinearGradientBrush lgb = new LinearGradientBrush(top, surroundColor, ShadowColor, LinearGradientMode.Vertical)) {
					lgb.GammaCorrection = true;
					g.FillRectangle(lgb, top);
				}
				using (LinearGradientBrush lgb = new LinearGradientBrush(left, surroundColor, ShadowColor, LinearGradientMode.Horizontal)) {
					lgb.GammaCorrection = true;
					g.FillRectangle(lgb, left);
				}
				using (LinearGradientBrush lgb = new LinearGradientBrush(right, ShadowColor, surroundColor, LinearGradientMode.Horizontal)) {
					lgb.GammaCorrection = true;
					g.FillRectangle(lgb, right);
				}
				using (LinearGradientBrush lgb = new LinearGradientBrush(bottom, ShadowColor, surroundColor, LinearGradientMode.Vertical)) {
					lgb.GammaCorrection = true;
					g.FillRectangle(lgb, bottom);
				}
			}
			Width = fw;
			Height = fh;
			return bitmap;
		}
	}
}
