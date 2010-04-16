using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Net.Brotherus
{
    public class SpriteFonts
    {
        private SpriteFont _smallFont;
        private SpriteFont _bardTitle;

        public SpriteFonts(ContentManager contentManager)
        {
            _smallFont = contentManager.Load<SpriteFont>("Content/SmallFont");
            _bardTitle = contentManager.Load<SpriteFont>("Content/BardTitle");
            //_menuSpriteFont = contentManager.Load<SpriteFont>("Content/Fonts/menuFont");
            //_frameRateCounterFont = contentManager.Load<SpriteFont>("Content/Fonts/frameRateCounterFont");
            //_gameSpriteFont = contentManager.Load<SpriteFont>("Content/Fonts/gamefont");
            //_detailsFont = contentManager.Load<SpriteFont>("Content/Fonts/detailsFont");
        }

        public SpriteFont SmallFont { get { return _smallFont; } }

        public SpriteFont BardTitle { get { return _bardTitle; } }
    }
}