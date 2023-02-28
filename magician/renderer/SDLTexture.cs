using Magician.Renderer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static SDL2.SDL;

// Wrapper around an SDL texture
namespace Magician.Renderer
{
    public class Texture : IDisposable
    {
        IntPtr texture; // SDL_Texture*
        public int Width { get; private set; }
        public int Height { get; private set; }

        // Create a texture from an image file
        public Texture(string filepath)
        {
            Image<Rgba32> image;
            try
            {
                image = Image.Load<Rgba32>(filepath);
            }
            catch (IOException)
            {
                using Stream stream = typeof(Text).Assembly.GetManifestResourceStream("MagicianDemo.magician.ui.assets.default.png")!;
                image = Image.Load<Rgba32>(stream);
            }
            Width = image.Width;
            Height = image.Height;
            IntPtr surface = SDL_CreateRGBSurfaceWithFormat(0, Width, Height, 32, SDL_PIXELFORMAT_ABGR8888);
            unsafe
            {
                SDL_Surface* surfacePtr = (SDL_Surface*)surface;
                Span<Rgba32> dest = new((void*)surfacePtr->pixels, surfacePtr->pitch * surfacePtr->h);
                image.CopyPixelDataTo(dest);
            }

            texture = SDL_CreateTextureFromSurface(SDLGlobals.renderer, surface);
            SDL_FreeSurface(surface);
        }

        // Create a texture from a given SDL texture
        public Texture(IntPtr texture)
        {
            this.texture = texture;

            // Grab width and height of the rendered text
            unsafe
            {
                Width = ((SDL_Surface*)texture)->w;
                Height = ((SDL_Surface*)texture)->h;
            }
        }

        public void Draw(double xOffset = 0, double yOffset = 0)
        {

            // Options
            Control.SetRenderDrawBlendMode(SDL_BlendMode.SDL_BLENDMODE_BLEND);
            SDL_SetTextureBlendMode(texture, SDL_BlendMode.SDL_BLENDMODE_BLEND);

            // Draw objects
            SDL_SetRenderTarget(SDLGlobals.renderer, texture);
            SDL_Rect srcRect = new()
            {
                x = 0,
                y = 0,
                w = Width,
                h = Height,
            };
            SDL_Rect dstRect = new()
            {
                x = (int)xOffset,
                y = (int)yOffset,
                w = Width,
                h = Height,
            };

            SDL_RenderCopy(SDLGlobals.renderer, texture, ref srcRect, ref dstRect);
        }

        /* IDisposable implementation */
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (texture != IntPtr.Zero)
            {
                SDL_DestroyTexture(texture);
            }
        }

        ~Texture()
        {
            Dispose(false);
        }
    }
}
