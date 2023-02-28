using static SDL2.SDL;

namespace Magician.Renderer
{
    public static class Control
    {
        public static bool doRender = true;
        public static bool display = true;
        public static bool saveFrame = false;
        public static int saveCount = 0;
        static IntPtr target;
        public static void Clear()
        {
            Clear(Data.Col.UIDefault.BG);
        }
        public static void Clear(Color c)
        {
            SaveTarget();
            SDL_SetRenderTarget(SDLGlobals.renderer, SDLGlobals.renderedTexture);
            SDL_SetRenderDrawColor(SDLGlobals.renderer, (byte)c.R, (byte)c.G, (byte)c.B, (byte)c.A);
            SDL_RenderClear(SDLGlobals.renderer);
            RecallTarget();
        }

        public static void SaveTarget()
        {
            target = SDL_GetRenderTarget(SDLGlobals.renderer);
        }

        public static void RecallTarget()
        {
            SDL_SetRenderTarget(SDLGlobals.renderer, target);
        }

        public static void SetTarget(IntPtr texture)
        {
            SDL_SetRenderTarget(SDLGlobals.renderer, texture);
        }

        public static void SetDrawColor(Color c)
        {
            SDL_SetRenderDrawColor(SDLGlobals.renderer, (byte)c.R, (byte)c.G, (byte)c.B, (byte)c.A);
        }

        public static void DrawPointF(float x, float y)
        {
            SDL_RenderDrawPointF(SDLGlobals.renderer, x, y);
        }

        public static void DrawLineF(float x1, float y1, float x2, float y2)
        {
            SDL_RenderDrawLineF(SDLGlobals.renderer, x1, y1, x2, y2);
        }

        public static void RenderGeometry(IntPtr texture, SDL_Vertex[] vertices, int num_vertices, int[]? indices, int num_indices)
        {
            SDL_RenderGeometry(SDLGlobals.renderer, texture, vertices, num_vertices, indices, num_indices);
        }

        public static void SetRenderDrawBlendMode(SDL_BlendMode blendMode)
        {
            SDL_SetRenderDrawBlendMode(SDLGlobals.renderer, blendMode);
        }
    }
}