using Magician.Geo;
using Magician.Interactive;
using Magician.Library;
using Magician.Symbols;
using static Magician.Symbols.Algebra;

namespace Magician.Demos.Tests;

public class Proto3D : Spell
{
    Brush? b;
    double walkSpeed = 4.0;
    public override void Loop()
    {
        Renderer.RControl.Clear();
        Origin["2"].RotatedZ(-0.009);
        Origin["tetra"].RotatedX(-0.0025);
        Origin["tetra"].RotatedY(-0.003);
        Origin["tetra"].RotatedZ(-0.004);

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_w])
        {
            //Ref.Perspective.z.Delta(walkSpeed);
            Ref.Perspective.Forward(-walkSpeed);
            //Origin["tetra"].z.Delta(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_a])
        {
            //Ref.Perspective.x.Delta(-walkSpeed);
            Ref.Perspective.Strafe(-walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_s])
        {
            //Ref.Perspective.z.Delta(-walkSpeed);
            Ref.Perspective.Forward(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_d])
        {
            //Ref.Perspective.x.Delta(walkSpeed);
            Ref.Perspective.Strafe(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_j])
        {
            Ref.Perspective.RotatedY(-0.05);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_l])
        {
            Ref.Perspective.RotatedY(0.05);
        }

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_SPACE])
        {
            Ref.Perspective.y.Delta(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_LSHIFT])
        {
            Ref.Perspective.y.Delta(-walkSpeed);
        }

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_x])
        {
            Origin["cube"].RotatedX(0.01);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_y])
        {
            Origin["cube"].RotatedY(0.01);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_z])
        {
            Origin["cube"].RotatedZ(0.01);
        }

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_UP])
        {
            Origin["cube"].y.Delta(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_DOWN])
        {
            Origin["cube"].y.Delta(-walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_LEFT])
        {
            Origin["cube"].x.Delta(-walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_RIGHT])
        {
            Origin["cube"].x.Delta(walkSpeed);
        }
        //Ref.Perspective.x.Set(10 * Math.Sin(Time / 10));
        //Ref.Perspective.y.Set(10 * Math.Cos(Time / 10));
        //Scribe.Info(Ref.Perspective.z);

        double freqRatio = Math.Sin(Time/80)*2 + Math.Cos(Time/80)*5;
        
        //Origin["spring"] = new ParamMap(
        //    t => loopRadius * Math.Sin(t*freqRatio),
        //    t => loopRadius * Math.Cos(t*(1-freqRatio)) + 30*Math.Sin(t*Time/100),
        //    t => t * 20)
        //.Plot(0, 0, 0, 0, 25 * Math.PI, 0.15, new RGBA(0x00ffff));
        //Origin["spring"].Sub((m, i) => m.Colored(new HSLA(m.NormIdx * 2 * Math.PI + Time/4, 1, 1, 255)));
    }

    public override void PreLoop()
    {
        b = new Brush(new DirectMap(x => Events.MouseX), new DirectMap(y => Events.MouseY));
        //Origin["bg"] = new UI.RuledAxes(100, 10, 100, 10).Render();
        Origin["cube"] = Create.Cube(200, 100, -200, 10);
        Origin["cube"].Colored(new RGBA(0x6020ffff));
        Origin["2"] = Create.Cube(0, 0, 340, 100);

        Origin["tetra"] = Create.TriPyramid(-100, 0, 200, 16);

        Origin["my star"] = Create.Star(-200, -250, HSLA.RandomVisible(), 10, 40, 140).Flagged(DrawMode.FILLED);

        Oper lhs = Let("y");
        Oper rhs = new Fraction(N(100), Let("x"), Let("z"));
        Equation e = new(lhs, Equation.Fulcrum.EQUALS, rhs);
        Origin["plotEq"] = e.Plot(
            (Let("y"), Equation.AxisSpecifier.Y, -1000d, 1000d, 80d),
            (Let("x"), Equation.AxisSpecifier.X, -1000d, 1000d, 80d),
            (Let("z"), Equation.AxisSpecifier.Z, 0, 100, 60d)
        );

    }
}