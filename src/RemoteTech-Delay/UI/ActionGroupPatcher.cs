using System.Collections.Generic;
using KSP.UI.Screens.Flight;
using System.Collections;
using UnityEngine;

namespace RemoteTech.Delay.UI
{
    /// <summary>
    ///     Class responsible for patching GUI action group buttons (like gear, brakes, light, abort, etc.).
    /// </summary>
    public static class ActionGroupPatcher
    {
        /// <summary>
        ///     Action groups corresponding to the GUI buttons we want to hook / patch.
        /// </summary>
        private static readonly KSPActionGroup[] PatchedActionGroups = { KSPActionGroup.Gear, KSPActionGroup.Brakes, KSPActionGroup.Light, KSPActionGroup.Abort };
        // TODO: Capture action group keypresses

        /// <summary>
        ///     Hook flight action group buttons: gear, brakes, light and abort buttons.
        /// </summary>
        public static void Patch()
        {
            var buttons = CollectActionGroupToggleButtons(PatchedActionGroups);
            foreach (var actionGroupToggleButton in buttons)
            {
                // Remove default KSP listener (otherwise we can't delay anything)
                actionGroupToggleButton.toggle.onToggle.RemoveListener(actionGroupToggleButton.SetToggle);

                // Set our hook-ups
                var actionGroup = actionGroupToggleButton.group;
                actionGroupToggleButton.toggle.onToggle.AddListener(() => ActivateActionGroup(actionGroup));
            }
        }

        /// <summary>
        ///     Get action groups buttons depending on their group.
        /// </summary>
        /// <param name="actionGroups">The action group(s) in which the buttons should be.</param>
        /// <returns>A list of action ActionGroupToggleButton buttons, filtered by actionGroups parameter.</returns>
        private static IEnumerable<ActionGroupToggleButton> CollectActionGroupToggleButtons(KSPActionGroup[] actionGroups)
        {
            List<ActionGroupToggleButton> buttons = new List<ActionGroupToggleButton>();

            // get all action group buttons
            var actionGroupToggleButtons = UnityEngine.Object.FindObjectsOfType<ActionGroupToggleButton>();
            // filter them to only get the buttons that have a group in the actionGroups array
            for (var i = 0; i < actionGroupToggleButtons.Length; i++)
            {
                for (var j = 0; j < actionGroups.Length; j++)
                {
                    if (actionGroupToggleButtons[i].group == actionGroups[j])
                        buttons.Add(actionGroupToggleButtons[i]);
                }
            }

            return buttons;
        }

        /// <summary>
        ///     Called when an action group button, from the KSP GUI, is pressed.
        /// </summary>
        /// <param name="ag">The action group that was pressed.</param>
        private static void ActivateActionGroup(KSPActionGroup ag)
        {
            var vessel = FlightGlobals.ActiveVessel;
            if (vessel != null && vessel.connection != null) // TODO: check here if there's a flight computer
            {
                //TODO: enqueue command to flight computer
                DelayInvoke(ag, vessel.connection.SignalDelay);
            }
            else if (vessel == null /*|| vessel.HasLocalControl*/)
            {
                if (!FlightGlobals.ready)
                    return;

                if (!FlightGlobals.ActiveVessel.IsControllable)
                    return;

                // check if EVA or not (as we removed the default KSP listener).
                if (!FlightGlobals.ActiveVessel.isEVA)
                {
                    FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(ag);
                }
                else // it's an EVA
                {
                    switch (ag)
                    {
                        case KSPActionGroup.RCS:
                            FlightGlobals.ActiveVessel.evaController.ToggleJetpack();
                            break;
                        case KSPActionGroup.Light:
                            FlightGlobals.ActiveVessel.evaController.ToggleLamp();
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Enqueue pending ActionGroup action with delay time
        /// </summary>
        public static void DelayInvoke(KSPActionGroup ag, double waitTime)
        {
            HighLogic.fetch.StartCoroutine(DelayInvokeCoroutine(ag, waitTime));
        }

        /// <summary>
        ///     Execute pending ActionGRoup action at end of delay
        /// </summary>
        private static IEnumerator DelayInvokeCoroutine(KSPActionGroup ag, double waitTime)
        {
            yield return new WaitForSeconds((float)waitTime);
            FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(ag);
        }
    }
}
