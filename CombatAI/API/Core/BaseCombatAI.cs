using System;
using Exiled.API.Features;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using System.Collections.Generic;
using UnityEngine;

namespace CombatAI.API.Core
{
    public abstract class BaseCombatAI
    {
        public Npc Npc { get; }

        public abstract RoleTypeId RoleType { get; }
        public abstract string Name { get; }

        private readonly Dictionary<Type, BaseCombatAIModule> modules = new Dictionary<Type, BaseCombatAIModule>();

        public T TryAddModule<T>() where T : BaseCombatAIModule
        {
            T module = TryGetModule<T>();
            if (module != null) return module;

            module = (T)Activator.CreateInstance(typeof(T), this);
            modules[typeof(T)] = module;
            return module;
        }

        public T TryGetModule<T>() where T : BaseCombatAIModule
            => modules.TryGetValue(typeof(T), out var m) ? (T)m : null;

        public void TryDestroyModule<T>() where T : BaseCombatAIModule
        {
            T module = TryGetModule<T>();
            if (module == null) return;
            module.Destroy();
            modules.Remove(typeof(T));
        }

        protected BaseCombatAI(Vector3 spawnPosition)
        {
            Npc = Exiled.API.Features.Npc.Spawn(Name, RoleType, spawnPosition);
            Npc.ReferenceHub.playerStats.OnThisPlayerDied += OnDead;    
        }

        protected void Destroy()
        {
            Npc.ReferenceHub.playerStats.OnThisPlayerDied -= OnDead;
            Npc.Destroy();
        }

        protected abstract void OnDead(DamageHandlerBase handler);
    }
}