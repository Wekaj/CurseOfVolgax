using LD44.Generation;
using LD44.Items;
using LD44.Levels;
using LD44.Mobs;
using LD44.Physics;
using LD44.Player;
using LD44.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Ruut;
using Ruut.Animation;
using Ruut.Extensions;
using Ruut.Graphics;
using Ruut.Input;
using Ruut.Screens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LD44.Screens {
    public sealed class GameScreen : IScreen {
        private sealed class LifespanChange {
            public LifespanChange(float amount, Vector2 position) {
                Text = new Text("normal", (amount > 0f ? "+" : "") + amount.ToString()) {
                    Origin = new Vector2(0.5f),
                    Color = amount > 0f ? Color.LightGreen : Color.Red
                };
                Color = amount > 0f ? Color.LightGreen : Color.Red;
                Position = position;
            }

            public Text Text { get; }
            public Color Color { get; }
            public Vector2 Position { get; set; }
            public float Life { get; set; } = 0f;
        }

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

        private PlayerData _playerData;

        private PlayerMob _playerMob;

        private bool _isDead = false;
        private LevelTemplate _destination = null;

        private Vector2 _movement;
        private bool _climbing = false;

        private string _message = null;
        private Text _displayMessage = new Text("normal", "") { Color = Color.White, Origin = new Vector2(0.5f) };
        private float _messageTimer = 0f;
        private int _messageChar = 0;
        private Vector2 _messageSource;
        private float _messageStay = 0f;

        private Sprite _transition = new Sprite("transition");
        private float _transitionStart = 0f, _transitionEnd = 1f;
        private float _transitionCurrent = 0f;
        private bool _transitioning = true;

        private List<Item> _shop = null;
        private int _shopSelection = 0;

        private Sprite _swordSprite = new Sprite("sword") {
            Origin = new Vector2(0f, 1f)
        };
        private float _swordCharge = 0f;
        private bool _isCharging = false;
        private bool _isSwinging = false;
        private bool _powerSwing = false;
        private float _swingStart = 0f;
        private float _swingProgress = 0f;

        private bool _doubleJumped = false;
        private bool _jetpack = false;

        private bool _showValgox = false;
        private Vector2 _valgoxStart, _valgoxEnd;
        private float _valgoxTimer, _valgoxDuration;
        private Sprite _valgoxSprite = new Sprite("valgox_aiming") {
            Origin = new Vector2(0.5f)
        };

        private List<LifespanChange> _lifespanChanges = new List<LifespanChange>();

        private readonly List<Item> _items = new List<Item> {
            new Item("ARMOR", 10),
            new Item("ARMOR", 10),
            new Item("ARMOR", 10),
            new Item("CURSED SWORD", 10),
            new Item("JETPACK", 60),
            new Item("COUPON", 20),
            new Item("ENCHANTED FEATHER", 25),
            new Item("MASTER SWORD", 35),
            new Item("HONED SWORD", 10),
            new Item("HONED SWORD", 10),
            new Item("RUNNING SHOES", 10),
            new Item("RUNNING SHOES", 10),
            new Item("LIGHTNING SWORD", 20),
            new Item("SCYTHE", 25),
        };

        private float _shake;

        private readonly List<Interactable> _ignorables = new List<Interactable>();

        private SoundEffectInstance _music;

        public GameScreen(LD44Game game, LevelTemplate template, PlayerData playerData) {
            _game = game;
            _playerData = playerData;
            _normalFont = _game.Content.Load<SpriteFont>("Fonts/normal");

            if (template.Theme != null) {
                _music = game.Content.Load<SoundEffect>(template.Theme).CreateInstance();
                _music.Volume = 0.25f;
                _music.IsLooped = true;
                _music.Play();
            }

            Level = Generator.GenerateLevel(game, template, _random);

            if (Level.Exits.Count > 0) {
                Point exit = Level.Exits.OrderByDescending(p => p.X).First();
                var door = new Interactable {
                    Position = exit.ToVector2() + new Vector2(0.5f),
                    Region = new RectangleF(0f, 0f, 1f, 1f),

                    InteractableType = InteractableType.Door,

                    Destination = template.Destination
                };
                door.Sprite.Texture = "door";
                door.Sprite.Origin = new Vector2(0.5f);
                Level.Interactables.Add(door);
            }

            for (int i = 0; i < 3; i++) {
                if (Level.Spots.Count == 0) {
                    break;
                }

                int choice = _random.Next(Level.Spots.Count);
                Point spot = Level.Spots[choice];
                Level.Spots.RemoveAt(choice);

                string message = "";
                switch (_random.Next(4)) {
                    case 0: {
                        message = "Your life... pleasssse sssshare ssssome.";
                        break;
                    }
                    case 1: {
                        message = "Do you have any ssssecondssss to sssspare?";
                        break;
                    }
                    case 2: {
                        message = "I have many sssspecial itemssss for ssssale.";
                        break;
                    }
                    case 3: {
                        message = "Give me your ssssecondssss... I'll give you ssssomething in return.";
                        break;
                    }
                }

                var merchant = new Interactable {
                    Position = new Vector2(spot.X, spot.Y) + new Vector2(0.5f),
                    Region = new RectangleF(0f, 0f, 1f, 1f),

                    InteractableType = InteractableType.Merchant,

                    Message = message
                };
                merchant.Sprite.Texture = "beggar";
                merchant.Sprite.Effects = _random.Next(2) == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally; 
                merchant.Sprite.Origin = new Vector2(0.5f, 0.75f);
                merchant.Animation = new AnimationState<Sprite>(_game.SpriteAnimations["beggar_idle"], 0.5f) {
                    IsLooping = true
                };
                for (int j = 0; j < 3; j++) {
                    merchant.Inventory.Add(new Item(_items[_random.Next(_items.Count)], _random));
                }
                Level.Interactables.Add(merchant);
            }

            _playerMob = new PlayerMob();
            _playerMob.Body.Position = new Vector2(3f);
            if (Level.Entrance != null) {
                _playerMob.Body.Position = Level.Entrance.Value;
            }
            else {
                Point entrance = Level.Entrances.OrderBy(p => p.X).First();

                Level.GetTile(entrance.X, entrance.Y).FrontSprite.Texture = "door";

                _playerMob.Body.Position = entrance.ToVector2() + new Vector2(0.5f);
            }
            Level.Mobs.Add(_playerMob);
        }

        public event ScreenEventHandler ReplacedSelf;
        public event ScreenEventHandler PushedScreen;
        public event EventHandler PoppedSelf;

        public Level Level { get; }

        public void HandleInput(InputState inputState, InputBindings bindings) {
            bool wasCharging = _isCharging;
            _isCharging = false;

            if (_isDead) {
                return;
            }

            if (_shop != null) {
                if (bindings.JustPressed("select_up")) {
                    _shopSelection--;
                    if (_shopSelection < 0) {
                        _shopSelection = 3;
                    }
                }
                if (bindings.JustPressed("select_down")) {
                    _shopSelection++;
                    if (_shopSelection > 3) {
                        _shopSelection = 0;
                    }
                }
                if (bindings.JustPressed("select")) {
                    if (_shopSelection < 3 && _shop[_shopSelection] != null) {
                        Item item = _shop[_shopSelection];
                        _shop[_shopSelection] = null;

                        int cost = item.Cost;
                        if (_playerData.HasCoupon) {
                            cost = Math.Max(cost / 2, 1);
                        }

                        _playerData.Lifespan -= cost;
                        _lifespanChanges.Add(new LifespanChange(-cost, _playerMob.Body.Position));
                        _game.Content.Load<SoundEffect>("Sounds/lose").Play();

                        switch (item.Name) {
                            case "ARMOR": {
                                _playerData.Armor++;
                                break;
                            }
                            case "COUPON": {
                                _playerData.HasCoupon = true;
                                break;
                            }
                            case "CURSED SWORD": {
                                _playerData.Damage = 6;
                                _playerData.Sword = "cursed_sword";
                                _playerData.Cursed = true;
                                _playerData.Dash = false;
                                _playerData.Steal = false;
                                break;
                            }
                            case "ENCHANTED FEATHER": {
                                _playerData.DoubleJump = true;
                                break;
                            }
                            case "MASTER SWORD": {
                                _playerData.Damage = 7;
                                _playerData.Sword = "master_sword";
                                _playerData.Cursed = false;
                                _playerData.Dash = false;
                                _playerData.Steal = false;
                                break;
                            }
                            case "HONED SWORD": {
                                _playerData.Damage = 5;
                                _playerData.Sword = "honed_sword";
                                _playerData.Cursed = false;
                                _playerData.Dash = false;
                                _playerData.Steal = false;
                                break;
                            }
                            case "RUNNING SHOES": {
                                _playerData.Speed = 7.5f;
                                break;
                            }
                            case "LIGHTNING SWORD": {
                                _playerData.Damage = 4;
                                _playerData.Sword = "lightning_sword";
                                _playerData.Cursed = false;
                                _playerData.Dash = true;
                                _playerData.Steal = false;
                                break;
                            }
                            case "SCYTHE": {
                                _playerData.Damage = 3;
                                _playerData.Sword = "scythe";
                                _playerData.Cursed = false;
                                _playerData.Dash = false;
                                _playerData.Steal = true;
                                break;
                            }
                            case "JETPACK": {
                                _playerData.Jetpack = true;
                                break;
                            }
                        }
                    }
                    if (_shopSelection == 3) {
                        _shop = null;
                        _ignorables.Clear();
                    }
                }
                return;
            }

            if (_playerMob.StunTimer <= 0f && _shop == null) {
                _movement = Vector2.Zero;
                _jetpack = false;
                if (bindings.IsPressed("move_right")) {
                    _movement.X++;
                }
                if (bindings.IsPressed("move_left")) {
                    _movement.X--;
                }

                if (_movement.X > 0f) {
                    _playerMob.Sprite.Effects = SpriteEffects.None;
                }
                else if (_movement.X < 0f) {
                    _playerMob.Sprite.Effects = SpriteEffects.FlipHorizontally;
                }

                if (!_isSwinging && bindings.IsPressed("attack")) {
                    _isCharging = true;
                }

                if (wasCharging && bindings.JustReleased("attack")) {
                    _game.Content.Load<SoundEffect>("Sounds/slash").Play();

                    if (_playerData.Dash) {
                        _playerMob.Body.Velocity -= new Vector2(0f, 10f * _swordCharge);
                    }

                    if (_playerData.Cursed) {
                        _playerData.Lifespan -= 3;
                        _lifespanChanges.Add(new LifespanChange(-3, _playerMob.Body.Position));
                        _game.Content.Load<SoundEffect>("Sounds/lose").Play();
                    }

                    float p = _swordCharge;
                    p = 1f - (float)Math.Pow(1f - p, 2f);

                    float startAngle = 0f;
                    float targetAngle = (float)-Math.PI / 2f;

                    if (!_playerMob.FacingRight) {
                        startAngle = targetAngle;
                        targetAngle = 0f;
                    }

                    _swingStart = startAngle * (1f - p) + targetAngle * p;

                    _isCharging = false;
                    _isSwinging = true;
                    _powerSwing = _swordCharge >= 1f;
                    _swingProgress = 0f;
                }

                if (_playerData.Jetpack) {
                    if (bindings.IsPressed("jump")) {
                        _jetpack = true;
                    }
                }
                else if (bindings.JustPressed("jump")) {
                    if (_playerMob.Body.Contact.Y > 0f || _playerMob.LenienceTimer > 0f) {
                        _playerMob.Body.Velocity = new Vector2(_playerMob.Body.Velocity.X, -11.5f);
                        _playerMob.LenienceTimer = 0f;
                    }
                    else if (_playerData.DoubleJump && !_doubleJumped) {
                        _playerMob.Body.Velocity = new Vector2(_playerMob.Body.Velocity.X, -11.5f);
                        _playerMob.LenienceTimer = 0f;
                        _doubleJumped = true;
                    }
                }

                if (bindings.JustPressed("interact")) {
                    Interactable interacting = null;

                    RectangleF playerRegion = _playerMob.Body.Bounds;
                    playerRegion.X = _playerMob.Body.Position.X - _playerMob.Body.Bounds.Width / 2f;
                    playerRegion.Y = _playerMob.Body.Position.Y - _playerMob.Body.Bounds.Height / 2f;
                    foreach (Interactable interactable in Level.Interactables.Where(i => !_ignorables.Contains(i))) {
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
                                _destination = interacting.Destination;

                                _transitioning = true;
                                _transitionStart = -1f;
                                _transitionEnd = 0f;
                                _transitionCurrent = -1f;
                                break;
                            }
                            case InteractableType.Merchant: {
                                if (interacting.Message != null) {
                                    _message = TextUtilities.WrapText(_normalFont, interacting.Message, 256f);
                                    _displayMessage.Contents = "";
                                    _messageTimer = 0f;
                                    _messageChar = 0;
                                    _messageSource = interacting.Position;

                                    interacting.Message = null;
                                }
                                else {
                                    _shop = interacting.Inventory;
                                    _shopSelection = 0;
                                }
                                break;
                            }
                            case InteractableType.Blessing: {
                                if (interacting.Message != null) {
                                    _message = TextUtilities.WrapText(_normalFont, interacting.Message, 256f);
                                    _displayMessage.Contents = "";
                                    _messageTimer = 0f;
                                    _messageChar = 0;
                                    _messageSource = interacting.Position;

                                    interacting.Message = null;
                                }
                                else if (!interacting.Accepted) {
                                    _playerData.Lifespan += 60f;
                                    _lifespanChanges.Add(new LifespanChange(60, _playerMob.Body.Position));
                                    _game.Content.Load<SoundEffect>("Sounds/big_gain").Play();
                                    interacting.Accepted = true;
                                }
                                else {
                                    _message = TextUtilities.WrapText(_normalFont, "...", 256f);
                                    _displayMessage.Contents = "";
                                    _messageTimer = 0f;
                                    _messageChar = 0;
                                    _messageSource = interacting.Position;

                                    interacting.Message = null;
                                }
                                break;
                            }
                            case InteractableType.Valgox: {
                                if (interacting.Message != null) {
                                    _message = TextUtilities.WrapText(_normalFont, interacting.Message, 256f);
                                    _displayMessage.Contents = "";
                                    _messageTimer = 0f;
                                    _messageChar = 0;
                                    _messageSource = interacting.Position;

                                    interacting.Message = null;
                                }
                                else {
                                    _message = null;
                                    Level.Interactables.Remove(interacting);

                                    var valgox = new ValgoxMob();
                                    valgox.Body.Position = interacting.Position - new Vector2(0f, 0.35f * 2f);
                                    valgox.Animation = new AnimationState<Sprite>(_game.SpriteAnimations["valgox_idle"], 0.5f) {
                                        IsLooping = true
                                    };
                                    valgox.Sprite.Texture = "valgox";
                                    Level.Mobs.Add(valgox);
                                }
                                break;
                            }
                        }

                        _ignorables.Add(interacting);
                    }
                }
            }
        }

        public void Update(GameTime gameTime) {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < _lifespanChanges.Count; i++) {
                LifespanChange change = _lifespanChanges[i];

                change.Life += delta;
                change.Position -= new Vector2(0f, 3f * delta);

                float p = change.Life / 2f;
                change.Text.Color = change.Color * (1f - p * p * p);

                change.Position += new Vector2((float)Math.Cos(p * 20f) / 10f, 0f);

                if (change.Life > 2f) {
                    _lifespanChanges.RemoveAt(i);
                    i--;
                }
            }

            if (_showValgox) {
                float p = _valgoxTimer / _valgoxDuration;

                _valgoxTimer += delta;
                if (_valgoxTimer > _valgoxDuration) {
                    _valgoxTimer = _valgoxDuration;
                }

                if (_valgoxTimer / _valgoxDuration >= 0.5f && p < 0.5f) {
                    _playerData.Prologue = false;

                    _playerMob.StunTimer = 3f;
                    _playerMob.Body.Velocity = RandomUpVector() * 15f;
                    _playerMob.CollisionCooldownTimer = 0.5f;

                    for (int i = 0; i < 3; i++) {
                        var star = new StarMob();
                        star.Body.Position = _playerMob.Body.Position;
                        star.Body.Velocity = _random.NextUnitVector() * 6f;
                        Level.Mobs.Add(star);
                    }

                    _shake += 6f;

                    _game.Content.Load<SoundEffect>("Sounds/hurt").Play();
                    _game.Content.Load<SoundEffect>("Sounds/projectile").Play();

                    var change = new LifespanChange(-1f, _playerMob.Body.Position);
                    change.Text.Contents = "CURSED";
                    _lifespanChanges.Add(change);
                }

                if (_valgoxTimer / _valgoxDuration >= 0.75f && p < 0.75f) {
                    _message = TextUtilities.WrapText(_normalFont, "That'll teach you!", 256f);
                    _displayMessage.Contents = "";
                    _messageTimer = 0f;
                    _messageChar = 0;
                    _messageSource = _playerMob.Body.Position - new Vector2(0f, 2f);
                    _messageStay = 5f;
                }
            }

            if (_jetpack) {
                _playerMob.Body.Velocity -= new Vector2(0f, 50f) * delta;
            }

            if (_playerMob.Body.Contact.Y > 0f) {
                _doubleJumped = false;
            }

            if (_isCharging) {
                bool notFull = _swordCharge < 1f;

                if (_swordCharge <= 1f - delta) {
                    _swordCharge += delta;
                }
                else {
                    _swordCharge = 1f;
                }

                if (notFull && _swordCharge >= 1f) {
                    var flash = new FlashMob(_random);
                    flash.Body.Position = _playerMob.Body.Position;
                    Level.Mobs.Add(flash);

                    _game.Content.Load<SoundEffect>("Sounds/flash").Play();
                }
            }
            else {
                _swordCharge = 0f;
            }

            if (_isSwinging) {
                int damage = _playerData.Damage;
                if (_powerSwing) {
                    damage *= 3;
                }

                var region = new RectangleF(_playerMob.Body.Position.X, _playerMob.Body.Position.Y - 0.5f, 1f, 1.5f);
                if (!_playerMob.FacingRight) {
                    region.X -= region.Width;
                }

                foreach (IMob mob in Level.Mobs.Where(m => m != _playerMob && m.Hittable && m.HitCooldown <= 0f)) {
                    RectangleF mobRegion = mob.Body.Bounds;
                    mobRegion.X = mob.Body.Position.X - mobRegion.Width / 2f;
                    mobRegion.Y = mob.Body.Position.Y - mobRegion.Height / 2f;

                    if (region.Intersects(mobRegion)) {
                        mob.Health -= damage;
                        mob.HitCooldown = 0.15f;

                        if (_powerSwing && _playerData.Steal) {
                            _playerData.Lifespan += 3f;
                            _lifespanChanges.Add(new LifespanChange(3, _playerMob.Body.Position));
                            _game.Content.Load<SoundEffect>("Sounds/gain").Play();
                        }

                        _game.Content.Load<SoundEffect>("Sounds/hit").Play();

                        if (mob is ValgoxMob && mob.Health <= 0) {
                            ReplacedSelf?.Invoke(this, new ScreenEventArgs(new WinScreen(_game)));
                        }

                        var smash = new SmashMob(_random);
                        smash.Body.Position = _playerMob.Body.Position + (mob.Body.Position - _playerMob.Body.Position) / 2f;
                        Level.FutureMobs.Add(smash);
                    }
                }

                float speed = 8f * delta;
                if (_swingProgress < 1f - speed) {
                    _swingProgress += speed;
                }
                else {
                    _swingProgress = 1f;
                    _isSwinging = false;
                }
            }

            if (_shake > 0f) {
                _shake -= 10f * delta;
            }
            else {
                _shake = 0f;
            }

            if (_playerMob.StunTimer > 0f) {
                _playerMob.StunTimer -= delta;
            }

            if (_playerMob.StunTimer > 0f || _isDead) {
                if (_playerMob.Body.Contact.Y > 0f) {
                    float vel = _playerMob.Body.Velocity.X;
                    float slowdown = 8f * delta;
                    if (vel <= -slowdown) {
                        vel += slowdown;
                    }
                    else if (vel >= slowdown) {
                        vel -= slowdown;
                    }
                    else {
                        vel = 0f;
                    }
                    _playerMob.Body.Velocity = new Vector2(vel, _playerMob.Body.Velocity.Y);
                }
            }

            RectangleF playerRegion = _playerMob.Body.Bounds;
            playerRegion.X = _playerMob.Body.Position.X - _playerMob.Body.Bounds.Width / 2f;
            playerRegion.Y = _playerMob.Body.Position.Y - _playerMob.Body.Bounds.Height / 2f;
            foreach (Interactable interactable in Level.Interactables) {
                RectangleF region = interactable.Region;
                region.X = interactable.Position.X - region.Width / 2f;
                region.Y = interactable.Position.Y - region.Height / 2f;

                if (!region.Intersects(playerRegion)) {
                    _ignorables.Remove(interactable);
                }
            }

            if (_playerMob.Body.Contact.Y > 0f) {
                _playerMob.LenienceTimer = 0.1f;
            }
            else if (_playerMob.LenienceTimer > 0f) {
                _playerMob.LenienceTimer -= delta;
            }

            if (_movement.X != 0f) {
                _playerMob.Body.Position += _movement * _playerData.Speed * delta;
                _playerMob.Body.Velocity = new Vector2(0f, _playerMob.Body.Velocity.Y);
            }

            if (_movement.X != 0f && _playerMob.Body.Contact.Y > 0f) {
                if (_playerMob.Animation.Animation != _game.SpriteAnimations["player_walking"]) {
                    _playerMob.Animation = new AnimationState<Sprite>(_game.SpriteAnimations["player_walking"], 0.35f) {
                        IsLooping = true
                    };
                }
            }
            else {
                _playerMob.Animation = new AnimationState<Sprite>(_game.SpriteAnimations["player_idle"], 0.5f);
            }

            _movement = Vector2.Zero;

            foreach (IMob mob in Level.FutureMobs) {
                Level.Mobs.Add(mob);
            }
            Level.FutureMobs.Clear();

            var dead = new List<IMob>();
            foreach (IMob mob in Level.Mobs) {
                if (mob.Gravity) {
                    mob.Body.Velocity += new Vector2(0f, 30f) * delta;
                }

                BodyPhysics.Update(mob.Body, delta);

                TilePhysics.DoTileCollisions(mob.Body, Level);

                mob.Update(_game, Level, delta);

                if (mob.Animation != null) {
                    mob.Animation.Update(delta);
                    mob.Animation.Apply(mob.Sprite);
                }

                if (mob.HitCooldown > 0f) {
                    mob.HitCooldown -= delta;
                }

                if (mob.Dead || mob.Hittable && mob.Health <= 0) {
                    dead.Add(mob);
                }
            }
            foreach (IMob mob in dead) {
                Level.Mobs.Remove(mob);
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

            if (!_playerData.Prologue) {
                _playerData.Lifespan -= delta;
            }

            if (_message != null) {
                if (_displayMessage.Contents.Length < _message.Length) {
                    _messageTimer += delta;

                    while (_messageTimer >= 0.075f) {
                        _messageTimer -= 0.075f;

                        _messageChar++;
                        _displayMessage.Contents = _message.Substring(0, _messageChar);

                        if (_messageChar < _message.Length && char.IsLetterOrDigit(_message[_messageChar])) {
                            _game.Content.Load<SoundEffect>("Sounds/talk").Play();
                        }
                    }
                }
                else {
                    _ignorables.Clear();
                }
            }

            if (_messageStay > 0f) {
                _messageStay -= delta;
            }

            if (_messageStay <= 0f && Vector2.Distance(_playerMob.Body.Position, _messageSource) > 3f) {
                _message = null;
            }

            foreach (Interactable interactable in Level.Interactables) {
                if (interactable.Animation != null) {
                    interactable.Animation.Update(delta);
                    interactable.Animation.Apply(interactable.Sprite);
                }
            }

            if (_playerMob.CollisionCooldownTimer <= 0f) {
                foreach (IMob mob in Level.Mobs.Where(m => m != _playerMob)) {
                    RectangleF region = mob.Body.Bounds;
                    region.X = mob.Body.Position.X - region.Width / 2f;
                    region.Y = mob.Body.Position.Y - region.Height / 2f;

                    if (playerRegion.Intersects(region)) {
                        switch (mob.CollisionType) {
                            case CollisionType.Stun: {
                                _playerMob.StunTimer = 1f;
                                _playerMob.Body.Velocity = RandomUpVector() * 10f;
                                _playerMob.CollisionCooldownTimer = 0.5f;

                                for (int i = 0; i < 3; i++) {
                                    var star = new StarMob();
                                    star.Body.Position = _playerMob.Body.Position;
                                    star.Body.Velocity = _random.NextUnitVector() * 6f;
                                    Level.Mobs.Add(star);
                                }

                                _shake += 3f;

                                _game.Content.Load<SoundEffect>("Sounds/hurt").Play();
                                break;
                            }
                            case CollisionType.StunPlus: {
                                _playerMob.StunTimer = 1.25f;
                                _playerMob.Body.Velocity = RandomUpVector() * 11f;
                                _playerMob.CollisionCooldownTimer = 0.25f;

                                for (int i = 0; i < 3; i++) {
                                    var star = new StarMob();
                                    star.Body.Position = _playerMob.Body.Position;
                                    star.Body.Velocity = _random.NextUnitVector() * 6f;
                                    Level.Mobs.Add(star);
                                }

                                _shake += 4f;

                                _game.Content.Load<SoundEffect>("Sounds/hurt").Play();
                                break;
                            }
                            case CollisionType.SpikeKill: {
                                Die();
                                break;
                            }
                            case CollisionType.Gem: {
                                _playerData.Lifespan += 3;
                                _lifespanChanges.Add(new LifespanChange(3, _playerMob.Body.Position));
                                _game.Content.Load<SoundEffect>("Sounds/gain").Play();
                                mob.Dead = true;

                                _game.Content.Load<SoundEffect>("Sounds/pickup").Play();
                                break;
                            }
                            case CollisionType.Absorb: {
                                _playerMob.StunTimer = 0.5f;
                                _playerMob.CollisionCooldownTimer = 0.25f;

                                for (int i = 0; i < 3; i++) {
                                    var star = new StarMob();
                                    star.Body.Position = _playerMob.Body.Position;
                                    star.Body.Velocity = _random.NextUnitVector() * 6f;
                                    Level.Mobs.Add(star);
                                }

                                _shake += 4f;

                                _playerData.Lifespan -= 5f;
                                _lifespanChanges.Add(new LifespanChange(-5, _playerMob.Body.Position));
                                _game.Content.Load<SoundEffect>("Sounds/lose").Play();

                                _game.Content.Load<SoundEffect>("Sounds/hurt").Play();
                                break;
                            }
                            case CollisionType.Curse: {
                                if (!_playerData.Prologue) {
                                    continue;
                                }

                                _showValgox = true;
                                _valgoxStart = new Vector2(10f, -12f);
                                _valgoxEnd = new Vector2(-10f, -12f);
                                _valgoxTimer = 0f;
                                _valgoxDuration = 2f;
                                break;
                            }
                            default: {
                                continue;
                            }
                        }
                        break;
                    }
                }
            }
            else {
                _playerMob.CollisionCooldownTimer -= delta;
            }

            if (!_isDead) {
                if (_playerData.Lifespan < 0f || _playerMob.Body.Position.Y > Level.Height) {
                    Die();
                }
            }

            if (_transitioning) {
                float speed = delta * 3f;
                if (_transitionStart < _transitionEnd && _transitionCurrent <= _transitionEnd - speed) {
                    _transitionCurrent += speed;
                }
                else if (_transitionStart > _transitionEnd && _transitionCurrent >= _transitionEnd + speed) {
                    _transitionCurrent -= speed;
                }
                else {
                    _transitionCurrent = _transitionEnd;

                    if (_isDead) {
                        if (_playerData.Prologue) {
                            ReplacedSelf?.Invoke(this, new ScreenEventArgs(new GameScreen(_game, _game.EntranceTemplate, new PlayerData())));

                            _music?.Stop();
                        }
                        else {
                            ReplacedSelf?.Invoke(this, new ScreenEventArgs(new RestartScreen(_game)));

                            _music?.Stop();
                        }
                    }
                    else if (_destination != null) {
                        ReplacedSelf?.Invoke(this, new ScreenEventArgs(new GameScreen(_game, _destination, _playerData)));

                        _music?.Stop();
                    }

                    _transitioning = false;
                }
            }

            for (int y = 0; y < Level.Height; y++) {
                for (int x = 0; x < Level.Width; x++) {
                    Tile tile = Level.GetTile(x, y);

                    if (tile.FrontAnimation != null) {
                        tile.FrontAnimation.Update(delta);
                        tile.FrontAnimation.Apply(tile.FrontSprite);
                    }
                }
            }
        }

        public void Draw(Renderer renderer) {
            Vector2 oriPos = _camera.Position;
            _camera.Position += _random.NextUnitVector() * _shake;
            _worldSettings.TransformMatrix = _camera.GetTransformMatrix();

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

            _swordSprite.Texture = _playerData.Sword;
            if (_isCharging) {
                float p = _swordCharge;
                p = 1f - (float)Math.Pow(1f - p, 2f);

                float startAngle = 0f;
                float targetAngle = (float)-Math.PI / 2f;

                if (!_playerMob.FacingRight) {
                    startAngle = targetAngle;
                    targetAngle = 0f;
                }

                float angle = startAngle * (1f - p) + targetAngle * p;
                _swordSprite.Rotation = angle;

                renderer.Draw(_swordSprite, _playerMob.Body.Position * GameProperties.TileSize);
            }
            if (_isSwinging) {
                float p = _swingProgress;
                p = 1f - (float)Math.Pow(1f - p, 2f);

                float startAngle = _swingStart;
                float targetAngle = (float)Math.PI * 3f / 5f;

                if (!_playerMob.FacingRight) {
                    float x = MathHelper.Pi - targetAngle;
                    targetAngle = -x - targetAngle;
                }

                float angle = startAngle * (1f - p) + targetAngle * p;
                _swordSprite.Rotation = angle;

                renderer.Draw(_swordSprite, _playerMob.Body.Position * GameProperties.TileSize);
            }

            if (_showValgox) {
                float vp = _valgoxTimer / _valgoxDuration;
                Vector2 vpos = _valgoxStart * (1f - vp) + _valgoxEnd * vp + new Vector2(0f, 11f * (float)Math.Sin((float)Math.PI * vp));
                renderer.Draw(_valgoxSprite, (_playerMob.Body.Position + vpos) * GameProperties.TileSize);
            }

            foreach (LifespanChange change in _lifespanChanges) {
                renderer.Draw(change.Text, change.Position * GameProperties.TileSize);
            }

            bool canInteract = false;

            RectangleF playerRegion = _playerMob.Body.Bounds;
            playerRegion.X = _playerMob.Body.Position.X - _playerMob.Body.Bounds.Width / 2f;
            playerRegion.Y = _playerMob.Body.Position.Y - _playerMob.Body.Bounds.Height / 2f;
            foreach (Interactable interactable in Level.Interactables.Where(i => !_ignorables.Contains(i))) {
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

            for (int i = 0; i < _playerData.Armor; i++) {
                float hOffset = -(_playerData.Armor - 1f) / 2f + i;

                renderer.Draw(new Sprite("armor") {
                    Origin = new Vector2(0.5f)
                }, new Vector2(GameProperties.Width / 2f + hOffset * 16f, 26f));
            }

            if (_shop != null) {
                renderer.Draw(new Sprite("pixel") {
                    Color = Color.Black * 0.75f,
                    Scale = new Vector2(GameProperties.Width, GameProperties.Height)
                }, Vector2.Zero);

                var pos = new Vector2();
                for (int i = 0; i < 3; i++) {
                    pos = new Vector2(GameProperties.Width / 2f, 48f + 48f * i);

                    if (_shopSelection == i) {
                        renderer.Draw(new Sprite("pixel") {
                            Color = Color.White,
                            Scale = new Vector2(258f, 34f),
                            Origin = new Vector2(0.5f, 0f)
                        }, pos - new Vector2(0f, 1f));
                    }

                    renderer.Draw(new Sprite("pixel") {
                        Color = Color.Black,
                        Scale = new Vector2(256f, 32f),
                        Origin = new Vector2(0.5f, 0f)
                    }, pos);

                    string text = "OUT OF STOCK";
                    Color color = Color.Gray;
                    if (_shop[i] != null) {
                        Item item = _shop[i];
                        color = Color.White;

                        int cost = item.Cost;
                        if (_playerData.HasCoupon) {
                            cost = Math.Max(cost / 2, 1);
                            color = Color.Yellow;
                        }

                        text = item.Name + " - " + cost + " SECONDS";
                    }
                    renderer.Draw(new Text("normal", text) {
                        Color = color,
                        Origin = new Vector2(0.5f)
                    }, pos + new Vector2(0f, 16f));
                }

                pos += new Vector2(0f, 48f);

                if (_shopSelection == 3) {
                    renderer.Draw(new Sprite("pixel") {
                        Color = Color.White,
                        Scale = new Vector2(130f, 18f),
                        Origin = new Vector2(0.5f, 0f)
                    }, pos - new Vector2(0f, 1f));
                }

                renderer.Draw(new Sprite("pixel") {
                    Color = Color.Black,
                    Scale = new Vector2(128f, 16f),
                    Origin = new Vector2(0.5f, 0f)
                }, pos);

                renderer.Draw(new Text("normal", "CANCEL") {
                    Origin = new Vector2(0.5f)
                }, pos + new Vector2(0f, 8f));
            }

            if (_isDead) {
                renderer.Draw(new Text("normal", "RIP") { Color = Color.White, Origin = new Vector2(0.5f, 0.5f) }, new Vector2(GameProperties.Width / 2f, 12f));
            }
            else if (_playerData.Prologue) {
                renderer.Draw(new Text("normal", "60 YEARS") { Color = Color.White, Origin = new Vector2(0.5f, 0.5f) }, new Vector2(GameProperties.Width / 2f, 12f));
            }
            else {
                int seconds = (int)Math.Ceiling(_playerData.Lifespan);
                string timerText = seconds + " SECONDS";
                if (seconds == 1) {
                    timerText = "1 SECOND ";
                }
                renderer.Draw(new Text("normal", timerText) { Color = Color.White, Origin = new Vector2(1f, 0.5f) }, new Vector2(GameProperties.Width / 2f + 39f, 12f));
            }

            if (_transitioning) {
                float y = -16f + _transitionCurrent * (GameProperties.Width + 16f);
                renderer.Draw(_transition, new Vector2(0f, y));
            }

            renderer.End();

            _camera.Position = oriPos;
        }

        private void Die() {
            _playerMob.Body.Velocity = RandomUpVector() * 10f;

            if (_playerData.Armor > 0 && _playerData.Lifespan > 0f) {
                _playerData.Armor--;

                _playerMob.CollisionCooldownTimer = 1f;

                _shake += 5f;

                _game.Content.Load<SoundEffect>("Sounds/hurt").Play();
            }
            else {
                _playerMob.CollisionCooldownTimer = 100000f;

                _playerMob.Animation = new AnimationState<Sprite>(_game.SpriteAnimations["player_dead"], 1f);

                _isDead = true;

                _shake += 5f;

                _transitioning = true;
                _transitionStart = -1f;
                _transitionEnd = 0f;
                _transitionCurrent = -1f;

                _game.Content.Load<SoundEffect>("Sounds/death").Play();
            }
        }

        private Vector2 RandomUpVector() {
            float angle = (float)(_random.NextDouble() * Math.PI / 2f + Math.PI * 5f / 4f);

            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }
    }
}
