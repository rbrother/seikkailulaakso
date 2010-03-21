using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vector2Fs = FarseerGames.FarseerPhysics.Mathematics.Vector2;
using Vector2Xna = Microsoft.Xna.Framework.Vector2;

namespace Net.Brotherus {
    public static class VectorExtensions {
        public static Vector2Fs ToVector2Fs(this Vector2Xna v) {
            return new Vector2Fs(v.X, v.Y);
        }
        public static Vector2Xna ToVector2Xna(this Vector2Fs v) {
            return new Vector2Xna(v.X, v.Y);
        }
        public static Vector2Xna ToVector2Xna(this Vector2Fs v, float xOffset) {
            return new Vector2Xna(v.X + xOffset, v.Y);
        }
    }
}
