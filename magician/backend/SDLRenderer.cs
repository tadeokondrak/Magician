using static SDL2.SDL;
using static SDL2.SDL.SDL_RendererFlags;

namespace Magician.Backend
{
    class SDLRenderer : IRenderer, IDisposable
    {
        private readonly SDLWindow window;
        public IntPtr Renderer { get; private set; } // SDL_Renderer*
        public ITexture? Target
        {
            set
            {
                if (value == null)
                {
                    SDL_SetRenderTarget(Renderer, IntPtr.Zero);
                }
                else
                {
                    SDL_SetRenderTarget(Renderer, (value as SDLTexture)!.Texture);
                }
            }
        }

        public SDLRenderer(SDLWindow window)
        {
            this.window = window;
            Renderer = SDL_CreateRenderer(window.Window, -1, SDL_RENDERER_ACCELERATED | SDL_RENDERER_PRESENTVSYNC | SDL_RENDERER_TARGETTEXTURE);
            if (Renderer == IntPtr.Zero)
            {
                throw new Exception("SDL_CreateRenderer failed: " + SDL_GetError());
            }
        }

        public void Dispose()
        {
            if (Renderer != IntPtr.Zero)
            {
                SDL_DestroyRenderer(Renderer);
                Renderer = IntPtr.Zero;
            }
        }
        ITexture IRenderer.CreateTexture(int width, int height, PixelFormat format, TextureAccess access)
            => CreateTexture(width, height, format, access);

        public SDLTexture CreateTexture(int width, int height, PixelFormat format = PixelFormat.Argb32, TextureAccess access = TextureAccess.RenderTarget)
        {
            return new SDLTexture(this, width, height, format, access);
        }

        public void Present()
        {
            SDL_RenderPresent(Renderer);
        }
    }
}
