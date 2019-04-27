using LD44.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Ruut.Content;
using Ruut.Graphics;
using Ruut.Input;
using Ruut.Screens;

namespace LD44 {
    public sealed class LD44Game : Game {
        private readonly GraphicsDeviceManager _graphics;
        private readonly ScreenStack _screens;
        private readonly InputBindings _inputBindings;

        private Renderer _renderer;
        private RenderTarget2D _renderTarget;
        private SpriteBatch _spriteBatch;

        public LD44Game() {
            _graphics = new GraphicsDeviceManager(this);
            _screens = new ScreenStack();
            _inputBindings = new InputBindings();

            Content.RootDirectory = "Content";
        }

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

            base.Initialize();
        }

        protected override void LoadContent() {
            _screens.Push(new GameScreen());
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
