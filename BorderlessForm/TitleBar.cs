using BorderlessForm.Native;

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BorderlessForm {

	public class TitleBar : Panel {

		public Label CloseLbl { get; private set; }
		public Label MaxLbl { get; private set; }
		public Label MinLbl { get; private set; }

		public Color CloseLblForeColor { get; set; }
		public Color MaxLblForeColor { get; set; }
		public Color MinLblForeColor { get; set; }

		public Color CloseLblBackColor { get; set; }
		public Color MaxLblBackColor { get; set; }
		public Color MinLblBackColor { get; set; }

		public Color CloseLblHoverForeColor { get; set; }
		public Color MaxLblHoverForeColor { get; set; }
		public Color MinLblHoverForeColor { get; set; }

		public Color CloseLblDisabledColor { get; set; }
		public Color MaxLblDisabledColor { get; set; }
		public Color MinLblDisabledColor { get; set; }

		public bool HandleEvents { get; set; }

		public bool CloseLblEnabled {
			get => closeLblEnabled;
			set {
				bool oldValue = closeLblEnabled;
				closeLblEnabled = value;

				if (oldValue != closeLblEnabled)
					CloseLbl?.Invalidate();
			}
		}
		public bool MaxLblEnabled {
			get => maxLblEnabled;
			set {
				bool oldValue = maxLblEnabled;
				maxLblEnabled = value;

				if (oldValue != maxLblEnabled)
					MaxLbl?.Invalidate();
			}
		}
		public bool MinLblEnabled {
			get => minLblEnabled;
			set {
				bool oldValue = minLblEnabled;
				minLblEnabled = value;

				if (oldValue != minLblEnabled)
					MinLbl?.Invalidate();
			}
		}

		public bool ExtendedTitleBar {
			get => extended;
			set {
				if (extended != value) {
					extended = value;
					SetExtended(value);
				}
			}
		}
		private bool extended;

		public ShapeType BackgroundShape { get; set; }

		private bool closeLblEnabled;
		private bool maxLblEnabled;
		private bool minLblEnabled;

		private bool closeMouseHover;
		private bool maxMouseHover;
		private bool minMouseHover;

		private FormBase parent;
		private bool created;

		internal TitleBar(FormBase parent) : base() {
			this.parent = parent;
			InitComponent();
		}

		internal void InitComponent() {
			InitCloseLbl();
			InitMaximizeLbl();
			InitMinimizeLbl();

			InitTitleBar();
			MouseDown += TitleBarMouseDown;
			MouseUp += TitleBarMouseUp;

			parent.Resize += (s, e) => {
				CloseLbl?.Invalidate();
				MaxLbl?.Invalidate();
				MinLbl?.Invalidate();
			};
		}

		internal void ApplyChanges() {
			created = true;
			SetExtended(extended);

			CloseLbl?.Invalidate();
			MaxLbl?.Invalidate();
			MinLbl?.Invalidate();
		}

		private void InitTitleBar() {
			HandleEvents = true;
			MaxLblEnabled = true;
			MinLblEnabled = true;
			CloseLblEnabled = true;
			ExtendedTitleBar = true;

			BackColor = Color.White;

			CloseLblForeColor = Color.Black;
			CloseLblBackColor = Color.LightGray;
			CloseLblHoverForeColor = Color.Black;
			CloseLblDisabledColor = Color.Gray;

			MaxLblForeColor = Color.Black;
			MaxLblBackColor = Color.LightGray;
			MaxLblHoverForeColor = Color.Black;
			MaxLblDisabledColor = Color.Gray;

			MinLblForeColor = Color.Black;
			MinLblBackColor = Color.LightGray;
			MinLblHoverForeColor = Color.Black;
			MinLblDisabledColor = Color.Gray;
		}

		private void SetExtended(bool extended) {
			if (!created) return;

			if (extended) {
				Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
				Location = new Point(0, 0);
				Size = new Size(parent.ClientRectangle.Width, Height);
			} else {
				Anchor = AnchorStyles.Right | AnchorStyles.Top;
				Location = new Point(MinLbl.Left - 5, 0);
				Size = new Size(Width - MinLbl.Left + 5, Height);
			}
		}

		public void AddTitleBarListeners(Control control) {
			if (control == null || control.IsDisposed) return;

			control.MouseDown += TitleBarMouseDown;
			control.MouseUp += TitleBarMouseUp;
		}

		public void RemoveTitleBarListeners(Control control) {
			if (control == null || control.IsDisposed) return;

			control.MouseDown -= TitleBarMouseDown;
			control.MouseUp -= TitleBarMouseUp;
		}

		private void InitCloseLbl() {
			CloseLbl = new Label {
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Location = new Point(ClientRectangle.Width - 25, 5),
				Size = new Size(20, 20),
				Margin = Padding.Empty,
				AutoSize = false,
				Parent = this
			};
			CloseLbl.MouseEnter += (s, e) => {
				if (HandleEvents) {
					closeMouseHover = true;
					CloseLbl?.Invalidate();
				}
			};
			CloseLbl.MouseLeave += (s, e) => {
				if (HandleEvents) {
					closeMouseHover = false;
					CloseLbl?.Invalidate();
				}
			};
			CloseLbl.Paint += (s, e) => {
				if (HandleEvents) {
					Pen p = new Pen(closeLblEnabled ? CloseLblForeColor : CloseLblDisabledColor, 2);

					if (closeMouseHover && closeLblEnabled) {
						e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
						Rectangle rect = new Rectangle(1, 1, e.ClipRectangle.Width - 2, e.ClipRectangle.Height - 2);
						DrawBackground(e.Graphics, new SolidBrush(CloseLblBackColor), rect);

						if (CloseLblHoverForeColor != CloseLblBackColor)
							p = new Pen(CloseLblHoverForeColor, p.Width);
					}
					e.Graphics.SmoothingMode = SmoothingMode.None;

					e.Graphics.DrawLine(p, 6, 6, 14, 14);
					e.Graphics.DrawLine(p, 14, 6, 6, 14);

					e.Graphics.FillRectangle(new SolidBrush(p.Color), 6, 6, 1, 1);
					e.Graphics.FillRectangle(new SolidBrush(p.Color), 14, 6, 1, 1);
				}
			};
			CloseLbl.MouseClick += (s, e) => {
				if (HandleEvents && CloseLblEnabled && CloseLbl.ClientRectangle.Contains(e.Location)) {
					switch (e.Button) {
						case MouseButtons.Left:
							parent.Close();
							break;
						case MouseButtons.Right:
							parent.DecorationMouseUp(new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0), HitTest.Nowhere);
							break;
					}
				}
			};
		}

		private void InitMaximizeLbl() {
			MaxLbl = new Label {
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Location = new Point(CloseLbl.Left - 20, 5),
				Size = new Size(20, 20),
				Margin = Padding.Empty,
				AutoSize = false,
				Parent = this
			};

			MaxLbl.MouseEnter += (s, e) => {
				if (HandleEvents) {
					maxMouseHover = true;
					MaxLbl?.Invalidate();
				}
			};
			MaxLbl.MouseLeave += (s, e) => {
				if (HandleEvents) {
					maxMouseHover = false;
					MaxLbl?.Invalidate();
				}
			};
			MaxLbl.Paint += (s, e) => {
				if (HandleEvents) {
					Pen p = new Pen(MaxLblEnabled ? MaxLblForeColor : MaxLblDisabledColor);

					if (maxMouseHover && MaxLblEnabled) {
						e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
						Rectangle rect = new Rectangle(1, 1, e.ClipRectangle.Width - 2, e.ClipRectangle.Height - 2);
						DrawBackground(e.Graphics, new SolidBrush(MaxLblBackColor), rect);

						if (MaxLblHoverForeColor != MaxLblBackColor)
							p = new Pen(MaxLblHoverForeColor, p.Width);
					}
					e.Graphics.SmoothingMode = SmoothingMode.None;

					if (parent.MinMaxState == FormWindowState.Maximized) {
						e.Graphics.DrawLine(p, 5, 9, 5, 14);
						e.Graphics.DrawLine(p, 11, 9, 11, 14);
						e.Graphics.DrawLine(p, 5, 14, 11, 14);
						e.Graphics.DrawLine(p, 8, 9, 8, 6);
						e.Graphics.DrawLine(p, 12, 11, 14, 11);
						e.Graphics.DrawLine(p, 14, 11, 14, 6);

						p.Width = 2;
						e.Graphics.DrawLine(p, 8, 6, 15, 6);
						e.Graphics.DrawLine(p, 5, 9, 12, 9);
					} else {
						e.Graphics.DrawLine(p, 6, 7, 6, 14);
						e.Graphics.DrawLine(p, 14, 7, 14, 14);
						e.Graphics.DrawLine(p, 6, 14, 14, 14);

						p.Width = 3;
						e.Graphics.DrawLine(p, 6, 7, 15, 7);
					}
				}
			};
			MaxLbl.MouseClick += (s, e) => {
				if (HandleEvents && MaxLblEnabled && MaxLbl.ClientRectangle.Contains(e.Location)) {
					switch (e.Button) {
						case MouseButtons.Left:
							parent.WindowState = (FormWindowState) Math.Abs((int) parent.MinMaxState - 2);
							break;
						case MouseButtons.Right:
							parent.DecorationMouseUp(new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0), HitTest.Nowhere);
							break;
					}
				}
			};
		}

		private void InitMinimizeLbl() {
			MinLbl = new Label {
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Location = new Point(MaxLbl.Left - 20, 5),
				Size = new Size(20, 20),
				Margin = Padding.Empty,
				AutoSize = false,
				Parent = this
			};

			MinLbl.MouseEnter += (s, e) => {
				if (HandleEvents) {
					minMouseHover = true;
					MinLbl?.Invalidate();
				}
			};
			MinLbl.MouseLeave += (s, e) => {
				if (HandleEvents) {
					minMouseHover = false;
					MinLbl?.Invalidate();
				}
			};
			MinLbl.Paint += (s, e) => {
				if (HandleEvents) {
					Pen p = new Pen(MinLblEnabled ? MinLblForeColor : MinLblDisabledColor, 3);

					if (minMouseHover && MinLblEnabled) {
						e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

						Rectangle rect = new Rectangle(1, 1, e.ClipRectangle.Width - 2, e.ClipRectangle.Height - 2);
						DrawBackground(e.Graphics, new SolidBrush(MinLblBackColor), rect);

						if (MinLblHoverForeColor != MinLblBackColor)
							p = new Pen(MinLblHoverForeColor, p.Width);
					}
					e.Graphics.SmoothingMode = SmoothingMode.None;
					e.Graphics.DrawLine(p, 7, 13, 15, 13);
				}
			};
			MinLbl.MouseClick += (s, e) => {
				if (HandleEvents && MinLblEnabled) {
					switch (e.Button) {
						case MouseButtons.Left:
							parent.WindowState = FormWindowState.Minimized;
							break;
						case MouseButtons.Right:
							parent.DecorationMouseUp(new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0), HitTest.Nowhere);
							break;
					}
				}
			};
		}

		private void DrawBackground(Graphics graphics, Brush brush, Rectangle rect) {
			switch (BackgroundShape) {
				case ShapeType.Rectangle:
					graphics.FillRectangle(brush, rect);
					break;
				case ShapeType.Circle:
					graphics.FillEllipse(brush, rect);
					break;
			}
		}

		private void TitleBarMouseDown(object sender, MouseEventArgs e) {
			parent.DecorationMouseDown(e, HitTest.Caption);
		}

		private void TitleBarMouseUp(object sender, MouseEventArgs e) {
			parent.DecorationMouseUp(e, HitTest.Caption);
		}
	}

	public enum ShapeType {
		Rectangle,
		Circle
	}
}
