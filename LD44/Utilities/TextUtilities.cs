using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace LD44.Utilities {
    public static class TextUtilities {
        // Taken from: https://stackoverflow.com/questions/15986473/how-do-i-implement-word-wrap
        public static string WrapText(SpriteFont font, string text, float maxLineWidth) {
            string[] words = text.Split(' ');
            var sb = new StringBuilder();
            float lineWidth = 0f;
            float spaceWidth = font.MeasureString(" ").X;

            foreach (string word in words) {
                Vector2 size = font.MeasureString(word);

                if (lineWidth + size.X < maxLineWidth) {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else {
                    sb.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }

            return sb.ToString().Trim();
        }
    }
}
