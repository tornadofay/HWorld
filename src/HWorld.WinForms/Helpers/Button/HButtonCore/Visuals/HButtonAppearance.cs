namespace HWorld.WinForms.Helpers.Button.HButtonCore.Visuals
{
    public readonly struct HButtonAppearance
    {
        public readonly bool RoundButton;
        public readonly bool RoundStyle; 
        public readonly int Radius;
        public readonly HButtonCustomColors CustomColors;


        public HButtonAppearance(bool round, bool roundStyle, int radius, HButtonCustomColors custom)
        {  RoundButton = round; RoundStyle = roundStyle; Radius = radius; CustomColors = custom; }
    }
}
