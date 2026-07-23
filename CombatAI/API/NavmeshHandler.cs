using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CombatAI.API
{
    public static class NavmeshHandler
    {
        private static NavMeshDataInstance _instance;

        public static float AgentRadius { get; set; } = 0.17f;
        public static float AgentHeight { get; set; } = 1f;
        public static float AgentClimb { get; set; } = 0.24f;
        public static float AgentSlope { get; set; } = 45f;
        public static string[] Layers { get; set; } = Layers.coll

        public static bool IsBuilt => _instance.valid;

        public static void AddNavmesh(Bounds bounds)
        {
            var markups = new List<NavMeshBuildMarkup>();
            var sources = new List<NavMeshBuildSource>();

            int mask = LayerMask.GetMask(Layers);
            NavMeshBuilder.CollectSources(bounds, mask,
                NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources);

            var settings = NavMesh.CreateSettings();
            settings.agentRadius = AgentRadius;
            settings.agentHeight = AgentHeight;
            settings.agentClimb = AgentClimb;
            settings.agentSlope = AgentSlope;

            var data = NavMeshBuilder.BuildNavMeshData(settings, sources, bounds,
                Vector3.zero, Quaternion.identity);
            _instance = NavMesh.AddNavMeshData(data);
        }

        public static void ClearNavmesh()
        {
            NavMesh.RemoveAllNavMeshData();
            _instance = default;
        }

        public static void RecreateNavmesh(Bounds bounds)
        {
            ClearNavmesh();
            AddNavmesh(bounds);
        }
    }
}