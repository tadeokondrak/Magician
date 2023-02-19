using Magician.Backend;
using static SDL2.SDL;

namespace Magician
{
    public static class SDLGlobals
    {
        public static IntPtr renderer;
        public static IntPtr renderedTexture;
        public static ITexture renderedTextureWrapper;
    }
}
