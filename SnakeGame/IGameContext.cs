using System.Drawing;

namespace SnakeGame {

	public interface IGameContext {

		Graphics GetGraphics();
		void Redraw();

		int GetWidth();
		int GetHeight();
		Rectangle GetClientArea();
	}
}
