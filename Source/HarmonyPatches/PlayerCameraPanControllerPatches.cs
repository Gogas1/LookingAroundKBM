using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;

namespace LookingAroundKBM.HarmonyPatches {
    [HarmonyPatch(typeof(PlayerCameraPanController))]
    internal static class PlayerCameraPanControllerPatches {

        static FieldInfo f_lastLookDirection = AccessTools.Field(typeof(PlayerCameraPanController), "lastLookDirection");
        static FieldInfo f_lookFurtherDirection = AccessTools.Field(typeof(PlayerCameraPanController), "lookFurtherDirection");

        static bool isLooking = false;
        static Vector2 anchorMousePosition = Vector2.zero;

        static float maxOffset = 96f * 1;
        static float horizontalModifier = 1f;
        static float verticalModifier = 1f;

        [HarmonyPatch(nameof(PlayerCameraPanController.LookFurtherCheck))]
        [HarmonyPrefix]
        internal static bool LookFurtherCheck_Prefix(PlayerCameraPanController __instance, ref Vector2 __result, ref PlayerGamePlayActionSet actions) {
            if(actions.CameraUp.Value != 0f ||
                actions.CameraRight.Value != 0f ||
                actions.CameraLeft.Value != 0f ||
                actions.CameraDown.Value != 0f) {
                return true;
            }

            var lookFurtherDirection = (Vector2)f_lookFurtherDirection.GetValue(__instance);
            f_lastLookDirection.SetValue(__instance, lookFurtherDirection);

            Vector2 vector = Vector2.zero;
            if (LookingAroundKBM.cameraControllKeybind.Value.IsPressed() && Player.i.CanPanCamera) {
                if(!isLooking) {
                    isLooking = true;
                    anchorMousePosition = Input.mousePosition;
                }

                var width = Screen.width;
                var height = Screen.height;

                var verticalFactor = (Input.mousePosition.y - height / 2) / (height / 2);
                var horizontalFactor = (Input.mousePosition.x - width / 2) / (width / 2);

                var offset = anchorMousePosition - (Vector2)Input.mousePosition;
                vector += 1 * new Vector2(horizontalFactor * maxOffset * horizontalModifier, verticalFactor * maxOffset * verticalModifier);
            }
            else {
                isLooking = false;
            }

            f_lookFurtherDirection.SetValue(__instance, vector);
            __instance.Pan(vector);

            __result = vector;


            return false;
        }
    }
}
