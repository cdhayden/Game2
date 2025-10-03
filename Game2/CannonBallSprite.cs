using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Game2.Collision;

namespace Game2
{
    public class CannonBallSprite
    {
        private ContentManager content;

        private Texture2D texture;

        private CollisionCircle _bounds;

        public CollisionCircle Bounds => _bounds;

        public Vector2 Position { get; private set; }

        public Vector2 Center => Position + new Vector2(17, 17);
        public Vector2 Velocity { get; private set; }

        public Vector2 acceleration = new Vector2(0, 400f);


        public CannonBallSprite(Vector2 position, Vector2 velocity)
        {
            Position = position;
            Velocity = velocity;
            _bounds = new CollisionCircle(Position + new Vector2(17, 17), 17);
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Velocity += acceleration * dt;
            Position += Velocity * dt;

            _bounds = new CollisionCircle(Position + new Vector2(17, 17), 17);
        }

        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("ball");
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, 0, new Vector2(17, 17), 1f, SpriteEffects.None, 0);
        }
    }
}
