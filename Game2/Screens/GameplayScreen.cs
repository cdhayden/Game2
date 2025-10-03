using Game2;
using GameArchitectureExample.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GameArchitectureExample.Screens
{
    // This screen implements the actual game logic. It is just a
    // placeholder to get the idea across: you'll probably want to
    // put some more interesting gameplay in here!
    public class GameplayScreen : GameScreen
    {
        private ContentManager _content;
        private SpriteFont _gameFont;

        private int _oceanFrame = 0;
        private float _oceantimer = 0;
        private const float _OCEAN_THRESHOLD = 0.1f;
        private Texture2D[] _oceanBackground = new Texture2D[21];

        private Texture2D _dockBackground;
        private Rectangle _dockSource = new Rectangle(32, 33, 64, 64);


        private float _cannonRotation = 0;
        private bool _shotFired = false;
        private Texture2D _cannonBase;
        private Texture2D _cannon;
        private Vector2 _cannonPosition;
        private Vector2 _cannonOrigin = new Vector2(25, 34);

        private CannonBallSprite _cannonBall;

        private List<BoomSprite> _booms;

        private List<ShipSprite> _ships;
        private float _shipSpawnTimer = 0f;
        private const float _SHIP_SPAWN_THRESHOLD = 2f;
        private float _shipSortTimer = 0f;
        private float _shipSortThreshold = 0.5f;

        private Texture2D _tint;

        private Viewport _viewport;

        private readonly Random _random = new Random();

        private float _pauseAlpha;
        private readonly InputAction _pauseAction;
        private readonly InputAction _cannonShotAction;

        private SoundEffect _shipDestroyed;

        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            _pauseAction = new InputAction(
                new Buttons[0],
                new[] { Keys.Back, Keys.Escape }, true);

            _cannonShotAction = new InputAction(
                new Buttons[0],
                new[] { Keys.Space, }, true);
        }

        // Load graphics content for the game
        public override void Activate()
        {
            if (_content == null)
                _content = new ContentManager(ScreenManager.Game.Services, "Content");

            _gameFont = _content.Load<SpriteFont>("gamefont");

            _cannonPosition = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height - 50);

            for (int i = 0; i < _oceanBackground.Length; i++) 
            { 
                string fileName = string.Format("WaterAnimations/WaterAnimations/ocean{0:00}", i+1);
                _oceanBackground[i] = _content.Load<Texture2D>(fileName);
            }

            _dockBackground = _content.Load<Texture2D>("Artis_dock");
            
            _cannon = _content.Load<Texture2D>("cannon"); //150 x 67
            _cannonBase = _content.Load<Texture2D>("cannon2"); //86 x 68

            _tint = _content.Load<Texture2D>("blank"); //86 x 68

            _shipDestroyed = _content.Load<SoundEffect>("Hitting Wall");

            _cannonRotation = -MathHelper.PiOver2;
            _ships = new List<ShipSprite>();
            _booms = new List<BoomSprite>();


            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            Thread.Sleep(1000);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }


        public override void Deactivate()
        {
            base.Deactivate();
            MediaPlayer.Pause();
        }

        public override void Unload()
        {
            _content.Unload();
        }

        // This method checks the GameScreen.IsActive property, so the game will
        // stop updating when the pause menu is active, or if you tab away to a different application.
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            _oceantimer += dt;
            if (_oceantimer >= _OCEAN_THRESHOLD) 
            {
                _oceanFrame = (_oceanFrame + 1) % _oceanBackground.Length;
                _oceantimer -= _OCEAN_THRESHOLD;
            }

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                _pauseAlpha = Math.Min(_pauseAlpha + 1f / 32, 1);
            else
                _pauseAlpha = Math.Max(_pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                foreach (BoomSprite b in _booms) b.Update(gameTime);
                if (_booms.Count > 0)
                    _booms.RemoveAll((BoomSprite item) => item.Done);

                foreach (ShipSprite s in _ships) s.Update(gameTime);
                _shipSortTimer += dt;
                _shipSpawnTimer += dt;

                if (_shipSpawnTimer >= _SHIP_SPAWN_THRESHOLD) 
                {
                    _shipSpawnTimer -= _SHIP_SPAWN_THRESHOLD;
                    float velX = ((float)_random.NextDouble() - 0.5f);
                    if(velX > 0 && velX < 0.2f) velX = 0.2f;
                    if(velX <= 0 && velX > -0.2f) velX = -0.2f;
                    Vector2 vel = new(velX * 500, ((float)_random.NextDouble() - 0.5f) * 30);
                    float posX = vel.X > 0 ? viewport.X : viewport.Width + viewport.X;
                    Vector2 pos = new(posX, (float)_random.NextDouble() * (viewport.Height - 356) + viewport.Y);
                    ShipSprite newShip = new ShipSprite(pos, vel);
                    newShip.LoadContent(_content);
                    _ships.Add(newShip);
                    _ships.Sort((a, b) => a.Position.Y.CompareTo(b.Position.Y));
                    _shipSortTimer -= _shipSortThreshold;
                }
                else if (_shipSortTimer >= _shipSortThreshold)
                {
                    _shipSortTimer -= _shipSortThreshold;
                    _ships.Sort((a, b) => a.Position.Y.CompareTo(b.Position.Y));
                }

                if (_shotFired)
                {
                    ShipSprite removed = null;
                    foreach (ShipSprite s in _ships)
                    {
                        if (_cannonBall.Bounds.CollidesWith(s.Bounds))
                        {
                            _shotFired = false;
                            removed = s;
                            Vector2 diff = (s.Center - _cannonBall.Center) / 2;
                            BoomSprite boom = new BoomSprite(_cannonBall.Center + diff - new Vector2(72, 36));
                            boom.LoadContent(_content);
                            _booms.Add(boom);
                            _shipDestroyed.Play();
                            break;
                        }
                    }
                    _cannonBall.Update(gameTime);
                    if (_cannonBall.Position.X < viewport.X || _cannonBall.Position.X > viewport.X + viewport.Width || _cannonBall.Position.Y < viewport.Y || _cannonBall.Position.Y >= viewport.Y + viewport.Height)
                    {
                        _cannonBall = null;
                        _shotFired = false;
                    }
                    
                    if (removed != null) 
                    {
                        _cannonBall = null;
                        _ships.Remove(removed);
                    }
                }
            }
        }

        // Unlike the Update method, this will only be called when the gameplay screen is active.
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            var keyboardState = input.CurrentKeyboardStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!

            PlayerIndex player;
            if (_pauseAction.Occurred(input, ControllingPlayer, out player))
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {
                if (keyboardState.IsKeyDown(Keys.Left) && _cannonRotation > -1 - MathHelper.PiOver2)
                    _cannonRotation-= 0.01f;

                if (keyboardState.IsKeyDown(Keys.Right) && _cannonRotation < 1 - MathHelper.PiOver2)
                    _cannonRotation+= 0.01f;

                if(!_shotFired && _cannonShotAction.Occurred(input, ControllingPlayer, out player)) 
                {
                    _shotFired = true;
                    Vector2 direction = Vector2.Normalize(new Vector2((float)Math.Cos(_cannonRotation), (float)Math.Sin(_cannonRotation)));
                    _cannonBall = new CannonBallSprite(_cannonPosition + _cannonOrigin - new Vector2(26,26) + direction*120, direction * 600); //35 x 35
                    _cannonBall.LoadContent(_content);
                    
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.CornflowerBlue, 0, 0);

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            // Our player and enemy are both actually just text strings.
            var spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            spriteBatch.Draw(_oceanBackground[_oceanFrame], new Rectangle(viewport.X, viewport.Y, viewport.Width, viewport.Height), Color.LightCyan);
            int dockX = 0;
            while (dockX <= viewport.Width) 
            {
                spriteBatch.Draw(_dockBackground, new Vector2(dockX, viewport.Y + viewport.Height-120), _dockSource, Color.SandyBrown, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
                dockX += 128;
            }
            spriteBatch.Draw(_cannon, _cannonPosition, null, Color.White, _cannonRotation, _cannonOrigin, 0.8f, SpriteEffects.None, 0);
            spriteBatch.Draw(_cannonBase, _cannonPosition - new Vector2(35,20), null, Color.White, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0);
            if (_shotFired) _cannonBall.Draw(gameTime, spriteBatch);
            foreach (ShipSprite s in _ships) s.Draw(gameTime, spriteBatch);
            foreach (BoomSprite b in _booms) b.Draw(gameTime, spriteBatch);
            spriteBatch.Draw(_tint, viewport.Bounds, Color.Black * 0.3f);


            //spritebatch.Draw(_tint, new Rectangle((int)pos.X - 20, (int)pos.Y - 20, (int)len.X + 40, (int)len.Y + 40), outline);
            string instruction = "\'BACKSPACE\'  to  Pause";
            Vector2 len = _gameFont.MeasureString(instruction);
            Vector2 pos = new Vector2(viewport.X + 60, viewport.Y + viewport.Height - 55);
            spriteBatch.Draw(_tint, new Rectangle((int)pos.X - 10, (int)pos.Y - 10, (int)len.X + 20, (int)len.Y + 20), Color.Black * 0.3f);
            spriteBatch.DrawString(_gameFont, instruction, pos, Color.LightGray);

            instruction = "Arrow  Keys  to  Rotate";
            len = _gameFont.MeasureString(instruction);
            pos = new Vector2(viewport.X + viewport.Width - 280, viewport.Y + viewport.Height - 70);
            spriteBatch.Draw(_tint, new Rectangle((int)pos.X - 10, (int)pos.Y - 10, (int)len.X + 20, (int)len.Y + 50), Color.Black * 0.3f);
            spriteBatch.DrawString(_gameFont, instruction, new Vector2(viewport.X + viewport.Width - 280, viewport.Y + viewport.Height - 40), Color.LightGray);

            instruction = "       'SPACE'  to  Shoot";
            pos = new Vector2(viewport.X + viewport.Width - 280, viewport.Y + viewport.Height - 70);
            spriteBatch.DrawString(_gameFont, instruction, pos, Color.LightGray);

            spriteBatch.End();
            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || _pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, _pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha, Color.Black);
            }
        }
    }
}
