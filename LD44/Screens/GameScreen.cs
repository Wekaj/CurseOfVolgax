using LD44.Levels;
using LD44.Mobs;
using LD44.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ruut.Graphics;
using Ruut.Input;
using Ruut.Screens;
using System;

namespace LD44.Screens {
    public sealed class GameScreen : IScreen {
        private readonly RendererSettings _worldSettings = new RendererSettings {
            SamplerState = SamplerState.PointClamp
        };
        private readonly RendererSettings _uiSettings = new RendererSettings {
            SamplerState = SamplerState.PointClamp,
            OriginMode = OriginMode.Relative
        };
        private readonly Camera _camera = new Camera();

        private IMob _playerMob;

        private Vector2 _movement;
        private bool _climbing = false;

        private float _timer = 60f;

        public GameScreen() {
            var random = new Random();
            bool was = false;
            for (int y = 0; y < Level.Height; y++) {
                for (int x = 0; x < Level.Width; x++) {
                    if (was && random.Next(4) > 0
                        || !was && random.Next(4) == 0) {
                        Level.GetTile(x, y).FrontSprite.Texture = "block";
                        Level.GetTile(x, y).TileType = TileType.Rock;
                        was = true;
                    }
                    else {
                        was = false;
                    }
                }
            }

            _playerMob = new PlayerMob();
            Level.Mobs.Add(_playerMob);
        }

        public event ScreenEventHandler ReplacedSelf;
        public event ScreenEventHandler PushedScreen;
        public event EventHandler PoppedSelf;

        public Level Level { get; } = new Level(32, 32);

        public void HandleInput(InputState inputState, InputBindings bindings) {
            _movement = Vector2.Zero;
            if (bindings.IsPressed("move_right")) {
                _movement.X++;
            }
            if (bindings.IsPressed("move_left")) {
                _movement.X--;
            }

            if (bindings.JustPressed("jump")) {
                _playerMob.Body.Velocity -= new Vector2(0f, 11.5f);
            }
        }

        public void Update(GameTime gameTime) {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _playerMob.Body.Position += _movement * 6.5f * delta;

            _movement = Vector2.Zero;

            foreach (IMob mob in Level.Mobs) {
                mob.Body.Velocity += new Vector2(0f, 30f) * delta;

                BodyPhysics.Update(mob.Body, delta);

                TilePhysics.DoTileCollisions(mob.Body, Level);
            }

            if (_timer <= 5f) {
                _camera.Zoom = 3f - (int)Math.Ceiling(_timer) / 3f;
            }

            _camera.Position = _playerMob.Body.Position * GameProperties.TileSize 
                - new Vector2(GameProperties.Width, GameProperties.Height) / _camera.Zoom / 2f;

            _worldSettings.TransformMatrix = _camera.GetTransformMatrix();

            _timer -= delta;
        }

        public void Draw(Renderer renderer) {
            renderer.Begin(_worldSettings);

            for (int y = 0; y < Level.Height; y++) {
                for (int x = 0; x < Level.Width; x++) {
                    renderer.Draw(Level.GetTile(x, y).BackSprite, new Vector2(x, y) * GameProperties.TileSize);
                }
            }
            for (int y = 0; y < Level.Height; y++) {
                for (int x = 0; x < Level.Width; x++) {
                    renderer.Draw(Level.GetTile(x, y).FrontSprite, new Vector2(x, y) * GameProperties.TileSize);
                }
            }

            renderer.Draw(_playerMob.Sprite, _playerMob.Body.Position * GameProperties.TileSize);

            renderer.End();

            renderer.Begin(_uiSettings);

            int seconds = (int)Math.Ceiling(_timer);
            string timerText = seconds + " SECONDS";
            if (seconds == 1) {
                timerText = "1 SECOND ";
            }
            renderer.Draw(new Text("normal", timerText) { Color = Color.White, Origin = new Vector2(1f, 0.5f) }, new Vector2(GameProperties.Width / 2f + 39f, 12f));

            renderer.End();
        }
    }
}
