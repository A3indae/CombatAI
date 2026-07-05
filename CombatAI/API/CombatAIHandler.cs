using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using CombatAI.API.Core;

namespace CombatAI.API
{
    public static class CombatAIHandler
    {
        public static Dictionary<int, BaseCombatAI> List { get; private set; } = new Dictionary<int, BaseCombatAI>();

        public static T AddCombatAI<T>(Vector3 pos) where T : BaseCombatAI
        {
            T unit = (T)Activator.CreateInstance(typeof(T), pos);
            List.Add(unit.Npc.Id, unit);
            return unit;
        }

        public static void RemoveById(int id)
        {
            if (!List.TryGetValue(id, out var unit)) return;
            unit.Destroy();
            List.Remove(id);
        }

        public static void ClearCombatAI()
        {
            foreach (var unit in List.Values.ToList())
            unit.Destroy();
            List.Clear();
        }
    }
}