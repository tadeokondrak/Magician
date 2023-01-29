using Magician.Renderer;
using static Magician.Geo.Create;

namespace Magician
{
    public interface IMap
    {
        public abstract double[] Evaluate(params double[] x);
        public abstract double Evaluate(double x);

        // IMap operators
        public virtual IMap Add(IMap o)
        {
            throw new NotImplementedException($"Method Add not supported on {this.GetType().Name}");
        }
        public virtual IMap Mult(IMap o)
        {
            throw new NotImplementedException($"Method Mult not supported on {this.GetType().Name}");
        }
        public virtual IMap Derivative()
        {
            throw new NotImplementedException($"Method Derivative not supported on {this.GetType().Name}");
        }
        public virtual IMap Integral()
        {
            throw new NotImplementedException($"Method Integral not supported on {this.GetType().Name}");
        }
        public virtual IMap Concat()
        {
            throw new NotImplementedException($"Method Concat not supported on {this.GetType().Name}");
        }

        // Place Multis along an IMap according to some truth function
        public Multi MultisAlong(double lb, double ub, double dx, Multi tmp, double xOffset=0, double yOffset=0, Func<double, double>? truth=null, double threshold=0)
        {
            if (truth is null)
            {
                truth = x => 1;
            }
            Multi m = new Multi(xOffset, yOffset);
            for (double i = lb; i < ub; i+=dx)
            {
                if (truth.Invoke(i) >= threshold)
                {
                    tmp.parent = m;
                    double[] p = Evaluate(new double[]{i});
                    // Parametric Multi placement
                    if (p.Length > 1)
                    {
                        m.Add(tmp.Copy().Positioned(p[0]+tmp.X.Evaluate(), p[1]+tmp.Y.Evaluate()));
                    }
                    // In terms of one axis
                    else
                    {
                        m.Add(tmp.Copy().Positioned(i+tmp.X.Evaluate(), p[0]+tmp.Y.Evaluate()));
                    }
                }
            }
            m.parent = Geo.Ref.Origin;
            return m.DrawFlags(DrawMode.INVISIBLE);
        }
        public Multi TextAlong(double lb, double ub, double dx, string msg, Color? c=null, double xOffset=0, double yOffset=0, Func<double, double>? truth=null, double threshold=0)
        {
            if (truth is null)
            {
                truth = x => 1;
            }
            if (c is null)
            {
                c = Globals.UIDefault.FG;
            }

            Multi m = new Multi(xOffset, yOffset);
            int j = 0;
            for (double i = lb; i < ub; i+=dx)
            {
                // Do not create more multis than characters in the string
                if (j >= msg.Length)
                {
                    break;
                }
                Text tx = new Text(msg.Substring(j, 1), c);
                Texture txr = tx.Render();
                Multi tmp = new Multi().Textured(txr);
                if (truth.Invoke(i) >= threshold)
                {
                    tmp.parent = m;
                    double[] p = Evaluate(new double[]{i});
                    if (p.Length > 1)
                    {
                        m.Add(tmp.Copy().Positioned(p[0]+tmp.X.Evaluate(), p[1]+tmp.Y.Evaluate()));
                    }
                    else
                    {
                        m.Add(tmp.Copy().Positioned(i+tmp.X.Evaluate(), p[0]+tmp.Y.Evaluate()));
                    }
                }
                j++;
                tx.Dispose();
            }
            m.parent = Geo.Ref.Origin;
            return m.DrawFlags(DrawMode.INVISIBLE);
        }

        // Render an IMap to a Multi
        public Multi Plot(double x, double y, double start, double end, double dx, Color c)
        {
            List<Multi> points = new List<Multi>();
            for (double t = start; t < end; t+=dx)
            {
                Multi[] ps = interpolate(t, t+dx);
                //ps[0].Col = c;
                //ps[1].Col = c;
                // TODO: test plotting after cleaning Multi
                ps[0].Colored(c);
                ps[1].Colored(c);
                points.Add(ps[0].DrawFlags(DrawMode.INVISIBLE));

            }
            Multi m = new Multi(x, y, c, DrawMode.PLOT, points.ToArray());
            return m;
        }

        private Multi[] interpolate(double t0, double t1)
        {            
            double[] p0 = Evaluate(new double[] {t0});
            double[] p1 = Evaluate(new double[] {t1});
            
            if (p0.Count() < 2)
            {
                p0 = new double[]{t0, p0[0]};
            }
            if (p1.Count() < 2)
            {
                p1 = new double[]{t1, p1[0]};
            }


            Multi mp0 = Point(p0[0], p0[1]);
            Multi mp1 = Point(p1[0], p1[1]);
            return new Multi[] {mp0, mp1};
        }
    }
}