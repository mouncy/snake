using BorderlessForm.Native;

using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BorderlessForm {

	public class FormBase : Form {

		public FormWindowState MinMaxState {
			get {
				int s = NativeMethods.GetWindowLong(Handle, NativeConstants.GWL_STYLE);
				if ((s & (int) WindowStyle.WS_MAXIMIZE) > 0) return FormWindowState.Maximized;
				if ((s & (int) WindowStyle.WS_MINIMIZE) > 0) return FormWindowState.Minimized;
				return FormWindowState.Normal;
			}
		}

		[Category("WindowStyle")]
		public TitleBar TitleBar { get; private set; }

		public Control Content {
			get => content;
			set {
				if (content != value) {
					UnhookListener(content);
					HookListener(value);
					content = value;
					SetBorder(borderColor, showBorder);
				}
			}
		}

		private Control content;

		[Category("Appearance")]
		public Color BorderColor {
			get => borderColor;
			set {
				if (borderColor != value) {
					SetBorder(value, showBorder);
					borderColor = value;
				}
			}
		}

		[Category("WindowStyle")]
		public bool ShowBorder {
			get => showBorder;
			set {
				if (showBorder != value) {
					SetBorder(BorderColor, value);
					showBorder = value;
				}
			}
		}

		[Category("WindowStyle")]
		public DropShadow Shadow { get; private set; }

		private readonly Panel[] border = new Panel[8];
		private Color borderColor;
		private bool showBorder;

		private FormWindowState oldWindowState;

		public FormBase() {
			Content = this;
			KeyPreview = true;
			TitleBar = new TitleBar(this) {
				Height = 30
			};
			if (!DesignMode) {
				Shadow = new DropShadow(this) {
					ShadowBorder = 10,
					BorderRadius = 30,
					ShadowColor = Color.FromArgb(30, 0, 0, 0)
				};
			}
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated(e);

			if (!DesignMode)
				SetWindowRegion(Handle, 0, 0, Width, Height);
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			if (!DesignMode)
				Shadow.RefreshShadow();
		}

		protected override void OnShown(EventArgs e) {
			InitBorder();
			InitTitleBar();
			oldWindowState = MinMaxState;

			base.OnShown(e);
		}

		protected override void OnSizeChanged(EventArgs e) {
			base.OnSizeChanged(e);

			if (oldWindowState != MinMaxState) {
				if (MinMaxState == FormWindowState.Maximized) {
					SetBorder(BorderColor, false, false);
				} else if (oldWindowState == FormWindowState.Maximized) {
					SetBorder(BorderColor, showBorder, true);
				}
				OnResizeBegin(e);
				OnResizeEnd(e);

				oldWindowState = MinMaxState;
			}
		}

		protected override void WndProc(ref Message m) {
			if (DesignMode) {
				base.WndProc(ref m);
				return;
			}

			switch ((WindowMessage) m.Msg) {
				case WindowMessage.WM_NCCALCSIZE:
					WmNCCalcSize(ref m);
					break;
				case WindowMessage.WM_NCPAINT:
					WmNCPaint(ref m);
					break;
				case WindowMessage.WM_NCACTIVATE:
					WmNCActivate(ref m);
					break;
				case WindowMessage.WM_SETTEXT:
					WmSetText(ref m);
					break;
				case WindowMessage.WM_WINDOWPOSCHANGED:
					WmWindowPosChanged(ref m);
					break;
				case (WindowMessage) 174:
					// ignore magic number message
					break;
				default:
					base.WndProc(ref m);
					break;
			}
		}

		private void WmWindowPosChanged(ref Message m) {
			DefWndProc(ref m);
			UpdateBounds();

			WINDOWPOS pos = (WINDOWPOS) Marshal.PtrToStructure(m.LParam, typeof(WINDOWPOS));
			SetWindowRegion(m.HWnd, 0, 0, pos.cx, pos.cy);
			m.Result = NativeConstants.TRUE;
		}

		private void WmNCCalcSize(ref Message m) {
			RECT r = (RECT) Marshal.PtrToStructure(m.LParam, typeof(RECT));
			bool max = MinMaxState == FormWindowState.Maximized;

			if (max) {
				int x = NativeMethods.GetSystemMetrics(NativeConstants.SM_CXSIZEFRAME);
				int y = NativeMethods.GetSystemMetrics(NativeConstants.SM_CYSIZEFRAME);
				int p = NativeMethods.GetSystemMetrics(NativeConstants.SM_CXPADDEDBORDER);
				int w = x + p;
				int h = y + p;

				r.left += w;
				r.top += h;
				r.right -= w;
				r.bottom -= h;

				APPBARDATA appBarData = new APPBARDATA {
					cbSize = Marshal.SizeOf(typeof(APPBARDATA))
				};
				int autohide = NativeMethods.SHAppBarMessage(NativeConstants.ABM_GETSTATE, ref appBarData) & NativeConstants.ABS_AUTOHIDE;
				if (autohide != 0) r.bottom -= 1;

				Marshal.StructureToPtr(r, m.LParam, true);
			}
			m.Result = IntPtr.Zero;
		}

		private void WmNCPaint(ref Message m) {
			m.Result = NativeConstants.TRUE;
		}

		private void WmSetText(ref Message m) {
			DefWndProc(ref m);
		}

		private void WmNCActivate(ref Message m) {
			if (MinMaxState == FormWindowState.Minimized)
				DefWndProc(ref m);
			else {
				m.Result = NativeConstants.TRUE;
			}
		}

		private void SetWindowRegion(IntPtr hwnd, int left, int top, int right, int bottom) {
			HandleRef hrgn = new HandleRef(this, NativeMethods.CreateRectRgn(0, 0, 0, 0));
			int r = NativeMethods.GetWindowRgn(hwnd, hrgn.Handle);

			NativeMethods.GetRgnBox(hrgn.Handle, out RECT box);
			if (box.left != left || box.top != top || box.right != right || box.bottom != bottom) {
				HandleRef hr = new HandleRef(this, NativeMethods.CreateRectRgn(left, top, right, bottom));
				NativeMethods.SetWindowRgn(hwnd, hr.Handle, NativeMethods.IsWindowVisible(hwnd));
			}
		}

		private void ContentMouseDown(object sender, MouseEventArgs e) {
			if (TitleBar == null || (!TitleBar.ExtendedTitleBar && PointToClient(MousePosition).Y <= TitleBar.ClientRectangle.Height)) {
				DecorationMouseDown(e, HitTest.Caption);
			}
		}

		protected internal void DecorationMouseDown(MouseEventArgs e, HitTest hit) {
			if (e.Button == MouseButtons.Left) {
				switch (e.Clicks) {
					case 1:
						DecorationMouseDown(hit, MousePosition);
						break;
					case 2:
						WindowState = (FormWindowState) Math.Abs((int) WindowState - 2);
						break;
				}
			}
		}

		protected internal void DecorationMouseDown(HitTest hit, Point p) {
			POINTS pt = new POINTS { X = (short) p.X, Y = (short) p.Y };

			NativeMethods.ReleaseCapture();
			NativeMethods.SendMessage(Handle, (int) WindowMessage.WM_NCLBUTTONDOWN, (int) hit, pt);
		}

		private void ContentMouseUp(object sender, MouseEventArgs e) {
			if (TitleBar == null || (!TitleBar.ExtendedTitleBar && PointToClient(MousePosition).Y <= TitleBar.ClientRectangle.Height)) {
				DecorationMouseUp(e, HitTest.Caption);
			}
		}

		protected internal void DecorationMouseUp(MouseEventArgs e, HitTest hit) {
			switch (e.Button) {
				case MouseButtons.Left:
					DecorationMouseUp(hit, MousePosition);
					break;
				case MouseButtons.Right:
					if (TitleBar == null || PointToClient(MousePosition).Y <= TitleBar.ClientRectangle.Height)
						ShowSystemMenu();
					break;
			}
		}

		protected internal void DecorationMouseUp(HitTest hit, Point p) {
			POINTS pt = new POINTS { X = (short) p.X, Y = (short) p.Y };

			NativeMethods.ReleaseCapture();
			NativeMethods.SendMessage(Handle, (int) WindowMessage.WM_NCLBUTTONUP, (int) hit, pt);
		}

		protected void ShowSystemMenu() {
			ShowSystemMenu(MousePosition);
		}

		protected void ShowSystemMenu(Point pos) {
			int lParam = MakeLong((short) pos.X, (short) pos.Y);
			NativeMethods.SendMessage(Handle, (int) WindowMessage.WM_SYSMENU, 0, lParam);
		}

		private void SetBorder(Color color, bool borderVisible = true, bool? visible = null) {
			if (border[0] == null) return;

			foreach (Panel p in border) {
				p.BackColor = color;
				p.Visible = borderVisible;
			}

			if (visible != null) {
				if (visible.Value) {
					if (TitleBar != null && !TitleBar.IsDisposed) {
						TitleBar.Top++;
						TitleBar.Left++;
						TitleBar.Size -= new Size(2, 0);
					}
					if (Content != null && Content != this && !Content.IsDisposed) {
						Content.Top++;
						Content.Left++;
						Content.Size -= new Size(2, 2);
					}
				} else {
					if (TitleBar != null && !TitleBar.IsDisposed) {
						TitleBar.Top--;
						TitleBar.Left--;
						TitleBar.Size += new Size(2, 0);
					}
					if (Content != null && Content != this && !Content.IsDisposed) {
						Content.Top--;
						Content.Left--;
						Content.Size += new Size(2, 2);
					}
				}
			}
		}

		private void HookListener(Control content) {
			if (content == null) return;

			content.MouseDown += ContentMouseDown;
			content.MouseUp += ContentMouseUp;
		}

		private void UnhookListener(Control content) {
			if (content == null) return;

			content.MouseDown -= ContentMouseDown;
			content.MouseUp -= ContentMouseUp;
		}

		private void InitTitleBar() {
			TitleBar.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
			TitleBar.Size = new Size(ClientRectangle.Width, TitleBar.Height);
			TitleBar.Location = new Point(0, 0);
			TitleBar.Margin = Padding.Empty;
			TitleBar.Parent = this;

			TitleBar.ApplyChanges();
		}

		private void InitBorder() {
			// top-left border
			border[0] = new Panel {
				Anchor = AnchorStyles.Left | AnchorStyles.Top,
				Cursor = Cursors.SizeNWSE,
				Location = new Point(0, 0),
				Margin = Padding.Empty,
				Size = new Size(1, 1),
				Parent = this
			};
			border[0].MouseDown += (s, e) => DecorationMouseDown(e, HitTest.TopLeft);

			// top-right border
			border[1] = new Panel {
				Anchor = AnchorStyles.Right | AnchorStyles.Top,
				Cursor = Cursors.SizeNESW,
				Location = new Point(ClientRectangle.Width - 1, 0),
				Margin = Padding.Empty,
				Size = new Size(1, 1),
				Parent = this
			};
			border[1].MouseDown += (s, e) => DecorationMouseDown(e, HitTest.TopRight);

			// bottom-left border
			border[2] = new Panel {
				Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
				Cursor = Cursors.SizeNESW,
				Location = new Point(0, ClientRectangle.Height - 1),
				Margin = Padding.Empty,
				Size = new Size(1, 1),
				Parent = this
			};
			border[2].MouseDown += (s, e) => DecorationMouseDown(e, HitTest.BottomLeft);

			// bottom-right border
			border[3] = new Panel {
				Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
				Cursor = Cursors.SizeNWSE,
				Location = new Point(ClientRectangle.Width - 1, ClientRectangle.Height - 1),
				Margin = Padding.Empty,
				Size = new Size(1, 1),
				Parent = this
			};
			border[3].MouseDown += (s, e) => DecorationMouseDown(e, HitTest.BottomRight);

			// top border
			border[4] = new Panel {
				Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
				Cursor = Cursors.SizeNS,
				Location = new Point(1, 0),
				Margin = Padding.Empty,
				Size = new Size(ClientRectangle.Width - 1, 1),
				Parent = this
			};
			border[4].MouseDown += (s, e) => DecorationMouseDown(e, HitTest.Top);

			// left border
			border[5] = new Panel {
				Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom,
				Cursor = Cursors.SizeWE,
				Location = new Point(0, 1),
				Margin = Padding.Empty,
				Size = new Size(1, ClientRectangle.Height - 1),
				Parent = this
			};
			border[5].MouseDown += (s, e) => DecorationMouseDown(e, HitTest.Left);

			// right border
			border[6] = new Panel {
				Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
				Cursor = Cursors.SizeWE,
				Location = new Point(ClientRectangle.Width - 1, 1),
				Margin = Padding.Empty,
				Size = new Size(1, ClientRectangle.Height - 2),
				Parent = this
			};
			border[6].MouseDown += (s, e) => DecorationMouseDown(e, HitTest.Right);

			// bottom border
			border[7] = new Panel {
				Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
				Cursor = Cursors.SizeNS,
				Location = new Point(1, ClientRectangle.Height - 1),
				Margin = Padding.Empty,
				Size = new Size(ClientRectangle.Width - 2, 1),
				Parent = this
			};
			border[7].MouseDown += (s, e) => DecorationMouseDown(e, HitTest.Bottom);

			SetBorder(borderColor, showBorder);
		}

		protected static int MakeLong(short lowPart, short highPart) {
			return (int) (((ushort) lowPart) | (uint) (highPart << 16));
		}
	}
}
