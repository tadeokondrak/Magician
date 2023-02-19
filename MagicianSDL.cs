using static SDL2.SDL;
using Magician.Library;
using Magician.Backend;

namespace Magician
{
    class MagicianSDL
    {
        static SDLWindow window;
        static SDLRenderer renderer;
        static IntPtr win;
        static bool done = false;
        static int frames = 0;
        static int stopFrame = -1;
        static int driveDelay = 0;
        static double timeResolution = 0.1;

        static void Main(string[] args)
        {
            /* Startup */
            Console.WriteLine(Data.App.Title);
            window = new();
            renderer = window.CreateRenderer();
            SDLGlobals.renderer = renderer.Renderer;

            // Load a spell
            Spellcaster.Load(new Demos.DefaultSpell());

            // Run
            MainLoop();
        }

        static void MainLoop()
        {
            // Create a texture from the surface
            // Textures are hardware-acclerated, while surfaces use CPU rendering
            SDLTexture renderTexture = renderer.CreateTexture(Data.Globals.winWidth, Data.Globals.winHeight);
            SDLGlobals.renderedTexture = renderTexture.Texture;
            SDLGlobals.renderedTextureWrapper = renderTexture;

            while (!done)
            {
                Spellcaster.Loop(frames * timeResolution);

                // Event handling
                while (SDL_PollEvent(out SDL_Event sdlEvent) != 0)
                {
                    Interactive.Events.Process(sdlEvent);
                    switch (sdlEvent.type)
                    {
                        case SDL_EventType.SDL_WINDOWEVENT:
                            SDL_WindowEvent windowEvent = sdlEvent.window;
                            switch (windowEvent.windowEvent)
                            {

                                case SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                                    Data.Globals.winWidth = windowEvent.data1;
                                    Data.Globals.winHeight = windowEvent.data2;
                                    renderTexture = renderer.CreateTexture(Data.Globals.winWidth, Data.Globals.winHeight);
                                    SDLGlobals.renderedTexture = renderTexture.Texture;
                                    SDLGlobals.renderedTextureWrapper = renderTexture;

                                    break;
                            }
                            break;
                        case SDL_EventType.SDL_QUIT:
                            done = true;
                            break;
                    }
                }

                // Drive things
                if (frames >= driveDelay)
                {
                    Geo.Ref.Origin.Drive();
                }

                // Draw things
                if (frames != stopFrame)
                {
                    Render();
                }
            }
        }

        // Renders each frame to a texture and displays the texture
        static void Render()
        {
            if (Renderer.Control.doRender)
            {
                // Options
                SDL_SetRenderDrawBlendMode(SDLGlobals.renderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);
                SDL_SetTextureBlendMode(SDLGlobals.renderedTexture, SDL_BlendMode.SDL_BLENDMODE_BLEND);

                // Draw objects
                Geo.Ref.Origin.Draw(UI.Perspective.x.Evaluate(), UI.Perspective.y.Evaluate());

                // SAVE FRAME TO IMAGE
                if (Renderer.Control.saveFrame && frames < stopFrame)
                {
                    using SDLTexture texture = renderer.CreateTexture(Data.Globals.winWidth, Data.Globals.winHeight);
                    IntPtr target = SDL_GetRenderTarget(SDLGlobals.renderer);

                    renderer.Target = texture;
                    IntPtr surface = SDL_CreateRGBSurfaceWithFormat(SDL_RLEACCEL, texture.Width, texture.Height, 0, SDL_PIXELFORMAT_ARGB8888);
                    SDL_Rect r = new()
                    {
                        x = 0,
                        y = 0,
                        w = Data.Globals.winWidth,
                        h = Data.Globals.winHeight,
                    };
                    unsafe
                    {
                        renderer.Target = null;

                        SDL_Surface* surf = (SDL_Surface*)surface;
                        SDL_RenderReadPixels(SDLGlobals.renderer, ref r, SDL_PIXELFORMAT_ARGB8888, surf->pixels, surf->pitch);
                        SDL_SaveBMP(surface, $"saved/frame_{Renderer.Control.saveCount:D4}.bmp");
                        Renderer.Control.saveCount++;
                        SDL_FreeSurface(surface);

                        renderer.Target = SDLGlobals.renderedTextureWrapper;
                    }
                }

                // Display
                SDL_Rect srcRect = new()
                {
                    x = 0,
                    y = 0,
                    w = Data.Globals.winWidth,
                    h = Data.Globals.winHeight,
                };

                SDL_Rect dstRect = new()
                {
                    x = 0,
                    y = 0,
                    w = Data.Globals.winWidth,
                    h = Data.Globals.winHeight,
                };

                if (Renderer.Control.display)
                {
                    renderer.Target = null;
                    SDL_RenderCopy(SDLGlobals.renderer, SDLGlobals.renderedTexture, ref srcRect, ref dstRect);
                    renderer.Present();
                }
                //SDL_Delay(1/6);
            }
            frames++;
        }
    }
}
