using LD44.Generation;
using LD44.Levels;
using LD44.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Ruut.Animation;
using Ruut.Content;
using Ruut.Graphics;
using Ruut.Input;
using Ruut.Screens;
using System;
using System.Collections.Generic;

namespace LD44 {
    public sealed class LD44Game : Game {
        private readonly GraphicsDeviceManager _graphics;
        private readonly ScreenStack _screens;
        private readonly InputBindings _inputBindings;
        private readonly Dictionary<string, IAnimation<Sprite>> _spriteAnimations = new Dictionary<string, IAnimation<Sprite>>();

        private Renderer _renderer;
        private RenderTarget2D _renderTarget;
        private SpriteBatch _spriteBatch;

        public LD44Game() {
            _graphics = new GraphicsDeviceManager(this);
            _screens = new ScreenStack();
            _inputBindings = new InputBindings();

            Content.RootDirectory = "Content";
        }

        public IReadOnlyDictionary<string, IAnimation<Sprite>> SpriteAnimations => _spriteAnimations;

        public LevelTemplate EntranceTemplate { get; private set; }
        public LevelTemplate JungleTemplate { get; private set; }

        protected override void Initialize() {
            _graphics.PreferredBackBufferWidth = GameProperties.Width * 2;
            _graphics.PreferredBackBufferHeight = GameProperties.Height * 2;
            _graphics.ApplyChanges();

            _renderer = new Renderer(GraphicsDevice, 
                new ContentManagerProvider<Texture2D>(Content) { Prefix = "Textures/" },
                new ContentManagerProvider<SpriteFont>(Content) { Prefix = "Fonts/" });
            _renderTarget = new RenderTarget2D(GraphicsDevice, GameProperties.Width, GameProperties.Height);
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            IsMouseVisible = true;

            Window.Title = GameProperties.Name;
            Window.AllowUserResizing = true;

            _inputBindings.Set("move_left", new KeyboardBinding(Keys.Left));
            _inputBindings.Set("move_right", new KeyboardBinding(Keys.Right));
            _inputBindings.Set("jump", new KeyboardBinding(Keys.Space));
            _inputBindings.Set("interact", new KeyboardBinding(Keys.Up));

            _spriteAnimations.Add("trader_idle", new FixedFrameAnimation("trader", 16, 16)
                .AddFrame(0, 0).AddFrame(1, 0).AddFrame(2, 0).AddFrame(1, 0));
            _spriteAnimations.Add("bat_flying", new FixedFrameAnimation("bat", 16, 16)
                .AddFrame(0, 0).AddFrame(1, 0).AddFrame(2, 0).AddFrame(2, 0).AddFrame(3, 0));

            base.Initialize();
        }

        protected override void LoadContent() {
            EntranceTemplate = new LevelTemplate(ChunkSet.FromTexture(Content.Load<Texture2D>("Levels/entrance"), 2), 2, 1, true, "bg_entrance");
            JungleTemplate = new LevelTemplate(ChunkSet.FromTexture(Content.Load<Texture2D>("Levels/jungle"), 15), 4, 4, false, "empty");

            _screens.Push(new GameScreen(this, EntranceTemplate));
        }

        protected override void UnloadContent() {
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed 
                || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                Exit();
            }

            if (IsActive) {
                var inputState = InputState.GetCurrentState();
                _inputBindings.Update(inputState);

                _screens.HandleInput(inputState, _inputBindings);
            }

            _screens.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.SetRenderTarget(_renderTarget);
            _screens.Draw(_renderer);
            GraphicsDevice.SetRenderTarget(null);

            Vector2 scale;
            float windowRatio = GraphicsDevice.Viewport.AspectRatio;
            float renderRatio = (float)GameProperties.Width / GameProperties.Height;
            if (renderRatio > windowRatio) {
                scale = new Vector2((float)GraphicsDevice.Viewport.Width / GameProperties.Width);
            }
            else {
                scale = new Vector2((float)GraphicsDevice.Viewport.Height / GameProperties.Height);
            }

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
#pragma warning disable CS0618 // Type or member is obsolete
            _spriteBatch.Draw(_renderTarget, 
                position: GraphicsDevice.Viewport.Bounds.Center.ToVector2(), 
                origin: _renderTarget.Bounds.Center.ToVector2(),
                scale: scale);
#pragma warning restore CS0618 // Type or member is obsolete
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
