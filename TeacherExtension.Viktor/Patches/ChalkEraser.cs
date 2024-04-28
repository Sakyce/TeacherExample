using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TeacherExtension.Viktor.Patches
{
    internal class ViktorTilePollutionManager : MonoBehaviour
    {
        private Dictionary<Cell, float> pollutedCells = new Dictionary<Cell, float>();
        private EnvironmentController ec;
        void Awake()
        {
            ec = GetComponent<EnvironmentController>();
        }
        void Update()
        {
            var newValues = new Dictionary<Cell, float>();
            foreach (var cell in pollutedCells.Keys)
            {
                if (pollutedCells.ContainsKey(cell) && pollutedCells[cell] > 0)
                {
                    newValues[cell] = pollutedCells[cell] - Time.deltaTime * ec.EnvironmentTimeScale;
                }
            }
            pollutedCells = newValues;
        }
        public bool IsCellPolluted(Cell cell)
        {
            return pollutedCells.ContainsKey(cell) && pollutedCells[cell] > 0;
        }
        public void PolluteCell(Cell cell, float duration)
        {
            pollutedCells[cell] = duration;
        }
    }
    [HarmonyPatch(typeof(ChalkEraser), nameof(ChalkEraser.Use))]
    internal class ChalkEraserPatch
    {
        static void Postfix(ChalkEraser __instance, bool __result)
        {
            var pollutionManager = __instance.ec.GetComponent<ViktorTilePollutionManager>();
            if (pollutionManager != null)
            {
                Debug.Log("pollution manager found");
                var gridPosition = IntVector2.GetGridPosition(__instance.pos);
                pollutionManager.PolluteCell(__instance.ec.CellFromPosition(gridPosition), __instance.setTime);
            }
        }
    }
}
