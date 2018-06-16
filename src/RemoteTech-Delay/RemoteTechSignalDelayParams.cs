using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteTech.Delay
{
    public class RemoteTechSignalDelayParams : GameParameters.CustomParameterNode
    {
        [GameParameters.CustomStringParameterUI("", autoPersistance = false, lines = 3)]
        public string description = "To disable signal delay, delete 'RemoteTech-Delay' from RemoteTech's main folder";

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
