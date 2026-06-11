namespace Configurations.Model.Types
{
    /// <summary>
    /// Unity-independent RGBA color stored as four bytes.
    /// </summary>
    [System.Serializable]
    public readonly struct ColorData : System.IEquatable<ColorData>
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly byte A;

        public static readonly ColorData White = new(255, 255, 255);
        public static readonly ColorData Gray = new(128, 128, 128);
        public static readonly ColorData Black = new(0, 0, 0);
        public static readonly ColorData Clear = new(0, 0, 0, 0);

        public static readonly ColorData Blue = new(0, 0, 255);
        public static readonly ColorData Red = new(255, 0, 0);
        public static readonly ColorData Green = new(0, 255, 0);
        public static readonly ColorData Yellow = new(255, 255, 0);
        public static readonly ColorData Cyan = new(0, 255, 255);
        public static readonly ColorData Magenta = new(255, 0, 255);

        public ColorData(byte r, byte g, byte b, byte a = 255)
        {
            R = r; G = g; B = b; A = a;
        }

        /// <summary>
        /// Parses "RRGGBB" or "RRGGBBAA" (leading '#' is ignored).
        /// Returns White on any parse error.
        /// </summary>
        public static ColorData FromHex(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return White;

            if (hex[0] == '#')
                hex = hex.Substring(1);

            try
            {
                if (hex.Length == 6)
                    return new ColorData(
                        System.Convert.ToByte(hex.Substring(0, 2), 16),
                        System.Convert.ToByte(hex.Substring(2, 2), 16),
                        System.Convert.ToByte(hex.Substring(4, 2), 16));

                if (hex.Length == 8)
                    return new ColorData(
                        System.Convert.ToByte(hex.Substring(0, 2), 16),
                        System.Convert.ToByte(hex.Substring(2, 2), 16),
                        System.Convert.ToByte(hex.Substring(4, 2), 16),
                        System.Convert.ToByte(hex.Substring(6, 2), 16));
            }
            catch { }

            return White;
        }

        // ── Formatting ────────────────────────────────────────────────────────

        /// <summary>"RRGGBB" when fully opaque; "RRGGBBAA" otherwise.</summary>
        public override string ToString() =>
            A == 255
                ? $"{R:X2}{G:X2}{B:X2}"
                : $"{R:X2}{G:X2}{B:X2}{A:X2}";

        // ── Equality ──────────────────────────────────────────────────────────

        public bool Equals(ColorData other) =>
            R == other.R && G == other.G && B == other.B && A == other.A;

        public override bool Equals(object obj) => obj is ColorData c && Equals(c);

        public override int GetHashCode() => (R << 24) | (G << 16) | (B << 8) | A;

        public static bool operator ==(ColorData a, ColorData b) => a.Equals(b);
        public static bool operator !=(ColorData a, ColorData b) => !a.Equals(b);

        // ── Unity casts (editor + player; absent on non-Unity server) ─────────

#if UNITY_5_3_OR_NEWER
        public static implicit operator UnityEngine.Color(ColorData c) =>
            new(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);

        public static explicit operator ColorData(UnityEngine.Color c) =>
            new(
                (byte)(c.r * 255f + 0.5f),
                (byte)(c.g * 255f + 0.5f),
                (byte)(c.b * 255f + 0.5f),
                (byte)(c.a * 255f + 0.5f));

        public static implicit operator UnityEngine.Color32(ColorData c) =>
            new(c.R, c.G, c.B, c.A);

        public static explicit operator ColorData(UnityEngine.Color32 c) =>
            new(c.r, c.g, c.b, c.a);
#endif
    }
}
