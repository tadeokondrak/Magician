using point_d = System.Numerics.Vector2;
using static AGG.Functions;
using static AGG.Constants;
using static AGG.curve_approximation_method_e;

/*
 * This code is a port of code from Anti-Grain Geometry (AGG).
 * https://web.archive.org/web/20180307160123/http://antigrain.com/research/adaptive_bezier/index.html
 */

namespace AGG
{
    static class Constants
    {
        public const double pi = Math.PI;

        public const double curve_distance_epsilon = 1e-30;
        public const double curve_collinearity_epsilon = 1e-30;
        public const double curve_angle_tolerance_epsilon = 0.01;
        public const uint curve_recursion_limit = 32;

        public const uint path_cmd_stop = 0;
        public const uint path_cmd_move_to = 1;
        public const uint path_cmd_line_to = 2;
        public const uint path_cmd_curve3 = 3;
        public const uint path_cmd_curve4 = 4;
        public const uint path_cmd_curveN = 5;
        public const uint path_cmd_catrom = 6;
        public const uint path_cmd_ubspline = 7;
        public const uint path_cmd_end_poly = 0x0F;
        public const uint path_cmd_mask = 0x0F;
    }

    static class Functions
    {

        public static double sqrt(double v)
        {
            return Math.Sqrt(v);
        }

        public static double fabs(double v)
        {
            return Math.Abs(v);
        }

        public static double atan2(double y, double x)
        {
            return Math.Atan2(y, x);
        }

        public static int uround(double v)
        {
            return (int)(v + 0.5);
        }

        public static double calc_sq_distance(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return dx * dx + dy * dy;
        }

        //-------------------------------------------------------catrom_to_bezier
        public static curve4_points catrom_to_bezier(double x1, double y1,
                                                     double x2, double y2,
                                                     double x3, double y3,
                                                     double x4, double y4)
        {
            // Trans. matrix Catmull-Rom to Bezier
            //
            //  0       1       0       0
            //  -1/6    1       1/6     0
            //  0       1/6     1       -1/6
            //  0       0       1       0
            //
            return new curve4_points(
                x2,
                y2,
                (-x1 + 6 * x2 + x3) / 6,
                (-y1 + 6 * y2 + y3) / 6,
                (x2 + 6 * x3 - x4) / 6,
                (y2 + 6 * y3 - y4) / 6,
                x3,
                y3);
        }


        //-----------------------------------------------------------------------
        public static curve4_points catrom_to_bezier(curve4_points cp)
        {
            return catrom_to_bezier(cp[0], cp[1], cp[2], cp[3],
                                    cp[4], cp[5], cp[6], cp[7]);
        }



        //-----------------------------------------------------ubspline_to_bezier
        public static curve4_points ubspline_to_bezier(double x1, double y1,
                                                       double x2, double y2,
                                                       double x3, double y3,
                                                       double x4, double y4)
        {
            // Trans. matrix Uniform BSpline to Bezier
            //
            //  1/6     4/6     1/6     0
            //  0       4/6     2/6     0
            //  0       2/6     4/6     0
            //  0       1/6     4/6     1/6
            //
            return new curve4_points(
                (x1 + 4 * x2 + x3) / 6,
                (y1 + 4 * y2 + y3) / 6,
                (4 * x2 + 2 * x3) / 6,
                (4 * y2 + 2 * y3) / 6,
                (2 * x2 + 4 * x3) / 6,
                (2 * y2 + 4 * y3) / 6,
                (x2 + 4 * x3 + x4) / 6,
                (y2 + 4 * y3 + y4) / 6);
        }


        //-----------------------------------------------------------------------
        public static curve4_points ubspline_to_bezier(curve4_points cp)
        {
            return ubspline_to_bezier(cp[0], cp[1], cp[2], cp[3],
                                      cp[4], cp[5], cp[6], cp[7]);
        }




        //------------------------------------------------------hermite_to_bezier
        public static curve4_points hermite_to_bezier(double x1, double y1,
                                                      double x2, double y2,
                                                      double x3, double y3,
                                                      double x4, double y4)
        {
            // Trans. matrix Hermite to Bezier
            //
            //  1       0       0       0
            //  1       0       1/3     0
            //  0       1       0       -1/3
            //  0       1       0       0
            //
            return new curve4_points(
                x1,
                y1,
                (3 * x1 + x3) / 3,
                (3 * y1 + y3) / 3,
                (3 * x2 - x4) / 3,
                (3 * y2 - y4) / 3,
                x2,
                y2);
        }



        //-----------------------------------------------------------------------
        public static curve4_points hermite_to_bezier(curve4_points cp)
        {
            return hermite_to_bezier(cp[0], cp[1], cp[2], cp[3],
                                     cp[4], cp[5], cp[6], cp[7]);
        }
    }

    //--------------------------------------------curve_approximation_method_e
    enum curve_approximation_method_e
    {
        curve_inc,
        curve_div
    };

    //--------------------------------------------------------------curve3_inc
    class curve3_inc
    {
        public curve3_inc()
        {
            m_num_steps = 0;
            m_step = 0;
            m_scale = 1.0;
        }

        public curve3_inc(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            m_num_steps = 0;
            m_step = 0;
            m_scale = 1.0;
            init(x1, y1, x2, y2, x3, y3);
        }

        public void reset()
        {
            m_num_steps = 0;
            m_step = -1;
        }

        //------------------------------------------------------------------------
        public void init(double x1, double y1,
                         double x2, double y2,
                         double x3, double y3)
        {
            m_start_x = x1;
            m_start_y = y1;
            m_end_x = x3;
            m_end_y = y3;

            double dx1 = x2 - x1;
            double dy1 = y2 - y1;
            double dx2 = x3 - x2;
            double dy2 = y3 - y2;

            double len = sqrt(dx1 * dx1 + dy1 * dy1) + sqrt(dx2 * dx2 + dy2 * dy2);

            m_num_steps = uround(len * 0.25 * m_scale);

            if (m_num_steps < 4)
            {
                m_num_steps = 4;
            }

            double subdivide_step = 1.0 / m_num_steps;
            double subdivide_step2 = subdivide_step * subdivide_step;

            double tmpx = (x1 - x2 * 2.0 + x3) * subdivide_step2;
            double tmpy = (y1 - y2 * 2.0 + y3) * subdivide_step2;

            m_saved_fx = m_fx = x1;
            m_saved_fy = m_fy = y1;

            m_saved_dfx = m_dfx = tmpx + (x2 - x1) * (2.0 * subdivide_step);
            m_saved_dfy = m_dfy = tmpy + (y2 - y1) * (2.0 * subdivide_step);

            m_ddfx = tmpx * 2.0;
            m_ddfy = tmpy * 2.0;

            m_step = m_num_steps;
        }


        public void approximation_method(curve_approximation_method_e _) { }
        public curve_approximation_method_e approximation_method() { return curve_inc; }

        public void approximation_scale(double s) { m_scale = s; }
        public double approximation_scale() { return m_scale; }

        public void angle_tolerance(double _) { }
        public double angle_tolerance() { return 0.0; }

        public void cusp_limit(double _) { }
        public double cusp_limit() { return 0.0; }

        public void rewind(uint path_id)
        {
            if (m_num_steps == 0)
            {
                m_step = -1;
                return;
            }
            m_step = m_num_steps;
            m_fx = m_saved_fx;
            m_fy = m_saved_fy;
            m_dfx = m_saved_dfx;
            m_dfy = m_saved_dfy;
        }

        public uint vertex(ref double x, ref double y)
        {
            if (m_step < 0) return path_cmd_stop;
            if (m_step == m_num_steps)
            {
                x = m_start_x;
                y = m_start_y;
                --m_step;
                return path_cmd_move_to;
            }
            if (m_step == 0)
            {
                x = m_end_x;
                y = m_end_y;
                --m_step;
                return path_cmd_line_to;
            }
            m_fx += m_dfx;
            m_fy += m_dfy;
            m_dfx += m_ddfx;
            m_dfy += m_ddfy;
            x = m_fx;
            y = m_fy;
            --m_step;
            return path_cmd_line_to;
        }

        int m_num_steps;
        int m_step;
        double m_scale;
        double m_start_x;
        double m_start_y;
        double m_end_x;
        double m_end_y;
        double m_fx;
        double m_fy;
        double m_dfx;
        double m_dfy;
        double m_ddfx;
        double m_ddfy;
        double m_saved_fx;
        double m_saved_fy;
        double m_saved_dfx;
        double m_saved_dfy;
    };

    //-------------------------------------------------------------curve3_div
    class curve3_div
    {
        public curve3_div()
        {
            m_approximation_scale = 1.0;
            m_angle_tolerance = 0.0;
            m_count = 0;
        }

        public curve3_div(double x1, double y1,
                          double x2, double y2,
                          double x3, double y3)
        {
            m_approximation_scale = 1.0;
            m_angle_tolerance = 0.0;
            m_count = 0;
            init(x1, y1, x2, y2, x3, y3);
        }

        public void reset()
        {
            m_points.Clear();
            m_count = 0;
        }

        public void init(double x1, double y1,
                  double x2, double y2,
                  double x3, double y3)
        {
            m_points.Clear();
            m_distance_tolerance_square = 0.5 / m_approximation_scale;
            m_distance_tolerance_square *= m_distance_tolerance_square;
            bezier(x1, y1, x2, y2, x3, y3);
            m_count = 0;
        }

        public void approximation_method(curve_approximation_method_e _) { }
        public curve_approximation_method_e approximation_method() { return curve_div; }

        public void approximation_scale(double s) { m_approximation_scale = s; }
        public double approximation_scale() { return m_approximation_scale; }

        public void angle_tolerance(double a) { m_angle_tolerance = a; }
        public double angle_tolerance() { return m_angle_tolerance; }

        public void cusp_limit(double _) { }
        public double cusp_limit() { return 0.0; }

        public void rewind(uint _)
        {
            m_count = 0;
        }

        public uint vertex(ref double x, ref double y)
        {
            if (m_count >= m_points.Count) return path_cmd_stop;
            int p = (int)m_count++;
            x = m_points[p].X;
            y = m_points[p].Y;
            return (m_count == 1) ? path_cmd_move_to : path_cmd_line_to;
        }

        void bezier(double x1, double y1,
                    double x2, double y2,
                    double x3, double y3)
        {
            m_points.Add(new point_d((float)x1, (float)y1));
            recursive_bezier(x1, y1, x2, y2, x3, y3, 0);
            m_points.Add(new point_d((float)x3, (float)y3));
        }

        void recursive_bezier(double x1, double y1,
                              double x2, double y2,
                              double x3, double y3,
                              uint level)
        {
            if (level > curve_recursion_limit)
            {
                return;
            }

            // Calculate all the mid-points of the line segments
            //----------------------
            double x12 = (x1 + x2) / 2;
            double y12 = (y1 + y2) / 2;
            double x23 = (x2 + x3) / 2;
            double y23 = (y2 + y3) / 2;
            double x123 = (x12 + x23) / 2;
            double y123 = (y12 + y23) / 2;

            double dx = x3 - x1;
            double dy = y3 - y1;
            double d = fabs(((x2 - x3) * dy - (y2 - y3) * dx));
            double da;

            if (d > curve_collinearity_epsilon)
            {
                // Regular case
                //-----------------
                if (d * d <= m_distance_tolerance_square * (dx * dx + dy * dy))
                {
                    // If the curvature doesn't exceed the distance_tolerance value
                    // we tend to finish subdivisions.
                    //----------------------
                    if (m_angle_tolerance < curve_angle_tolerance_epsilon)
                    {
                        m_points.Add(new point_d((float)x123, (float)y123));
                        return;
                    }

                    // Angle & Cusp Condition
                    //----------------------
                    da = fabs(atan2(y3 - y2, x3 - x2) - atan2(y2 - y1, x2 - x1));
                    if (da >= pi) da = 2 * pi - da;

                    if (da < m_angle_tolerance)
                    {
                        // Finally we can stop the recursion
                        //----------------------
                        m_points.Add(new point_d((float)x123, (float)y123));
                        return;
                    }
                }
            }
            else
            {
                // Collinear case
                //------------------
                da = dx * dx + dy * dy;
                if (da == 0)
                {
                    d = calc_sq_distance(x1, y1, x2, y2);
                }
                else
                {
                    d = ((x2 - x1) * dx + (y2 - y1) * dy) / da;
                    if (d > 0 && d < 1)
                    {
                        // Simple collinear case, 1---2---3
                        // We can leave just two endpoints
                        return;
                    }
                    if (d <= 0) d = calc_sq_distance(x2, y2, x1, y1);
                    else if (d >= 1) d = calc_sq_distance(x2, y2, x3, y3);
                    else d = calc_sq_distance(x2, y2, x1 + d * dx, y1 + d * dy);
                }
                if (d < m_distance_tolerance_square)
                {
                    m_points.Add(new point_d((float)x2, (float)y2));
                    return;
                }
            }

            // Continue subdivision
            //----------------------
            recursive_bezier(x1, y1, x12, y12, x123, y123, level + 1);
            recursive_bezier(x123, y123, x23, y23, x3, y3, level + 1);
        }

        double m_approximation_scale;
        double m_distance_tolerance_square;
        double m_angle_tolerance;
        uint m_count;
        List<point_d> m_points = new();
    };

    //-------------------------------------------------------------curve4_points
    struct curve4_points
    {
        double[] cp = new double[8];

        public curve4_points() { }

        public curve4_points(double x1, double y1,
                             double x2, double y2,
                             double x3, double y3,
                             double x4, double y4)
        {
            cp[0] = x1; cp[1] = y1; cp[2] = x2; cp[3] = y2;
            cp[4] = x3; cp[5] = y3; cp[6] = x4; cp[7] = y4;
        }

        public void init(double x1, double y1,
                         double x2, double y2,
                         double x3, double y3,
                         double x4, double y4)
        {
            cp[0] = x1; cp[1] = y1; cp[2] = x2; cp[3] = y2;
            cp[4] = x3; cp[5] = y3; cp[6] = x4; cp[7] = y4;
        }

        public double this[int i]
        {
            get
            {
                return cp[i];
            }
            set
            {
                cp[i] = value;
            }
        }
    };

    //-------------------------------------------------------------curve4_inc
    class curve4_inc
    {
        public curve4_inc()
        {
            m_num_steps = 0;
            m_step = 0;
            m_scale = 1.0;
        }

        public curve4_inc(double x1, double y1,
                          double x2, double y2,
                          double x3, double y3,
                          double x4, double y4)
        {
            m_num_steps = 0;
            m_step = 0;
            m_scale = 1.0;
            init(x1, y1, x2, y2, x3, y3, x4, y4);
        }

        public curve4_inc(curve4_points cp)
        {
            m_num_steps = 0;
            m_step = 0;
            m_scale = 1.0;
            init(cp[0], cp[1], cp[2], cp[3], cp[4], cp[5], cp[6], cp[7]);
        }

        public void reset() { m_num_steps = 0; m_step = -1; }

        public void init(double x1, double y1,
                  double x2, double y2,
                  double x3, double y3,
                  double x4, double y4)
        {
            m_start_x = x1;
            m_start_y = y1;
            m_end_x = x4;
            m_end_y = y4;

            double dx1 = x2 - x1;
            double dy1 = y2 - y1;
            double dx2 = x3 - x2;
            double dy2 = y3 - y2;
            double dx3 = x4 - x3;
            double dy3 = y4 - y3;

            double len = (sqrt(dx1 * dx1 + dy1 * dy1) +
                          sqrt(dx2 * dx2 + dy2 * dy2) +
                          sqrt(dx3 * dx3 + dy3 * dy3)) * 0.25 * m_scale;

            m_num_steps = uround(len);

            if (m_num_steps < 4)
            {
                m_num_steps = 4;
            }

            double subdivide_step = 1.0 / m_num_steps;
            double subdivide_step2 = subdivide_step * subdivide_step;
            double subdivide_step3 = subdivide_step * subdivide_step * subdivide_step;

            double pre1 = 3.0 * subdivide_step;
            double pre2 = 3.0 * subdivide_step2;
            double pre4 = 6.0 * subdivide_step2;
            double pre5 = 6.0 * subdivide_step3;

            double tmp1x = x1 - x2 * 2.0 + x3;
            double tmp1y = y1 - y2 * 2.0 + y3;

            double tmp2x = (x2 - x3) * 3.0 - x1 + x4;
            double tmp2y = (y2 - y3) * 3.0 - y1 + y4;

            m_saved_fx = m_fx = x1;
            m_saved_fy = m_fy = y1;

            m_saved_dfx = m_dfx = (x2 - x1) * pre1 + tmp1x * pre2 + tmp2x * subdivide_step3;
            m_saved_dfy = m_dfy = (y2 - y1) * pre1 + tmp1y * pre2 + tmp2y * subdivide_step3;

            m_saved_ddfx = m_ddfx = tmp1x * pre4 + tmp2x * pre5;
            m_saved_ddfy = m_ddfy = tmp1y * pre4 + tmp2y * pre5;

            m_dddfx = tmp2x * pre5;
            m_dddfy = tmp2y * pre5;

            m_step = m_num_steps;
        }

        public void init(curve4_points cp)
        {
            init(cp[0], cp[1], cp[2], cp[3], cp[4], cp[5], cp[6], cp[7]);
        }

        public void approximation_method(curve_approximation_method_e _) { }
        public curve_approximation_method_e approximation_method() { return curve_inc; }

        public void approximation_scale(double s) { m_scale = s; }
        public double approximation_scale() { return m_scale; }

        public void angle_tolerance(double _) { }
        public double angle_tolerance() { return 0.0; }

        public void cusp_limit(double _) { }
        double cusp_limit() { return 0.0; }

        public void rewind(uint path_id)
        {
            if (m_num_steps == 0)
            {
                m_step = -1;
                return;
            }
            m_step = m_num_steps;
            m_fx = m_saved_fx;
            m_fy = m_saved_fy;
            m_dfx = m_saved_dfx;
            m_dfy = m_saved_dfy;
            m_ddfx = m_saved_ddfx;
            m_ddfy = m_saved_ddfy;
        }

        public uint vertex(ref double x, ref double y)
        {
            if (m_step < 0) return path_cmd_stop;
            if (m_step == m_num_steps)
            {
                x = m_start_x;
                y = m_start_y;
                --m_step;
                return path_cmd_move_to;
            }

            if (m_step == 0)
            {
                x = m_end_x;
                y = m_end_y;
                --m_step;
                return path_cmd_line_to;
            }

            m_fx += m_dfx;
            m_fy += m_dfy;
            m_dfx += m_ddfx;
            m_dfy += m_ddfy;
            m_ddfx += m_dddfx;
            m_ddfy += m_dddfy;

            x = m_fx;
            y = m_fy;
            --m_step;
            return path_cmd_line_to;
        }

        int m_num_steps;
        int m_step;
        double m_scale;
        double m_start_x;
        double m_start_y;
        double m_end_x;
        double m_end_y;
        double m_fx;
        double m_fy;
        double m_dfx;
        double m_dfy;
        double m_ddfx;
        double m_ddfy;
        double m_dddfx;
        double m_dddfy;
        double m_saved_fx;
        double m_saved_fy;
        double m_saved_dfx;
        double m_saved_dfy;
        double m_saved_ddfx;
        double m_saved_ddfy;
    };

    //-------------------------------------------------------------curve4_div
    class curve4_div
    {
        public curve4_div()
        {
            m_approximation_scale = 1.0;
            m_angle_tolerance = 0.0;
            m_cusp_limit = 0.0;
            m_count = 0;
        }

        public curve4_div(double x1, double y1,
                          double x2, double y2,
                          double x3, double y3,
                          double x4, double y4)
        {
            m_approximation_scale = 1.0;
            m_angle_tolerance = 0.0;
            m_cusp_limit = 0.0;
            m_count = 0;
            init(x1, y1, x2, y2, x3, y3, x4, y4);
        }

        public curve4_div(curve4_points cp)
        {
            m_approximation_scale = 1.0;
            m_angle_tolerance = 0.0;
            m_count = 0;
            init(cp[0], cp[1], cp[2], cp[3], cp[4], cp[5], cp[6], cp[7]);
        }

        public void reset() { m_points.Clear(); m_count = 0; }

        public void init(double x1, double y1,
                         double x2, double y2,
                         double x3, double y3,
                         double x4, double y4)
        {
            m_points.Clear();
            m_distance_tolerance_square = 0.5 / m_approximation_scale;
            m_distance_tolerance_square *= m_distance_tolerance_square;
            bezier(x1, y1, x2, y2, x3, y3, x4, y4);
            m_count = 0;
        }

        public void init(curve4_points cp)
        {
            init(cp[0], cp[1], cp[2], cp[3], cp[4], cp[5], cp[6], cp[7]);
        }

        void approximation_method(curve_approximation_method_e _) { }
        public curve_approximation_method_e approximation_method() { return curve_div; }

        public void approximation_scale(double s) { m_approximation_scale = s; }
        public double approximation_scale() { return m_approximation_scale; }

        public void angle_tolerance(double a) { m_angle_tolerance = a; }
        public double angle_tolerance() { return m_angle_tolerance; }

        public void cusp_limit(double v)
        {
            m_cusp_limit = (v == 0.0) ? 0.0 : pi - v;
        }

        public double cusp_limit()
        {
            return (m_cusp_limit == 0.0) ? 0.0 : pi - m_cusp_limit;
        }

        public void rewind(uint _)
        {
            m_count = 0;
        }

        public uint vertex(ref double x, ref double y)
        {
            if (m_count >= m_points.Count) return path_cmd_stop;
            int p = (int)m_count++;
            x = m_points[p].X;
            y = m_points[p].Y;
            return (m_count == 1) ? path_cmd_move_to : path_cmd_line_to;
        }

        void bezier(double x1, double y1,
                    double x2, double y2,
                    double x3, double y3,
                    double x4, double y4)
        {
            m_points.Add(new point_d((float)x1, (float)y1));
            recursive_bezier(x1, y1, x2, y2, x3, y3, x4, y4, 0);
            m_points.Add(new point_d((float)x4, (float)y4));
        }

        void recursive_bezier(double x1, double y1,
                              double x2, double y2,
                              double x3, double y3,
                              double x4, double y4,
                              uint level)
        {
            if (level > curve_recursion_limit)
            {
                return;
            }

            // Calculate all the mid-points of the line segments
            //----------------------
            double x12 = (x1 + x2) / 2;
            double y12 = (y1 + y2) / 2;
            double x23 = (x2 + x3) / 2;
            double y23 = (y2 + y3) / 2;
            double x34 = (x3 + x4) / 2;
            double y34 = (y3 + y4) / 2;
            double x123 = (x12 + x23) / 2;
            double y123 = (y12 + y23) / 2;
            double x234 = (x23 + x34) / 2;
            double y234 = (y23 + y34) / 2;
            double x1234 = (x123 + x234) / 2;
            double y1234 = (y123 + y234) / 2;


            // Try to approximate the full cubic curve by a single straight line
            //------------------
            double dx = x4 - x1;
            double dy = y4 - y1;

            double d2 = fabs(((x2 - x4) * dy - (y2 - y4) * dx));
            double d3 = fabs(((x3 - x4) * dy - (y3 - y4) * dx));
            double da1, da2, k;

            switch (((d2 > curve_collinearity_epsilon ? 1 : 0) << 1) +
                    (d3 > curve_collinearity_epsilon ? 1 : 0))
            {
                case 0:
                    // All collinear OR p1==p4
                    //----------------------
                    k = dx * dx + dy * dy;
                    if (k == 0)
                    {
                        d2 = calc_sq_distance(x1, y1, x2, y2);
                        d3 = calc_sq_distance(x4, y4, x3, y3);
                    }
                    else
                    {
                        k = 1 / k;
                        da1 = x2 - x1;
                        da2 = y2 - y1;
                        d2 = k * (da1 * dx + da2 * dy);
                        da1 = x3 - x1;
                        da2 = y3 - y1;
                        d3 = k * (da1 * dx + da2 * dy);
                        if (d2 > 0 && d2 < 1 && d3 > 0 && d3 < 1)
                        {
                            // Simple collinear case, 1---2---3---4
                            // We can leave just two endpoints
                            return;
                        }
                        if (d2 <= 0) d2 = calc_sq_distance(x2, y2, x1, y1);
                        else if (d2 >= 1) d2 = calc_sq_distance(x2, y2, x4, y4);
                        else d2 = calc_sq_distance(x2, y2, x1 + d2 * dx, y1 + d2 * dy);

                        if (d3 <= 0) d3 = calc_sq_distance(x3, y3, x1, y1);
                        else if (d3 >= 1) d3 = calc_sq_distance(x3, y3, x4, y4);
                        else d3 = calc_sq_distance(x3, y3, x1 + d3 * dx, y1 + d3 * dy);
                    }
                    if (d2 > d3)
                    {
                        if (d2 < m_distance_tolerance_square)
                        {
                            m_points.Add(new point_d((float)x2, (float)y2));
                            return;
                        }
                    }
                    else
                    {
                        if (d3 < m_distance_tolerance_square)
                        {
                            m_points.Add(new point_d((float)x3, (float)y3));
                            return;
                        }
                    }
                    break;

                case 1:
                    // p1,p2,p4 are collinear, p3 is significant
                    //----------------------
                    if (d3 * d3 <= m_distance_tolerance_square * (dx * dx + dy * dy))
                    {
                        if (m_angle_tolerance < curve_angle_tolerance_epsilon)
                        {
                            m_points.Add(new point_d((float)x23, (float)y23));
                            return;
                        }

                        // Angle Condition
                        //----------------------
                        da1 = fabs(atan2(y4 - y3, x4 - x3) - atan2(y3 - y2, x3 - x2));
                        if (da1 >= pi) da1 = 2 * pi - da1;

                        if (da1 < m_angle_tolerance)
                        {
                            m_points.Add(new point_d((float)x2, (float)y2));
                            m_points.Add(new point_d((float)x3, (float)y3));
                            return;
                        }

                        if (m_cusp_limit != 0.0)
                        {
                            if (da1 > m_cusp_limit)
                            {
                                m_points.Add(new point_d((float)x3, (float)y3));
                                return;
                            }
                        }
                    }
                    break;

                case 2:
                    // p1,p3,p4 are collinear, p2 is significant
                    //----------------------
                    if (d2 * d2 <= m_distance_tolerance_square * (dx * dx + dy * dy))
                    {
                        if (m_angle_tolerance < curve_angle_tolerance_epsilon)
                        {
                            m_points.Add(new point_d((float)x23, (float)y23));
                            return;
                        }

                        // Angle Condition
                        //----------------------
                        da1 = fabs(atan2(y3 - y2, x3 - x2) - atan2(y2 - y1, x2 - x1));
                        if (da1 >= pi) da1 = 2 * pi - da1;

                        if (da1 < m_angle_tolerance)
                        {
                            m_points.Add(new point_d((float)x2, (float)y2));
                            m_points.Add(new point_d((float)x3, (float)y3));
                            return;
                        }

                        if (m_cusp_limit != 0.0)
                        {
                            if (da1 > m_cusp_limit)
                            {
                                m_points.Add(new point_d((float)x2, (float)y2));
                                return;
                            }
                        }
                    }
                    break;

                case 3:
                    // Regular case
                    //-----------------
                    if ((d2 + d3) * (d2 + d3) <= m_distance_tolerance_square * (dx * dx + dy * dy))
                    {
                        // If the curvature doesn't exceed the distance_tolerance value
                        // we tend to finish subdivisions.
                        //----------------------
                        if (m_angle_tolerance < curve_angle_tolerance_epsilon)
                        {
                            m_points.Add(new point_d((float)x23, (float)y23));
                            return;
                        }

                        // Angle & Cusp Condition
                        //----------------------
                        k = atan2(y3 - y2, x3 - x2);
                        da1 = fabs(k - atan2(y2 - y1, x2 - x1));
                        da2 = fabs(atan2(y4 - y3, x4 - x3) - k);
                        if (da1 >= pi) da1 = 2 * pi - da1;
                        if (da2 >= pi) da2 = 2 * pi - da2;

                        if (da1 + da2 < m_angle_tolerance)
                        {
                            // Finally we can stop the recursion
                            //----------------------
                            m_points.Add(new point_d((float)x23, (float)y23));
                            return;
                        }

                        if (m_cusp_limit != 0.0)
                        {
                            if (da1 > m_cusp_limit)
                            {
                                m_points.Add(new point_d((float)x2, (float)y2));
                                return;
                            }

                            if (da2 > m_cusp_limit)
                            {
                                m_points.Add(new point_d((float)x3, (float)y3));
                                return;
                            }
                        }
                    }
                    break;
            }

            // Continue subdivision
            //----------------------
            recursive_bezier(x1, y1, x12, y12, x123, y123, x1234, y1234, level + 1);
            recursive_bezier(x1234, y1234, x234, y234, x34, y34, x4, y4, level + 1);
        }

        double m_approximation_scale;
        double m_distance_tolerance_square;
        double m_angle_tolerance;
        double m_cusp_limit;
        uint m_count;
        List<point_d> m_points = new();
    };


    //-----------------------------------------------------------------curve3
    class curve3
    {
        public curve3()
        {
            m_approximation_method = curve_div;
        }

        public curve3(double x1, double y1,
                      double x2, double y2,
                      double x3, double y3)
        {
            m_approximation_method = curve_div;
            init(x1, y1, x2, y2, x3, y3);
        }

        public void reset()
        {
            m_curve_inc.reset();
            m_curve_div.reset();
        }

        public void init(double x1, double y1,
                  double x2, double y2,
                  double x3, double y3)
        {
            if (m_approximation_method == curve_inc)
            {
                m_curve_inc.init(x1, y1, x2, y2, x3, y3);
            }
            else
            {
                m_curve_div.init(x1, y1, x2, y2, x3, y3);
            }
        }

        public void approximation_method(curve_approximation_method_e v)
        {
            m_approximation_method = v;
        }

        public curve_approximation_method_e approximation_method()
        {
            return m_approximation_method;
        }

        public void approximation_scale(double s)
        {
            m_curve_inc.approximation_scale(s);
            m_curve_div.approximation_scale(s);
        }

        double approximation_scale()
        {
            return m_curve_inc.approximation_scale();
        }

        public void angle_tolerance(double a)
        {
            m_curve_div.angle_tolerance(a);
        }

        double angle_tolerance()
        {
            return m_curve_div.angle_tolerance();
        }

        public void cusp_limit(double v)
        {
            m_curve_div.cusp_limit(v);
        }

        public double cusp_limit()
        {
            return m_curve_div.cusp_limit();
        }

        public void rewind(uint path_id)
        {
            if (m_approximation_method == curve_inc)
            {
                m_curve_inc.rewind(path_id);
            }
            else
            {
                m_curve_div.rewind(path_id);
            }
        }

        public uint vertex(ref double x, ref double y)
        {
            if (m_approximation_method == curve_inc)
            {
                return m_curve_inc.vertex(ref x, ref y);
            }
            return m_curve_div.vertex(ref x, ref y);
        }

        curve3_inc m_curve_inc = new();
        curve3_div m_curve_div = new();
        curve_approximation_method_e m_approximation_method;
    };


    //-----------------------------------------------------------------curve4
    class curve4
    {
        public curve4()
        {
            m_approximation_method = curve_div;
        }
        public curve4(double x1, double y1,
                      double x2, double y2,
                      double x3, double y3,
                      double x4, double y4)
        {
            m_approximation_method = curve_div;
            init(x1, y1, x2, y2, x3, y3, x4, y4);
        }

        public curve4(curve4_points cp)
        {
            m_approximation_method = curve_div;
            init(cp[0], cp[1], cp[2], cp[3], cp[4], cp[5], cp[6], cp[7]);
        }

        public void reset()
        {
            m_curve_inc.reset();
            m_curve_div.reset();
        }

        public void init(double x1, double y1,
                         double x2, double y2,
                         double x3, double y3,
                         double x4, double y4)
        {
            if (m_approximation_method == curve_inc)
            {
                m_curve_inc.init(x1, y1, x2, y2, x3, y3, x4, y4);
            }
            else
            {
                m_curve_div.init(x1, y1, x2, y2, x3, y3, x4, y4);
            }
        }

        public void init(curve4_points cp)
        {
            init(cp[0], cp[1], cp[2], cp[3], cp[4], cp[5], cp[6], cp[7]);
        }

        public void approximation_method(curve_approximation_method_e v) { m_approximation_method = v; }

        public curve_approximation_method_e approximation_method() { return m_approximation_method; }

        public void approximation_scale(double s)
        {
            m_curve_inc.approximation_scale(s);
            m_curve_div.approximation_scale(s);
        }
        public double approximation_scale() { return m_curve_inc.approximation_scale(); }

        public void angle_tolerance(double v) { m_curve_div.angle_tolerance(v); }
        public double angle_tolerance() { return m_curve_div.angle_tolerance(); }

        public void cusp_limit(double v) { m_curve_div.cusp_limit(v); }

        public double cusp_limit() { return m_curve_div.cusp_limit(); }

        public void rewind(uint path_id)
        {
            if (m_approximation_method == curve_inc)
            {
                m_curve_inc.rewind(path_id);
            }
            else
            {
                m_curve_div.rewind(path_id);
            }
        }

        public uint vertex(ref double x, ref double y)
        {
            if (m_approximation_method == curve_inc)
            {
                return m_curve_inc.vertex(ref x, ref y);
            }
            return m_curve_div.vertex(ref x, ref y);
        }

        curve4_inc m_curve_inc = new();
        curve4_div m_curve_div = new();
        curve_approximation_method_e m_approximation_method;
    };

}
