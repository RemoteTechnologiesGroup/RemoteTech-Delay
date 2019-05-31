using System;
using RemoteTech.Common;
using RemoteTech.Delay.UI;
using UnityEngine;

namespace RemoteTech.Delay
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class RemoteTechDelayCoreFlight : RemoteTechDelayCore
    {
        public new void Start()
        {
            base.Start();
        }
    }

    /// <summary>
    ///     Individual RemoteTech instance responsible for signal delay
    /// </summary>
    public abstract class RemoteTechDelayCore : MonoBehaviour
    {
        /// <summary>
        ///     Main class instance.
        /// </summary>
        public static RemoteTechDelayCore Instance { get; protected set; }

        /// <summary>
        ///     UI overlay used to display and handle the status quadrant (time delay) and the flight computer button.
        /// </summary>
        public DelayQuadrant DelayQuadrant { get; protected set; }

        /// <summary>
        ///     Methods can register to this event to be called during the OnGUI() method of the Unity engine (GUI Rendering engine
        ///     phase).
        /// </summary>
        public event Action OnGuiUpdate = delegate { };

        /// <summary>
        ///     RemoteTech signal-delay parameters
        /// </summary>
        public RemoteTechSignalDelayParams signalDelayParams;

        // handle the F2 key GUI show / hide
        private bool _guiVisible = true;

        /// <summary>
        ///     Called by Unity engine during initialization phase.
        ///     Only ever called once.
        /// </summary>
        public void Start()
        {
            Logging.Debug($"RemoteTech-Delay Starting. Scene: {HighLogic.LoadedScene}");

            // Destroy the Core instance if != null or if RemoteTech is disabled
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            // Cache setting parameters and register for setting-change event
            signalDelayParams = HighLogic.CurrentGame.Parameters.CustomParams<RemoteTechSignalDelayParams>();
            GameEvents.OnGameSettingsApplied.Add(onSettingsChanged);

            // Create signal delay interface window
            DelayQuadrant = new DelayQuadrant();
            GameEvents.onShowUI.Add(UiOn);
            GameEvents.onHideUI.Add(UiOff);

            // Hook up action groups and Part Action Menu
            ActionGroupPatcher.Patch();
            GameEvents.onPartActionUICreate.Add(OnPartActionUiCreate);
            GameEvents.onPartActionUIDismiss.Add(OnPartActionUiDismiss);
        }

        /// <summary>
        ///     Called by the Unity engine during the Decommissioning phase of the Engine.
        ///     This is used to clean up everything before quiting.
        /// </summary>
        public void OnDestroy()
        {
            if(DelayQuadrant != null)
                DelayQuadrant = null;

            if (signalDelayParams != null)
                signalDelayParams = null;

            // Deregister custom game events
            GameEvents.onShowUI.Remove(UiOn);
            GameEvents.onHideUI.Remove(UiOff);
            GameEvents.OnGameSettingsApplied.Remove(onSettingsChanged);
            GameEvents.onPartActionUICreate.Remove(OnPartActionUiCreate);
            GameEvents.onPartActionUIDismiss.Remove(OnPartActionUiDismiss);
        }

        /// <summary>
        ///     Called by the Unity engine during the GUI rendering phase.
        ///     Note that OnGUI() is called multiple times per frame in response to GUI events.
        ///     The Layout and Repaint events are processed first, followed by a Layout and keyboard/mouse event for each input
        ///     event.
        /// </summary>
        public void OnGUI()
        {
            if (!_guiVisible)
                return;

            DelayQuadrant?.Draw();
            OnGuiUpdate?.Invoke();
        }

        /// <summary>
        ///     F2 GUI Show / Hide functionality: called when the UI must be displayed.
        /// </summary>
        public void UiOn()
        {
            _guiVisible = true;
        }

        /// <summary>
        ///     F2 GUI Show / Hide functionality: called when the UI must be hidden.
        /// </summary>
        public void UiOff()
        {
            _guiVisible = false;
        }

        /// <summary>
        ///     Reload RemoteTech signal-delay parameters upon setting event firing
        /// </summary>
        private void onSettingsChanged()
        {
            signalDelayParams = HighLogic.CurrentGame.Parameters.CustomParams<RemoteTechSignalDelayParams>();
        }

        /// <summary>
        ///     Hook up Part Action Menu components when Part Action Menu is created
        /// </summary>
        public void OnPartActionUiCreate(Part partForUi)
        {
            // Check if the current scene is not in flight
            if (HighLogic.fetch && !HighLogic.LoadedSceneIsFlight)
                return;

            // Check if the part is actually one from this vessel
            if (partForUi.vessel != FlightGlobals.ActiveVessel)
                return;

            // Hook part action menu
            PartActionMenuPatcher.WrapPartActionEventItem(partForUi, InvokeEvent);
            PartActionMenuPatcher.WrapPartActionFieldItem(partForUi, InvokePartAction);
        }

        /// <summary>
        ///     Remove Part Action Menu hook-ups when Part Action Menu is dismissed
        /// </summary>
        public void OnPartActionUiDismiss(Part partForUi)
        {
            PartActionMenuPatcher.ParsedPartActions.Clear();
        }

        /// <summary>
        ///     Run custom logic when player triggers Base Event of Part Action Menu
        /// </summary>
        private static void InvokeEvent(BaseEvent baseEvent, bool ignoreDelay)
        {
            // note: this gets called when the event is invoked through:
            // PartActionMenuPatcher.Wrapper.Invoke()

            var v = FlightGlobals.ActiveVessel;
            if (v == null || v.isEVA)
            {
                baseEvent.Invoke();
                return;
            }

            if (ignoreDelay)
            {
                baseEvent.Invoke();
            }
            else
            {
                //TODO:enqeue commands
                PartActionMenuPatcher.BaseEventDelayInvoke(baseEvent, v.connection.SignalDelay);
            }
        }

        /// <summary>
        ///     Run custom logic when player triggers Base Field of Part Action Menu
        /// </summary>
        private static void InvokePartAction(BaseField baseField, bool ignoreDelay)
        {
            var field = (baseField as PartActionMenuPatcher.WrappedField);
            if (field == null)
                return;

            var v = FlightGlobals.ActiveVessel;
            if (v == null || v.isEVA)
            {
                field.Invoke();
                return;
            }

            if (ignoreDelay)
            {
                field.Invoke();
            }
            else
            {
                // queue command into FC
                // TODO:enqeue commands
                PartActionMenuPatcher.BaseFieldDelayInvoke(baseField, field.NewValue, v.connection.SignalDelay);
            }
        }
    }
}