using static SDL2.SDL;
using static SDL2.SDL.SDL_WindowFlags;

namespace Magician.Backend
{
    class SDLWindow : IWindow, IDisposable
    {
        public IntPtr Window { get; private set; } // SDL_Window*
        public int Width { get; private set; }
        public int Height { get; private set; }

        public SDLWindow()
        {
            if (SDL_Init(SDL_INIT_VIDEO) != 0)
            {
                throw new Exception("SDL_Init failed: " + SDL_GetError());
            }

            Window = SDL_CreateWindow(
                Data.App.Title, SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED,
                Data.Globals.winWidth, Data.Globals.winHeight, SDL_WINDOW_RESIZABLE);
            if (Window == IntPtr.Zero)
            {
                throw new Exception("SDL_CreateWindow failed: " + SDL_GetError());
            }

            SDL_GetWindowSize(Window, out int width, out int height);
            Width = width;
            Height = height;
        }

        public void Dispose()
        {
            if (Window != IntPtr.Zero)
            {
                SDL_DestroyWindow(Window);
                Window = IntPtr.Zero;
            }
        }

        public SDLRenderer CreateRenderer()
        {
            return new SDLRenderer(this);
        }
    }
}
