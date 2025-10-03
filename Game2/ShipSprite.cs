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
    public class ShipSprite
    {
        private ContentManager content;

        private CollisionRectangle _bounds;

        public CollisionRectangle Bounds => _bounds;

        public Vector2 Center => new Vector2(Bounds.X + Bounds.Width/2, Bounds.Y + Bounds.Height / 2);

        private SpriteEffects flipped;

        private Texture2D texture;

        private float scale = 0.2f;

        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }

        public ShipSprite(Vector2 position, Vector2 velocity)
        {
            Position = position;
            Velocity = velocity;
            if(velocity.X > 0) flipped = SpriteEffects.FlipHorizontally;
            else flipped = SpriteEffects.None;
            _bounds = new CollisionRectangle(position.X, position.Y + 512 * scale * 0.25f, 512 * scale, 512 * scale * 0.75f);
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Position += Velocity * dt;
            _bounds = new CollisionRectangle(Position.X, Position.Y + 438 * scale * 0.25f, 512 * scale, 438 * scale * 0.75f);
        }

        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("ship"); //512 x 512
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, new Rectangle(0,0,512,438), Color.White, 0, Vector2.Zero, 0.2f, flipped, scale);
        }
    }
}
