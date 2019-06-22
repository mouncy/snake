using System;
using System.Windows.Forms;

namespace SnakeGame.Util {

	public class Animator {

		private bool enabled;

		public void CreateTimer(Action<Timer> action, int interval = 100, bool tickAtCreation = true) {
			if (action == null) return;
			enabled = true;

			Timer timer = new Timer {
				Interval = interval,
				Enabled = true
			};
			if (tickAtCreation)
				action.Invoke(timer);

			timer.Tick += (s, e) => {
				if (!enabled)
					timer.Enabled = false;
				else
					action.Invoke(timer);
			};
		}

		public void DoAnimation(Action<int> animation, Action callback = null, int frames = 10, int interval = 100) {
			if (animation == null) {
				callback?.Invoke();
				return;
			}

			int frame = 0;
			enabled = true;

			CreateTimer(timer => {
				if (!enabled || frame >= frames) {
					timer.Enabled = false;
					callback?.Invoke();
				} else {
					animation.Invoke(frame);
					frame++;
				}
			}, interval);
		}

		public void Stop() {
			enabled = false;
		}
	}
}
