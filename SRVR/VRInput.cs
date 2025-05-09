﻿using System;
using System.Collections.Generic;
using InControl;
using SRVR.Components;
using SRVR.Patches;
using Steamworks;
using Unity.XR.OpenVR;
using UnityEngine;
using Valve.VR;
using InputDevice = InControl.InputDevice;

namespace SRVR
{
    public class VRInput : InputDevice
    {
        public static VRInput Instance;
        public static SRInput.InputMode Mode;
        public static float repauseDelay = 0.0f;

        public static Dictionary<string, SteamVR_Action> actionsForKey;

        public static Dictionary<string, Sprite> leftControllerIcons = new Dictionary<string, Sprite>();
        public static Dictionary<string, Sprite> rightControllerIcons = new Dictionary<string, Sprite>();

        private static Dictionary<SteamVR_Action, Sprite> iconCache = new Dictionary<SteamVR_Action, Sprite>();

        public const float REPAUSE_DELAY = 0.25f;

        public VRInput()
        {
            Instance = this;
            Name = "Virtual Device";
            Meta = "Simulates inputs for SteamVR";
            
            // Add controls for the device
            AddControl(InputControlType.RightTrigger, "Right Trigger");
            AddControl(InputControlType.LeftTrigger, "Left Trigger");
            AddControl(InputControlType.LeftBumper, "Left Bumper");
            AddControl(InputControlType.RightBumper, "Right Bumper");
            
            this.AddControl(InputControlType.LeftStickLeft, "Left Stick Left", 0.2f, 0.9f);
            this.AddControl(InputControlType.LeftStickRight, "Left Stick Right", 0.2f, 0.9f);
            this.AddControl(InputControlType.LeftStickUp, "Left Stick Up", 0.2f, 0.9f);
            this.AddControl(InputControlType.LeftStickDown, "Left Stick Down", 0.2f, 0.9f);
            this.AddControl(InputControlType.RightStickLeft, "Right Stick Left", 0.2f, 0.9f);
            this.AddControl(InputControlType.RightStickRight, "Right Stick Right", 0.2f, 0.9f);
            this.AddControl(InputControlType.RightStickUp, "Right Stick Up", 0.2f, 0.9f);
            this.AddControl(InputControlType.RightStickDown, "Right Stick Down", 0.2f, 0.9f);
            
            this.AddControl(InputControlType.Action1, "Action1");
            this.AddControl(InputControlType.DPadRight, "DPad Right");
            this.AddControl(InputControlType.LeftStickButton, "Left Stick Button");
            this.AddControl(InputControlType.Action3, "Action3");
            this.AddControl(InputControlType.Action2, "Action2");

            actionsForKey = new Dictionary<string, SteamVR_Action>()
            {
                { "Attack", SteamVR_Actions.slimecontrols.shoot },
                { "Vac", SteamVR_Actions.slimecontrols.vac },
                { "NextSlot", SteamVR_Actions.slimecontrols.nextslot },
                { "PrevSlot", SteamVR_Actions.slimecontrols.prevslot },
                { "Jump", SteamVR_Actions.slimecontrols.jump },
                { "Run", SteamVR_Actions.slimecontrols.sprint },
                { "Interact", SteamVR_Actions.slimecontrols.interact },
                { "Burst", SteamVR_Actions.slimecontrols.pulse },
                { "OpenMap", SteamVR_Actions.slimecontrols.map },
                { "Pedia", SteamVR_Actions.slimecontrols.slimepedia },
                { "Light", SteamVR_Actions.slimecontrols.flashlight },
                { "RadarToggle", SteamVR_Actions.slimecontrols.radar },
                { "ToggleGadgetMode", SteamVR_Actions.slimecontrols.gadgetmode },
                { "Menu", SteamVR_Actions.slimecontrols.pause },
                { "Submit", SteamVR_Actions.ui.submit },
                { "AltSubmit", SteamVR_Actions.ui.alt_submit },
                { "Cancel", SteamVR_Actions.ui.cancel },
                { "CloseMap", SteamVR_Actions.ui.close },
                { "MenuTabLeft", SteamVR_Actions.ui.prev_tab },
                { "MenuTabRight", SteamVR_Actions.ui.next_tab },
                { "Unmenu", SteamVR_Actions.ui.close }
            };

            foreach (SteamVR_Action action in actionsForKey.Values)
            {
                if (action is SteamVR_Action_Boolean actionBool)
                    actionBool.onActiveBindingChange += (x, y, z) => GameContext.Instance?.InputDirector?.onKeysChanged?.Invoke();
                else if (action is SteamVR_Action_Single actionSingle)
                    actionSingle.onActiveBindingChange += (x, y, z) => GameContext.Instance?.InputDirector?.onKeysChanged?.Invoke();
                else if (action is SteamVR_Action_Vector2 actionVector)
                    actionVector.onActiveBindingChange += (x, y, z) => GameContext.Instance?.InputDirector?.onKeysChanged?.Invoke();
            }

            SteamVR_Actions.global.Activate();
        }

        public override void Update(ulong updateTick, float deltaTime)
        {
            if (repauseDelay > 0.0f)
            {
                repauseDelay = Mathf.Clamp(repauseDelay - deltaTime, 0, REPAUSE_DELAY);
                return;
            }

            switch (Mode)
            {
                case SRInput.InputMode.DEFAULT:
                    Vector2 move = SteamVR_Actions.slimecontrols.move.GetAxis(SteamVR_Input_Sources.Any);
                    SRInput.Actions.horizontal.UpdateWithValue(move.x, updateTick, deltaTime);
                    SRInput.Actions.vertical.UpdateWithValue(move.y, updateTick, deltaTime);

                    SRInput.Actions.attack.UpdateWithValue(SteamVR_Actions.slimecontrols.shoot.GetAxis(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.vac.UpdateWithValue(SteamVR_Actions.slimecontrols.vac.GetAxis(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.nextSlot.UpdateWithValue(SteamVR_Actions.slimecontrols.nextslot.GetAxis(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.prevSlot.UpdateWithValue(SteamVR_Actions.slimecontrols.prevslot.GetAxis(SteamVR_Input_Sources.Any), updateTick, deltaTime);

                    SRInput.Actions.jump.UpdateWithState(SteamVR_Actions.slimecontrols.jump.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.run.UpdateWithState(SteamVR_Actions.slimecontrols.sprint.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.interact.UpdateWithState(SteamVR_Actions.slimecontrols.interact.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.burst.UpdateWithState(SteamVR_Actions.slimecontrols.pulse.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.openMap.UpdateWithState(SteamVR_Actions.slimecontrols.map.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);

                    if (SteamVR_Actions.slimecontrols.toggle_vacgun.GetStateDown(SteamVR_Input_Sources.Any))
                        HandManager.Instance?.SetVacVisibility(!HandManager.Instance.vacShown);

                    SRInput.Actions.pedia.UpdateWithState(SteamVR_Actions.slimecontrols.slimepedia.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.light.UpdateWithState(SteamVR_Actions.slimecontrols.flashlight.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.radarToggle.UpdateWithState(SteamVR_Actions.slimecontrols.radar.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.toggleGadgetMode.UpdateWithState(SteamVR_Actions.slimecontrols.gadgetmode.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.menu.UpdateWithState(SteamVR_Actions.slimecontrols.pause.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);

                    break;
                case SRInput.InputMode.PAUSE:

                    SRInput.PauseActions.submit.UpdateWithState(SteamVR_Actions.ui.submit.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.PauseActions.altSubmit.UpdateWithState(SteamVR_Actions.ui.alt_submit.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.PauseActions.cancel.UpdateWithState(SteamVR_Actions.ui.cancel.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.PauseActions.closeMap.UpdateWithState(SteamVR_Actions.ui.close.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.PauseActions.menuTabLeft.UpdateWithState(SteamVR_Actions.ui.prev_tab.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.PauseActions.menuTabRight.UpdateWithState(SteamVR_Actions.ui.next_tab.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.PauseActions.unmenu.UpdateWithState(SteamVR_Actions.ui.close.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);

                    float value = SteamVR_Actions.ui.scroll.GetAxis(SteamVR_Input_Sources.Any).y * 5;
                    (value > 0 ? SRInput.PauseActions.menuScrollUp : SRInput.PauseActions.menuScrollDown).UpdateWithValue(Mathf.Abs(value), updateTick, deltaTime);

                    Vector2 navigation = SteamVR_Actions.ui.navigate.GetAxis(SteamVR_Input_Sources.Any);
                    (navigation.x > 0 ? SRInput.PauseActions.menuRight : SRInput.PauseActions.menuLeft).UpdateWithValue(Mathf.Abs(navigation.x), updateTick, deltaTime);
                    (navigation.y > 0 ? SRInput.PauseActions.menuUp : SRInput.PauseActions.menuDown).UpdateWithValue(Mathf.Abs(navigation.y), updateTick, deltaTime);

                    break;
            }

            if (!(SceneContext.Instance?.TimeDirector?.HasPauser() ?? false))
            {
                float rightStickX = SteamVR_Actions.slimecontrols.look.axis.x;
                float snapThreshold = 0.7f; // Stick must be pushed at least 70% to trigger snap turn
                float resetThreshold = 0.2f; // Stick must return below 20% to reset
                int snapAngle = VRConfig.SNAP_TURN_ANGLE; // Degrees per snap turn

                if (VRConfig.SNAP_TURN)
                {
                    if (Mathf.Abs(rightStickX) > snapThreshold && !Patch_vp_FPInput.SnapTriggered)
                    {
                        int snapDirection = rightStickX > 0 ? 1 : -1;
                        Patch_vp_FPInput.AdjustmentDegrees += snapDirection * snapAngle;
                        Patch_vp_FPInput.AdjustmentDegrees %= 360;
                        Patch_vp_FPInput.SnapTriggered = true; // Prevent multiple snaps until stick resets
                    }
                    else if (Mathf.Abs(rightStickX) < resetThreshold) // Stick has returned to center
                    {
                        Patch_vp_FPInput.SnapTriggered = false; // Allow next snap
                    }
                }
                else
                {
                    Patch_vp_FPInput.AdjustmentDegrees += rightStickX * deltaTime * SteamVR.instance.hmd_DisplayFrequency * VRConfig.TURN_SENSITIVITY;
                    Patch_vp_FPInput.AdjustmentDegrees %= 360;
                }
            }
        }

        public static void RegisterCallbacks()
        {
            Instance = new VRInput();
            InputManager.AttachDevice(Instance);
        }

        public static bool GetVRButton(string actionKey, out Sprite result)
        {
            if (actionsForKey.TryGetValue(actionKey, out SteamVR_Action action))
                return GetVRButton(action, out result);

            result = null;
            return false;
        }

        public static bool GetVRButton(SteamVR_Action action, out Sprite result)
        {
            result = null;

            if (action.activeBinding)
            {
                InputBindingInfo_t[] actionInfo;
                if (action is SteamVR_Action_Boolean actionBool)
                    actionInfo = actionBool.GetActionBindingInfo();
                else if (action is SteamVR_Action_Single actionSingle)
                    actionInfo = actionSingle.GetActionBindingInfo();
                else if (action is SteamVR_Action_Vector2 actionVector)
                    actionInfo = actionVector.GetActionBindingInfo();
                else
                {
                    EntryPoint.ConsoleInstance.LogError($"Unimplemented action type: {action.GetType()}");
                    if (iconCache.TryGetValue(action, out Sprite value))
                        result = value;
                    return result == null;
                }

                if (actionInfo.Length <= 0)
                {
                    EntryPoint.ConsoleInstance.LogError($"Action unbound: {action.fullPath}");
                    if (iconCache.TryGetValue(action, out Sprite value))
                        result = value;

                    return result == null;
                }

                // my below comment is completely incorrect for obvious reasons, but it shall be left as a testiment to why you don't program while barely conscious because head ow ow head ow
                Sprite icon; // yells about not being inlined, but I'm fairly sure that'd crash the entire runtime
                if ((actionInfo[0].rchDevicePathName == "/user/hand/left" && VRInput.leftControllerIcons.TryGetValue(actionInfo[0].rchInputPathName, out icon))
                    || (actionInfo[0].rchDevicePathName == "/user/hand/right" && VRInput.rightControllerIcons.TryGetValue(actionInfo[0].rchInputPathName, out icon)))
                {
                    result = icon;
                    iconCache[action] = icon;
                }
                else
                {
                    EntryPoint.ConsoleInstance.LogError($"Input {actionInfo[0].rchDevicePathName}{actionInfo[0].rchInputPathName} does not have a valid icon, what are you doing?");
                    if (iconCache.TryGetValue(action, out Sprite value))
                        result = value;
                    return result != null;
                }
            }
            else if (iconCache.TryGetValue(action, out Sprite value))
                result = value;

            return result != null;
        }
    }
}
