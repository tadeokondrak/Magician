﻿using static SDL2.SDL;

namespace Magician
{
    class Demo
    {
        static IntPtr win;
        static IntPtr renderer;

        bool done = false;
        Random r = new Random();
        int frames = 0;
        int stopFrame = -1;
        bool saveFrames = false;
        int driveDelay = 0;
        double timeResolution = 0.1;

        static void Main(string[] args)
        {
            // Startup
            Console.WriteLine("Abracadabra!");

            Demo demo = new Demo();
            demo.InitSDL();
            demo.CreateWindow();
            demo.CreateRenderer();

            demo.GameLoop();

            // Cleanup
            SDL_DestroyRenderer(renderer);
            SDL_DestroyWindow(win);
            SDL_Quit();
        }

        void GameLoop()
        {
            /*
            *  Pre-loop
            *  -----------------------------------------------------------------
            *  Much is possible in the pre-loop, since Drivers will still work
            */
            Quantity theta = new Quantity(0).Driven(x => x[0]);
            Quantity.ExtantQuantites.Add(theta);

            Driver sin = new Driver(x=>0);
            Multi squareWave = new Multi();


            /*
            *  Loop
            *  ----------------------------------------------------------------
            *  The loop automatically drives and renders the math objects.
            *  When you want to modulate arguments in a constructor, you will
            *  need the loop
            */
            while (!done)
            {
                // Modulators
                double h = Math.PI*2*Math.Sin(frames*timeResolution*0.03);
                sin = new Driver(x => 120*Math.Sin(x[0]/50 + (double)frames/40));
                
                // A group of 5-pointed stars in a sinusoidal pattern, rotating, and shifting through hue
                squareWave = ((IMap)sin).MultisAlong(-600, 600, 40, new Driver(x=>1), 0,
                    Multi.Star(5, 10, 20)
                        .Sub(m => m.Rotated((double)frames/90))
                        )
                    .Sub(m => m.Colored(new HSLA(h+(double)m.Index/10, 1, 1, 255)))
                        ;
                
                // Add our Multi
                Multi.Origin.Modify(squareWave.DrawFlags(0));
                
                //SDL_WaitEvent(out SDL_Event events);
                SDL_PollEvent(out SDL_Event sdlEvent);
                if (frames >= driveDelay)
                {
                    Drive();
                }
                if (frames != stopFrame)
                {
                    Render();
                }

                // Event handling
                switch (sdlEvent.type)
                {
                    case SDL_EventType.SDL_QUIT:
                        done = true;
                        break;
                }
            }
        }

        void Render()
        {
            // Clear with colour)
            SDL_SetRenderDrawColor(renderer,
            (byte)Globals.bgCol.R, (byte)Globals.bgCol.G, (byte)Globals.bgCol.B, (byte)Globals.bgCol.A);
            SDL_RenderClear(renderer);

            // Draw objects
            Multi.Origin.Draw(ref renderer, 0, 0);

            // SAVE FRAME TO IMAGE
            if (saveFrames)
            {
                IntPtr texture = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_ARGB8888, 0, Globals.winWidth, Globals.winHeight);
                IntPtr target = SDL_GetRenderTarget(renderer);

                int width, height;
                SDL_SetRenderTarget(renderer, texture);
                SDL_QueryTexture(texture, out _, out _, out width, out height);
                IntPtr surface = SDL_CreateRGBSurfaceWithFormat(SDL_RLEACCEL, width, height, 0, SDL_PIXELFORMAT_ARGB8888);
                SDL_Rect r = new SDL_Rect();
                r.x = 0;
                r.y = 0;
                r.w = Globals.winWidth;
                r.h = Globals.winHeight;
                unsafe
                {
                    SDL_Surface* surf = (SDL_Surface*)surface;
                    SDL_RenderReadPixels(renderer, ref r, SDL_PIXELFORMAT_ARGB8888, surf->pixels, surf->pitch);
                    SDL_SaveBMP(surface, $"saved/frame_{frames.ToString("D3")}.bmp");
                    SDL_FreeSurface(surface);
                }
            }

            frames++;

            // Display
            SDL_RenderPresent(renderer);
            //SDL_Delay(1/6);
        }

        void Drive()
        {
            Multi.Origin.Go((frames - driveDelay) * timeResolution);
            for (int i = 0; i < Quantity.ExtantQuantites.Count; i++)
            {
                Quantity.ExtantQuantites[i].Go((frames - driveDelay) * timeResolution);
            }
        }
        void InitSDL()
        {
            if (SDL_Init(SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine($"Error initializing SDL: {SDL_GetError()}");
            }
        }
        void CreateWindow()
        {
            win = SDL_CreateWindow("Test Window", 0, 0, Globals.winWidth, Globals.winHeight, SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            if (win == IntPtr.Zero)
            {
                Console.WriteLine($"Error creating the window: {SDL_GetError()}");
            }
        }
        void CreateRenderer()
        {
            renderer = SDL_CreateRenderer(win, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
            if (renderer == IntPtr.Zero)
            {
                Console.WriteLine($"Error creating the renderer: {SDL_GetError()}");
            }
        }
    }
}