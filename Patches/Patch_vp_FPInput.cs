﻿using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;

namespace SRVR.Patches
{

    [HarmonyPatch(typeof(InputDirector), nameof(InputDirector.UsingGamepad))]
    public static class Patch_InputDirector
    {
        public static bool Prefix(ref bool __result)
        {
            if (!EntryPoint.EnabledVR)
                return true;
            __result = true;
            return false;
        }
    }

    [HarmonyPatch()]
    public static class Patch_vp_FPInput
    {
        public static bool snapTriggered;
        public static Vector3 rotatedPos;

        public static bool UsingVR()
        {
            if (EntryPoint.EnabledVR)
                return true;
            return InputDirector.UsingGamepad();
        }
        public static Vector2 Rotate_V2(Vector2 v, float delta) {
            delta *= Mathf.Deg2Rad;
            return new Vector2(
                v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
            );
            
            
        }

        
        [HarmonyPostfix, HarmonyPatch(typeof(vp_FPInput), nameof(vp_FPInput.InputMove))]
        public static void InputMove(vp_FPInput __instance)
        {
            if (!EntryPoint.EnabledVR)
                return;
            Vector2 vector = new Vector2(SRInput.Actions.horizontal, SRInput.Actions.vertical);
            Vector2 o = (InputDirector.UsingGamepad() ? __instance.ApplyRadialDeadZone(vector, __instance.inputDir.ControllerStickDeadZone) : vector);
        
            UnityEngine.XR.InputDevice head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (head.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot))
            {
                 o = Rotate_V2(o, rot.eulerAngles.z);
                
            }
        
            __instance.FPPlayer.InputMoveVector.Set(o);
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(vp_FPInput), nameof(vp_FPInput.GetMouseLook))]

        static IEnumerable<CodeInstruction> GetMouseLook(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            foreach (var codeInstruction in codeInstructions)
            {
                if (codeInstruction.Calls(AccessTools.Method(typeof(InputDirector),
                        nameof(InputDirector.UsingGamepad))))
                {
                    codeInstruction.operand = AccessTools.Method(typeof(Patch_vp_FPInput), nameof(UsingVR));
                    EntryPoint.ConsoleInstance.Log("Changed: GetMouseLook ");
                }
            }


            return codeInstructions;
        }

        private static float offset = 0f;
        public static float adjustmentDegrees = 0f;
        public static float delay;

        [HarmonyPrefix, HarmonyPatch(typeof(vp_FPCamera), nameof(vp_FPCamera.LateUpdate))]

        public static bool LateUpdate(vp_FPCamera __instance) => !EntryPoint.EnabledVR;

        public static Vector3 HMDPosition;
    }

}