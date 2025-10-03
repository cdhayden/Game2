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
    public class BoomSprite
    {
        private ContentManager content;

        private Texture2D texture;
        private int frame;
        private float timer;

        public bool Done { get; private set; }

        public Vector2 Position { get; private set; }



        public BoomSprite(Vector2 position)
        {
            Position = position;
            Done = false;
            timer = 0;
            frame = 0;
        }

        public void Update(GameTime gameTime)
        {
            if (!Done) 
            {
                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
                timer += dt;
                if (timer > 0.1)
                {
                    frame++;
                    timer -= 0.1f;
                }
                if (frame > 11) Done = true;
            }
        }

        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("Explosion");
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if(!Done)
                spriteBatch.Draw(texture, Position, new Rectangle(96*frame, 0, 96, 96), Color.White, 0, new Vector2(17, 17), 1.5f, SpriteEffects.None, 0);
        }
    }
}
