using LD44.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ruut.Graphics;
using Ruut.Input;
using Ruut.Screens;
using System;

namespace LD44.Screens {
    public sealed class WinScreen : IScreen {
        private readonly LD44Game _game;

        private readonly RendererSettings _uiSettings = new RendererSettings {
            SamplerState = SamplerState.PointClamp,
            OriginMode = OriginMode.Relative
        };

        public WinScreen(LD44Game game) {
            _game = game;
        }

        public event ScreenEventHandler ReplacedSelf;
        public event ScreenEventHandler PushedScreen;
        public event EventHandler PoppedSelf;

        public void HandleInput(InputState inputState, InputBindings bindings) {
            if (bindings.JustPressed("attack")) {
                ReplacedSelf?.Invoke(this, new ScreenEventArgs(new GameScreen(_game, _game.JungleTemplate, new PlayerData { Prologue = false })));
            }
            else if (bindings.JustPressed("restart")) {
                ReplacedSelf?.Invoke(this, new ScreenEventArgs(new GameScreen(_game, _game.EntranceTemplate, new PlayerData())));
            }
        }

        public void Update(GameTime gameTime) {
        }

        public void Draw(Renderer renderer) {
            renderer.Begin(_uiSettings);

            renderer.Draw(new Text("normal", "You defeated Volgax!\n\n\n\nPress [Z] to restart at the jungle!\n\nPress [R] to go back to the beginning.") {
                Origin = new Vector2(0.5f)
            }, new Vector2(GameProperties.Width, GameProperties.Height) / 2f);

            renderer.End();
        }
    }
}
