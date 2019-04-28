using LD44.Generation;
using LD44.Levels;
using LD44.Mobs;
using LD44.Physics;
using LD44.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ruut;
using Ruut.Animation;
using Ruut.Graphics;
using Ruut.Input;
using Ruut.Screens;
using System;

namespace LD44.Screens {
    public sealed class GameScreen : IScreen {
        private readonly Random _random = new Random();

        private readonly LD44Game _game;

        private readonly RendererSettings _worldSettings = new RendererSettings {
            SamplerState = SamplerState.PointClamp,
            OriginMode = OriginMode.Relative
        };
        private readonly RendererSettings _uiSettings = new RendererSettings {
            SamplerState = SamplerState.PointClamp,
            OriginMode = OriginMode.Relative
        };
        private readonly Camera _camera = new Camera();

        private readonly SpriteFont _normalFont;

        private PlayerMob _playerMob;

        private Vector2 _movement;
        private bool _climbing = false;

        private float _timer = 60f;

        private string _message = null;
        private Text _displayMessage = new Text("normal", "") { Color = Color.White, Origin = new Vector2(0.5f) };
        private float _messageTimer = 0f;
        private int _messageChar = 0;
        private Vector2 _messageSource;

        public GameScreen(LD44Game game, LevelTemplate template) {
            _game = game;
            _normalFont = _game.Content.Load<SpriteFont>("Fonts/normal");

            Level = Generator.GenerateLevel(game, template, _random);

            _playerMob = new PlayerMob();
            _playerMob.Body.Position = new Vector2(3f);
            if (Level.Entrance != null) {
                _playerMob.Body.Position = Level.Entrance.Value;
            }
            Level.Mobs.Add(_playerMob);
        }

        public event ScreenEventHandler ReplacedSelf;
        public event ScreenEventHandler PushedScreen;
        public event EventHandler PoppedSelf;

        public Level Level { get; }

        public void HandleInput(InputState inputState, InputBindings bindings) {
            _movement = Vector2.Zero;
            if (bindings.IsPressed("move_right")) {
                _movement.X++;
            }
            if (bindings.IsPressed("move_left")) {
                _movement.X--;
            }

            if (bindings.JustPressed("jump") && (_playerMob.Body.Contact.Y > 0f || _playerMob.LenienceTimer > 0f)) {
                _playerMob.Body.Velocity -= new Vector2(0f, 11.5f);
                _playerMob.LenienceTimer = 0f;
            }

            if (bindings.JustPressed("interact")) {
                Interactable interacting = null;

                RectangleF playerRegion = _playerMob.Body.Bounds;
                playerRegion.X = _playerMob.Body.Position.X - _playerMob.Body.Bounds.Width / 2f;
                playerRegion.Y = _playerMob.Body.Position.Y - _playerMob.Body.Bounds.Height / 2f;
                foreach (Interactable interactable in Level.Interactables) {
                    RectangleF region = interactable.Region;
                    region.X = interactable.Position.X - region.Width / 2f;
                    region.Y = interactable.Position.Y - region.Height / 2f;

                    if (region.Intersects(playerRegion)) {
                        interacting = interactable;
                        break;
                    }
                }

                if (interacting != null) {
                    switch (interacting.InteractableType) {
                        case InteractableType.Message: {
                            _message = TextUtilities.WrapText(_normalFont, interacting.Message, 256f);
                            _displayMessage.Contents = "";
                            _messageTimer = 0f;
                            _messageChar = 0;
                            _messageSource = interacting.Position;
                            break;
                        }
                        case InteractableType.Door: {
                            ReplacedSelf?.Invoke(this, new ScreenEventArgs(new GameScreen(_game, interacting.Destination)));
                            break;
                        }
                    }
                }
            }
        }

        public void Update(GameTime gameTime) {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_playerMob.Body.Contact.Y > 0f) {
                _playerMob.LenienceTimer = 0.1f;
            }
            else if (_playerMob.LenienceTimer > 0f) {
                _playerMob.LenienceTimer -= delta;
            }

            _playerMob.Body.Position += _movement * 6.5f * delta;

            _movement = Vector2.Zero;

            foreach (IMob mob in Level.Mobs) {
                if (mob.Gravity) {
                    mob.Body.Velocity += new Vector2(0f, 30f) * delta;
                }

                BodyPhysics.Update(mob.Body, delta);

                TilePhysics.DoTileCollisions(mob.Body, Level);

                mob.Update(delta);

                if (mob.Animation != null) {
                    mob.Animation.Update(delta);
                    mob.Animation.Apply(mob.Sprite);
                }
            }

            if (_timer <= 5f) {
                _camera.Zoom = 3f - (int)Math.Ceiling(_timer) / 3f;
            }

            _camera.Position = _playerMob.Body.Position * GameProperties.TileSize 
                - new Vector2(GameProperties.Width, GameProperties.Height) / _camera.Zoom / 2f;

            Vector2 camPos = _camera.Position;
            camPos.X = Math.Max(camPos.X, 0f);
            camPos.Y = Math.Max(camPos.Y, 0f);
            camPos.X = Math.Min(camPos.X, Level.Width * GameProperties.TileSize - GameProperties.Width);
            camPos.Y = Math.Min(camPos.Y, Level.Height * GameProperties.TileSize - GameProperties.Height);
            _camera.Position = camPos;

            _worldSettings.TransformMatrix = _camera.GetTransformMatrix();

            _timer -= delta;

            if (_message != null && _displayMessage.Contents.Length < _message.Length) {
                _messageTimer += delta;

                while (_messageTimer >= 0.075f) {
                    _messageTimer -= 0.075f;

                    _messageChar++;
                    _displayMessage.Contents = _message.Substring(0, _messageChar);
                }
            }

            if (Vector2.Distance(_playerMob.Body.Position, _messageSource) > 3f) {
                _message = null;
            }

            foreach (Interactable interactable in Level.Interactables) {
                if (interactable.Animation != null) {
                    interactable.Animation.Update(delta);
                    interactable.Animation.Apply(interactable.Sprite);
                }
            }
        }

        public void Draw(Renderer renderer) {
            renderer.Begin(_worldSettings);

            float regionWidth = Level.Width * GameProperties.TileSize;
            float regionHeight = Level.Height * GameProperties.TileSize;

            float dx = _camera.Position.X / (regionWidth - GameProperties.Width);
            float dy = _camera.Position.Y / (regionHeight - GameProperties.Height);

            Texture2D bgTexture = _game.Content.Load<Texture2D>("Textures/" + Level.Background.Texture);

            renderer.Draw(Level.Background, new Vector2(dx * -(bgTexture.Width - regionWidth), dy * -(bgTexture.Height - regionHeight)));

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

            foreach (Interactable interactable in Level.Interactables) {
                renderer.Draw(interactable.Sprite, interactable.Position * GameProperties.TileSize);
            }

            if (_message != null) {
                Vector2 textSize = _normalFont.MeasureString(_displayMessage.Contents);
                Vector2 borderSize = textSize + new Vector2(6f);

                Vector2 center = _messageSource * GameProperties.TileSize - new Vector2(0f, 30f);

                float cameraRight = _camera.Position.X + GameProperties.Width;
                float cameraBottom = _camera.Position.Y + GameProperties.Height;

                if (center.X - borderSize.X / 2f < _camera.Position.X + 3f) {
                    center.X = _camera.Position.X + borderSize.X / 2f + 3f;
                }
                if (center.Y - borderSize.Y / 2f < _camera.Position.Y + 3f) {
                    center.Y = _camera.Position.Y + borderSize.Y / 2f + 3f;
                }
                if (center.X + borderSize.X / 2f > cameraRight - 3f) {
                    center.X = cameraRight - borderSize.X / 2f - 3f;
                }
                if (center.Y + borderSize.Y / 2f > cameraBottom - 3f) {
                    center.Y = cameraRight - borderSize.Y / 2f - 3f;
                }

                renderer.Draw(new Sprite("pixel") {
                    Color = Color.Black,
                    Scale = borderSize,
                    Origin = new Vector2(0.5f)
                }, center);
                renderer.Draw(_displayMessage, center);
            }

            foreach (IMob mob in Level.Mobs) {
                renderer.Draw(mob.Sprite, mob.Body.Position * GameProperties.TileSize);
            }
            
            bool canInteract = false;

            RectangleF playerRegion = _playerMob.Body.Bounds;
            playerRegion.X = _playerMob.Body.Position.X - _playerMob.Body.Bounds.Width / 2f;
            playerRegion.Y = _playerMob.Body.Position.Y - _playerMob.Body.Bounds.Height / 2f;
            foreach (Interactable interactable in Level.Interactables) {
                RectangleF region = interactable.Region;
                region.X = interactable.Position.X - region.Width / 2f;
                region.Y = interactable.Position.Y - region.Height / 2f;

                if (region.Intersects(playerRegion)) {
                    canInteract = true;
                    break;
                }
            }

            if (canInteract) {
                renderer.Draw(new Sprite("arrow_up"), _playerMob.Body.Position * GameProperties.TileSize - new Vector2(6f, 24f));
            }

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
