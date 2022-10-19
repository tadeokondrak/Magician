/*
*  Class for storing and manipulating colour data, with RGBA and HSLA support
*/


namespace Magician
{
    public abstract class Color
    {
        /*
        * Format:
        * r [0, 255]
        * g [0, 255]
        * b [0, 255]
        * h [0, 2Pi)
        * s [0, 1]
        * l [0, 1]
        * a [0, 255]
        */
        
        protected double a;
        public abstract double R {get; set;}
        public abstract double G {get; set;}
        public abstract double B {get; set;}
        public abstract double H {get; set;}
        public abstract double S {get; set;}
        public abstract double L {get; set;}
        public abstract Color Copy();
        public double A {get => a % 256; set => a = value; }
        public uint Hex()
        {
            return (uint)((byte)R << 24) + (uint)((byte)G << 16) + (uint)((byte)B << 8) + (uint)(A);
        }

        // Calculate a colour's hue angle from RGB values
        protected static double HueFromRGB(double r, double g, double b)
        {
            double h;
            r /= 255f;
            g /= 255f;
            b /= 255f;
            double colMax = Math.Max(r, Math.Max(g, b));
            double colMin = Math.Min(r, Math.Min(g, b));

            if (colMax == r)
            {
                h = (g - b) / (colMax - colMin);
            }
            else if (colMax == g)
            {
                h = 2f + (b - r) / (colMax - colMin);
            }
            else if (colMax == b)
            {
                h = 4f + (r - g) / (colMax - colMin);
            }
            else
            {
                throw new InvalidDataException($"Could not get hue from rgb: {r} {g} {b}");
            }
            
            // Convert from 0-1 to 0-2Pi
            h *= Math.PI / 3;
            if (h < 0)
            {
                h += 2*Math.PI;
            }
            return h;
        }

        protected static double SaturationFromRGB(double r, double g, double b)
        {
            double l = LightnessFromRGB(r, g, b);
            double s;
            double rf = r / 255;
            double gf = g / 255;
            double bf = b / 255;
            double colMax = Math.Max(rf, Math.Max(gf, bf));
            double colMin = Math.Min(rf, Math.Min(gf, bf));
            
            if (l < 1)
            {
                s = (colMax - colMin) / (1 - Math.Abs(2*l - 1));
            }
            else if (l == 1)
            {
                return 0;
            }
            else
            {
                throw new InvalidDataException("ERROR: invalid lightness {l}");
            }
            
            return s;
        }

        protected static double LightnessFromRGB(double r, double g, double b)
        {
            float rf = (float)r / 255f;
            float gf = (float)g / 255f;
            float bf = (float)b / 255f;
            float colMax = Math.Max(rf, Math.Max(gf, bf));
            float colMin = Math.Min(rf, Math.Min(gf, bf));
            return 0.5f * (colMax + colMin);
        }

        // TODO: fix Red, Green, Blue FromHSL
        public static double RedFromHSL(double h, double s, double l)
        {
            double r;
            double c = l * s;
            double x = c * (1-Math.Abs((h/(Math.PI/3f)) % 2 - 1));
            double m = l - c;

            h %= 2*Math.PI;

            if (h < Math.PI/3)
            {
                r = c;
            }
            else if (h < Math.PI - Math.PI/3)
            {
                r = x;
            }
            else if (h < Math.PI)
            {
                r = 0;
            }
            else if (h < Math.PI + Math.PI/3)
            {
                r = 0;
            }
            else if (h < 2*Math.PI - Math.PI/3)
            {
                r = x;
            }
            else if (h < 2*Math.PI)
            {
                r = c;
            }
            else
            {
                throw new InvalidDataException($"Red: invalid phase {h}");
            }
            return r*255;
        }

        public static double GreenFromHSL(double h, double s, double l)
        {
            double g;
            double c = l * s;
            double x = c * (1-Math.Abs((h/(Math.PI/3f)) % 2 - 1));
            double m = l - c;

            h %= 2*Math.PI;

            if (h < Math.PI/3)
            {
                g = x;
            }
            else if (h < Math.PI - Math.PI/3)
            {
                g = c;
            }
            else if (h < Math.PI)
            {
                g = c;
            }
            else if (h < Math.PI + Math.PI/3)
            {
                g = x;
            }
            else if (h < 2*Math.PI - Math.PI/3)
            {
                g = 0;
            }
            else if (h < 2*Math.PI)
            {
                g = 0;
            }
            else
            {
                throw new InvalidDataException($"Green: invalid phase {h}");
            }
            return g*255;


        }

        public static double BlueFromHSL(double h, double s, double l)
        {
            double b;
            double c = l * s;
            double x = c * (1-Math.Abs((h/(Math.PI/3f)) % 2 - 1));
            double m = l - c;

            h %= 2*Math.PI;
            if (h < Math.PI/3)
            {
                b = 0;
            }
            else if (h < Math.PI - Math.PI/3)
            {
                b = 0;
            }
            else if (h < Math.PI)
            {
                b = x;
            }
            else if (h < Math.PI + Math.PI/3)
            {
                b = c;
            }
            else if (h < 2*Math.PI - Math.PI/3)
            {
                b = c;
            }
            else if (h < 2*Math.PI)
            {
                b = x;
            }
            else
            {
                throw new InvalidDataException($"Blue: invalid phase {h}");
            }
            return b*255;
        }
    }

    public class RGBA : Color
    {
        double r;
        double g;
        double b;
        public RGBA(double r, double g, double b, double a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
        public RGBA(uint hex)
        {
            // 0xff00ff00
            this.r = hex >> 24;
            this.g = (hex & 0x00ff0000) >> 16;
            this.b = (hex & 0x0000ff00) >> 8;
            this.a = (hex & 0x000000ff);
        }

        public override double R {get => r%256; set => r = value; }
        public override double G {get => g%256; set => g = value; }
        public override double B {get => b%256; set => b = value; }
        public override double H
        {
            get => HueFromRGB(r, g, b);
            set
            {
                HSLA converted = ToHSLA();
                converted.H = value;
                R = converted.R;
                G = converted.G;
                B = converted.B;
            }
        }
        public override double S
        {
            get => SaturationFromRGB(r, g, b);
            set
            {
                Color converted = ToHSLA();
                converted.S = value;
                R = converted.R;
                G = converted.G;
                B = converted.B;
            }
        }
        public override double L
        {
            get => LightnessFromRGB(r, g, b);
            set
            {
                Color converted = ToHSLA();
                converted.L = value;
                R = converted.R;
                G = converted.G;
                B = converted.B;
            }
        }
        public HSLA ToHSLA()
        {
            return new HSLA(H, S, L, A);
        }

        public override RGBA Copy()
        {
            return new RGBA(R, G, B, A);
        }

        public override string ToString()
        {
            return $"RGBA({r}, {g}, {b}, {a})";
        }
    }

    public class HSLA : Color
    {
        double h;
        double s;
        double l;
        public HSLA(double h, double s, double l, double a)
        {
            this.h = h;
            this.s = s;
            this.l = l;
            this.a = a;
        }
        public override double H {get => h%(2*Math.PI); set => h = value;}
        public override double S {get => s; set => s = value;}
        public override double L {get => l; set => l = value;}
        public override double R
        {
            get => RedFromHSL(h, s, l);
            set
            {
                Color converted = ToRGBA();
                converted.R = value;
                H = converted.H;
                S = converted.S;
                L = converted.L;
            }
        }
        public override double G
        {
            get => GreenFromHSL(h, s, l);
            set
            {
                Color converted = ToRGBA();
                converted.G = value;
                H = converted.H;
                S = converted.S;
                L = converted.L;
            }
        }
        public override double B
        {
            get => BlueFromHSL(h, s, l);
            set
            {
                Color converted = ToRGBA();
                converted.B = value;
                H = converted.H;
                S = converted.S;
                L = converted.L;
            }
        }

        public RGBA ToRGBA()
        {
            return new RGBA(R, G, B, A);
        }

        public override HSLA Copy()
        {
            return new HSLA(H, S, L, A);
        }

        public override string ToString()
        {
            return $"HSLA({h}, {s}, {l}, {a})";
        }
    }
}