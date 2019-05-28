namespace RemoteTech.Delay
{
    public class RemoteTechSignalDelayParams : GameParameters.CustomParameterNode
    {
        [GameParameters.CustomParameterUI("Enabled", toolTip = "ON: Apply signal delay to flight control actions due to light speed over distance.\nOFF: Instant execution of flight control actions.")]
        public bool SignalDelayEnabled = true;

        public override string DisplaySection
        {
            get
            {
                return "RemoteTech";
            }
        }

        public override GameParameters.GameMode GameMode
        {
            get
            {
                return GameParameters.GameMode.ANY;
            }
        }

        public override bool HasPresets
        {
            get
            {
                return false;
            }
        }

        public override string Section
        {
            get
            {
                return "RemoteTech";
            }
        }

        public override int SectionOrder
        {
            get
            {
                return 3;
            }
        }

        public override string Title
        {
            get
            {
                return "Signal Delay";
            }
        }
    }
}
