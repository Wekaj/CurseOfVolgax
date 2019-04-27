using Microsoft.Xna.Framework;
using Ruut.Graphics;
using Ruut.Input;
using Ruut.Screens;
using System;

namespace LD44.Screens {
    public sealed class GameScreen : IScreen {
        public event ScreenEventHandler ReplacedSelf;
        public event ScreenEventHandler PushedScreen;
        public event EventHandler PoppedSelf;

        public void HandleInput(InputState inputState, InputBindings bindings) {
        }

        public void Update(GameTime gameTime) {
        }

        public void Draw(Renderer renderer) {
            renderer.Begin();
            renderer.Draw(new Sprite("block"), Vector2.Zero);
            renderer.End();
        }
    }
}
