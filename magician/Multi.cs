namespace Magician
{
    public class Multi : Single
    {
        private List<Multi> constituents;
        public Multi(params Multi[] cs)
        {
            pos = new double[]{0, 0};
            constituents = new List<Multi> {};
            constituents.AddRange(cs);
        }

        public override void Draw(ref IntPtr renderer, double xOffset=0, double yOffset=0)
        {
            foreach (Multi c in constituents)
            {
                c.Draw(ref renderer);
            }
        }
    }
}