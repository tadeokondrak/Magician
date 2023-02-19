using static SDL2.SDL;

namespace Magician.Backend
{
    class SDLTexture : ITexture, IDisposable
    {
        public int Width { get; private set; }

        public int Height { get; private set; }
        public IntPtr Texture { get; private set; } // SDL_Texture*

        public SDLTexture(SDLRenderer renderer, int width, int height, PixelFormat format = PixelFormat.Argb32, TextureAccess access = TextureAccess.RenderTarget)
        {
            Width = width;
            Height = height;
            uint pixelFormat = format switch
            {
                PixelFormat.Argb32 => SDL_PIXELFORMAT_ARGB8888,
                _ => throw new Exception($"Unknown pixel format {format}"),
            };
            int textureAccess = access switch
            {
                TextureAccess.RenderTarget => (int)SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET,
                _ => throw new Exception($"Unknown texture access {access}"),
            };
            Texture = SDL_CreateTexture(renderer.Renderer, pixelFormat, textureAccess, width, height);
            if (Texture == IntPtr.Zero)
            {
                throw new Exception("SDL_CreateTexture failed: " + SDL_GetError());
            }
        }

        public void Dispose()
        {
            if (Texture != IntPtr.Zero)
            {
                SDL_DestroyTexture(Texture);
                Texture = IntPtr.Zero;
            }
        }
    }
}
