using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace KeyBattery;

/// <summary>Draws the tray icon: the battery percentage on a coloured tile.</summary>
static class TrayIconRenderer
{
    static readonly Color Healthy = Color.FromArgb(46, 160, 67);
    static readonly Color Warning = Color.FromArgb(219, 140, 20);
    static readonly Color Critical = Color.FromArgb(207, 34, 46);
    static readonly Color Charging = Color.FromArgb(31, 111, 235);
    static readonly Color Unknown = Color.FromArgb(110, 118, 129);

    /// <param name="percentage">Null when there is no reading yet.</param>
    /// <param name="stale">True when the last reading is old, drawn in grey.</param>
    public static Icon Render(int? percentage, bool charging, bool stale, AppSettings settings)
    {
        var size = SystemInformation.SmallIconSize;
        using var bitmap = new Bitmap(size.Width, size.Height);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var colour = percentage is not { } pct || stale ? Unknown
                : charging ? Charging
                : pct <= settings.CriticalAt ? Critical
                : pct <= settings.WarnAt ? Warning
                : Healthy;

            using var fill = new SolidBrush(colour);
            int radius = Math.Max(2, size.Width / 5);
            g.FillRoundedRectangle(fill, new Rectangle(0, 0, size.Width - 1, size.Height - 1), new Size(radius, radius));

            using var text = TextPath(percentage?.ToString() ?? "?", size);
            g.FillPath(Brushes.White, text);
        }

        IntPtr handle = bitmap.GetHicon();
        try
        {
            return (Icon)Icon.FromHandle(handle).Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    /// <summary>
    /// The text as an outline scaled to fit the tile and centred on it.
    /// Scaling the glyph outlines, rather than picking a font size, means
    /// "100" can never wrap or clip at any icon size.
    /// </summary>
    static GraphicsPath TextPath(string text, Size size)
    {
        var path = new GraphicsPath();
        using var family = new FontFamily("Segoe UI");
        path.AddString(text, family, (int)FontStyle.Bold, 100f, PointF.Empty, StringFormat.GenericTypographic);

        var bounds = path.GetBounds();
        float maxWidth = size.Width - 2f;
        float maxHeight = size.Height * 0.62f;
        float scale = Math.Min(maxWidth / bounds.Width, maxHeight / bounds.Height);

        using var transform = new Matrix();
        transform.Translate(size.Width / 2f, size.Height / 2f);
        transform.Scale(scale, scale);
        transform.Translate(-(bounds.X + bounds.Width / 2f), -(bounds.Y + bounds.Height / 2f));
        path.Transform(transform);
        return path;
    }

    [DllImport("user32.dll")]
    static extern bool DestroyIcon(IntPtr handle);
}
