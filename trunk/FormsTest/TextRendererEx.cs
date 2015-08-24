using System.Drawing;
using System.Windows.Forms;

namespace MailClient.Common.UI
{
    public static class TextRendererEx
    {
        private const int MAX_TEXT_LENGTH = 1024;

        public static void DrawText(IDeviceContext dc, string text, Font font, Point pt, Color foreColor)
        {
            TextRenderer.DrawText(dc, (text != null && text.Length > MAX_TEXT_LENGTH) ? text.Substring(0, MAX_TEXT_LENGTH) : text, font, pt, foreColor);
        }
        public static void DrawText(IDeviceContext dc, string text, Font font, Rectangle bounds, Color foreColor)
        {
            TextRenderer.DrawText(dc, (text != null && text.Length > MAX_TEXT_LENGTH) ? text.Substring(0, MAX_TEXT_LENGTH) : text, font, bounds, foreColor);
        }
        public static void DrawText(IDeviceContext dc, string text, Font font, Point pt, Color foreColor, Color backColor)
        {
            TextRenderer.DrawText(dc, (text != null && text.Length > MAX_TEXT_LENGTH) ? text.Substring(0, MAX_TEXT_LENGTH) : text, font, pt, foreColor, backColor);
        }
        public static void DrawText(IDeviceContext dc, string text, Font font, Point pt, Color foreColor, TextFormatFlags flags)
        {
            TextRenderer.DrawText(dc, (text != null && text.Length > MAX_TEXT_LENGTH) ? text.Substring(0, MAX_TEXT_LENGTH) : text, font, pt, foreColor, flags);
        }
        public static void DrawText(IDeviceContext dc, string text, Font font, Rectangle bounds, Color foreColor, Color backColor)
        {
            TextRenderer.DrawText(dc, (text != null && text.Length > MAX_TEXT_LENGTH) ? text.Substring(0, MAX_TEXT_LENGTH) : text, font, bounds, foreColor, backColor);
        }
        public static void DrawText(IDeviceContext dc, string text, Font font, Rectangle bounds, Color foreColor, TextFormatFlags flags)
        {
            TextRenderer.DrawText(dc, (text != null && text.Length > MAX_TEXT_LENGTH) ? text.Substring(0, MAX_TEXT_LENGTH) : text, font, bounds, foreColor, flags);
        }
        public static void DrawText(IDeviceContext dc, string text, Font font, Point pt, Color foreColor, Color backColor, TextFormatFlags flags)
        {
            TextRenderer.DrawText(dc, (text != null && text.Length > MAX_TEXT_LENGTH) ? text.Substring(0, MAX_TEXT_LENGTH) : text, font, pt, foreColor, backColor, flags);
        }
        public static void DrawText(IDeviceContext dc, string text, Font font, Rectangle bounds, Color foreColor, Color backColor, TextFormatFlags flags)
        {
            TextRenderer.DrawText(dc, (text != null && text.Length > MAX_TEXT_LENGTH) ? text.Substring(0, MAX_TEXT_LENGTH) : text, font, bounds, foreColor, backColor, flags);
        }

        public static Size MeasureText(string text, Font font, Size size, TextFormatFlags flags)
        {
            return TextRenderer.MeasureText((text != null && text.Length > MAX_TEXT_LENGTH) ? text.Substring(0, MAX_TEXT_LENGTH) : text, font, size, flags);
        }
    }
}
