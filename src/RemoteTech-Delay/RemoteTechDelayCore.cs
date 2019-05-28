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

            DelayQuadrant = new DelayQuadrant();

            if(HighLogic.fetch != null)
                signalDelayParams = HighLogic.CurrentGame.Parameters.CustomParams<RemoteTechSignalDelayParams>();

            // Register for game events
            GameEvents.onShowUI.Add(UiOn);
            GameEvents.onHideUI.Add(UiOff);
            GameEvents.OnGameSettingsApplied.Add(onSettingsChanged);
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

            // Degister game events
            GameEvents.onShowUI.Remove(UiOn);
            GameEvents.onHideUI.Remove(UiOff);
            GameEvents.OnGameSettingsApplied.Remove(onSettingsChanged);
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
    }
}