using System;
using System.Drawing;
using System.Windows.Forms;

namespace SnakeGame {

	public class Canvas : Panel {

		public Action<Graphics> PaintAction { get; set; }

		private bool visible = true;
		private Point startPoint;
		private Timer timer;

		public Canvas() : this(true) {

		}

		public Canvas(bool autoHideCursor) : base() {
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
			UpdateStyles();

			if (autoHideCursor) {
				timer = new Timer { Interval = 1000 };
				timer.Tick += HandleTimerTick;
			}
		}

		protected override void OnPaintBackground(PaintEventArgs e) {

		}

		protected override void OnPaint(PaintEventArgs e) {
			PaintAction?.Invoke(e.Graphics);
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			if (timer != null) {
				startPoint = PointToClient(MousePosition);
				ShowCursor();

				timer.Enabled = true;
			}
			base.OnMouseMove(e);
		}

		private void HandleTimerTick(object sender, EventArgs e) {
			if (startPoint == PointToClient(MousePosition)) {
				HideCursor();
			}
			timer.Enabled = false;
		}

		private void HideCursor() {
			if (visible) {
				visible = false;
				Cursor.Hide();
			}
		}

		private void ShowCursor() {
			if (!visible) {
				visible = true;
				Cursor.Show();
			}
		}
	}
}
