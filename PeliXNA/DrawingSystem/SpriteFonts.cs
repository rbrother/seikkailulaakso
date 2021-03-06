﻿using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Net.Brotherus
{
    public class SpriteFonts
    {
        private SpriteFont _smallFont;
        private SpriteFont _bardTitle;

        public SpriteFonts(ContentManager contentManager)
        {
            _smallFont = contentManager.Load<SpriteFont>("SeikkailuLaaksoContent/SmallFont");
            _bardTitle = contentManager.Load<SpriteFont>("SeikkailuLaaksoContent/BardTitle");
        }

        public SpriteFont SmallFont { get { return _smallFont; } }

        public SpriteFont BardTitle { get { return _bardTitle; } }
    }
}