using CombatAI.API.Core;
using MEC;
using System.Collections.Generic;
using Exiled.API.Features;
using UnityEngine;
using UnityEngine.AI;
using RelativePositioning;
using PlayerRoles.FirstPersonControl;
using Exiled.API.Features.Toys;
using static PlayerRoles.Spectating.SpectatableModuleBase;

namespace CombatAI.API.Module
{
    public class Looker : Core.BaseCombatAIModule
    {
        private CoroutineHandle lookLoop;
        private IFpcRole fpc;

        public Player CurrentTarget;

        public float LookTickFrequency = 0.2f;

        public Looker(BaseCombatAI owner) : base(owner)
        {
            fpc = owner.Npc.RoleManager.CurrentRole as IFpcRole;
        }

        public void StartLook()
        {
            if (!lookLoop.IsRunning)
                lookLoop = Timing.RunCoroutine(LookCoroutine());
        }

        public void StopLook()
        {
            Timing.KillCoroutines(lookLoop);
        }

        public void LookTarget()
        {
            if (CurrentTarget != null && Owner?.Npc != null && CurrentTarget.IsAlive)
            {
                if (fpc.FpcModule == null)
                {
                    fpc = Owner.Npc.RoleManager.CurrentRole as IFpcRole;
                    return;
                }

                Vector3 p0 = Owner.Npc.CameraTransform.position;
                Vector3 p1 = CurrentTarget.Position;

                Vector3 lookDirection = p1 - p0;
                if (lookDirection.sqrMagnitude > 0)
                {
                    fpc.FpcModule.MouseLook.LookAtDirection(lookDirection.normalized);

                    Primitive prim0 = Primitive.Create(primitiveType: PrimitiveType.Cube, position: p1, scale: new Vector3(0.1f, 0.1f, 0.1f), spawn: false);
                    prim0.Collidable = false;
                    prim0.Color = new Color(1, 0, 0);
                    prim0.Spawn();
                    Timing.CallDelayed(0.2f, () =>
                    {
                        prim0.Destroy();
                    });

                    Primitive prim1 = Primitive.Create(primitiveType: PrimitiveType.Cube, position: p0, scale: new Vector3(0.1f, 0.1f, 0.1f), spawn: false);
                    prim1.Collidable = false;
                    prim1.Color = new Color(0, 1, 0);
                    prim1.Spawn();
                    Timing.CallDelayed(0.2f, () =>
                    {
                        prim1.Destroy();
                    });
                }
            }
        }

        private IEnumerator<float> LookCoroutine()
        {
            while (true)
            {
                if (CurrentTarget != null && Owner?.Npc != null && CurrentTarget.IsAlive)
                {
                    if (fpc.FpcModule == null)
                    {
                        fpc = Owner.Npc.RoleManager.CurrentRole as IFpcRole;
                        yield return Timing.WaitForSeconds(LookTickFrequency);
                        continue;
                    }

                    Vector3 p0 = Owner.Npc.CameraTransform.position;
                    Vector3 p1 = CurrentTarget.Position;

                    Vector3 lookDirection = p1 - p0;
                    if (lookDirection.sqrMagnitude > 0)
                    {
                        fpc.FpcModule.MouseLook.LookAtDirection(lookDirection.normalized);

                        Primitive prim0 = Primitive.Create(primitiveType: PrimitiveType.Cube, position: p1, scale: new Vector3(0.1f, 0.1f, 0.1f), spawn: false);
                        prim0.Collidable = false;
                        prim0.Color = new Color(1, 0, 0);
                        prim0.Spawn();
                        Timing.CallDelayed(0.2f, () =>
                        {
                            prim0.Destroy();
                        });

                        Primitive prim1 = Primitive.Create(primitiveType: PrimitiveType.Cube, position: p0, scale: new Vector3(0.1f, 0.1f, 0.1f), spawn: false);
                        prim1.Collidable = false;
                        prim1.Color = new Color(0, 1, 0);
                        prim1.Spawn();
                        Timing.CallDelayed(0.2f, () =>
                        {
                            prim1.Destroy();
                        });
                    }
                }
                yield return Timing.WaitForSeconds(LookTickFrequency);
            }
        }

        public override void Destroy()
        {
            StopLook();
        }
    }
}