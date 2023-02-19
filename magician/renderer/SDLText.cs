using System.Numerics;
using Magician.Geo;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static SDL2.SDL;

namespace Magician.Renderer
{
    public class Text
    {
        public static string FallbackFontPath = "";
        string fontPath;

        string s;
        Color c;
        int size;
        Font font;

        public Text(string s, Color c, int size, string fp = "")
        {
            if (FallbackFontPath == "")
            {
                throw new InvalidDataException("Must set fallback font path before using Text");
            }
            this.s = s;
            this.c = c;
            this.size = size;
            fontPath = fp == "" ? FallbackFontPath : fp;
            FontCollection collection = new();
            FontFamily family = collection.Add(fontPath);
            font = family.CreateFont(size);
        }

        public Texture Render()
        {
            SixLabors.ImageSharp.Color imageColor = new(new Argb32((byte)c.R, (byte)c.G, (byte)c.B, (byte)c.A));

            TextOptions textOptions = new(font);
            FontRectangle rectangle = TextMeasurer.Measure(s, textOptions);
            using Image<Rgba32> image = new((int)rectangle.Width, (int)rectangle.Height);
            IPathCollection paths = TextBuilder.GenerateGlyphs(s, textOptions);
            image.Mutate(x => x.Fill(imageColor, paths));
            IntPtr textSurface = SDL_CreateRGBSurfaceWithFormat(0, (int)rectangle.Width, (int)rectangle.Height, 32, SDL_PIXELFORMAT_ABGR8888);
            unsafe
            {
                SDL_Surface* surfacePtr = (SDL_Surface*)textSurface;
                Span<Rgba32> dest = new((void*)surfacePtr->pixels, surfacePtr->pitch * surfacePtr->h);
                image.CopyPixelDataTo(dest);
            }

            IntPtr textTexture = SDL_CreateTextureFromSurface(SDLGlobals.renderer, textSurface);
            SDL_FreeSurface(textSurface);
            return new Texture(textTexture);

            //throw new NotImplementedException("Text as Texture not supported. Please file an issue at https://github.com/Calendis/Magician");
        }

        public Multi AsMulti()
        {
            TextOptions textOptions = new(font);
            MultiGlyphRenderer renderer = new(c);
            TextRenderer.RenderTextTo(renderer, s, textOptions);
            return renderer.Parent.Parented(Ref.Origin);
        }

        public override string ToString()
        {
            return $"Text {s}";
        }
    }

    class MultiGlyphRenderer : IGlyphRenderer
    {
        public Multi Parent { get; private set; } = new Multi().DrawFlags(DrawMode.INVISIBLE);
        Color color;
        Multi? current;

        public MultiGlyphRenderer(Color color)
        {
            this.color = color;
        }

        public void BeginFigure()
        {
        }

        public bool BeginGlyph(FontRectangle bounds, GlyphRendererParameters parameters)
        {
            return true;
        }

        public void BeginText(FontRectangle bounds)
        {
        }

        public void EndFigure()
        {
        }

        public void EndGlyph()
        {
        }

        public void EndText()
        {
        }

        public void LineTo(Vector2 point)
        {
            Multi last = current!;
            current = Create.Point(Parent, point.X, point.Y, Data.Col.UIDefault.FG);
            Parent.Add(Create.Line(last, current, Data.Col.UIDefault.FG).Colored(color));
            Console.WriteLine("{0} {1} {2} {3}", last.X, last.Y, current.X, current.Y);
        }

        public void MoveTo(Vector2 point)
        {
            current = Create.Point(Parent, point.X, point.Y, Data.Col.UIDefault.FG);
        }

        public void QuadraticBezierTo(Vector2 secondControlPoint, Vector2 point)
        {
            AGG.curve3 curve = new(current!.X, current!.Y, secondControlPoint.X, secondControlPoint.Y, point.X, point.Y);
            double x = 0;
            double y = 0;
            while (true)
            {
                switch (curve.vertex(ref x, ref y))
                {
                    case AGG.Constants.path_cmd_stop:
                        return;
                    case AGG.Constants.path_cmd_move_to:
                        MoveTo(new Vector2((float)x, (float)y));
                        break;
                    case AGG.Constants.path_cmd_line_to:
                        LineTo(new Vector2((float)x, (float)y));
                        break;
                }
            }
        }

        public void CubicBezierTo(Vector2 secondControlPoint, Vector2 thirdControlPoint, Vector2 point)
        {
            AGG.curve4 curve = new(
                current!.X, current!.Y,
                secondControlPoint.X, secondControlPoint.Y,
                thirdControlPoint.X, thirdControlPoint.Y,
                point.X, point.Y);
            double x = 0;
            double y = 0;
            while (true)
            {
                switch (curve.vertex(ref x, ref y))
                {
                    case AGG.Constants.path_cmd_stop:
                        return;
                    case AGG.Constants.path_cmd_move_to:
                        MoveTo(new Vector2((float)x, (float)y));
                        break;
                    case AGG.Constants.path_cmd_line_to:
                        LineTo(new Vector2((float)x, (float)y));
                        break;
                }
            }
        }
    }
}
