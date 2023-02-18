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

        Multi AsMulti()
        {
            Scribe.Error("Text as Multi not supported yet");
            throw new Exception();
        }

        public override string ToString()
        {
            return $"Text {s}";
        }
    }
}
