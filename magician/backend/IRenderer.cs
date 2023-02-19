namespace Magician.Backend
{
    interface IRenderer
    {
        ITexture? Target { set; }

        ITexture CreateTexture(int width, int height, PixelFormat format = PixelFormat.Argb32, TextureAccess access = TextureAccess.RenderTarget);

        void Present();
    }
}
