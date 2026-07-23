using CombatAI.API.Core;
using MEC;
using System.Collections.Generic;
using Exiled.API.Features;
using UnityEngine;
using UnityEngine.AI;
using RelativePositioning;
using PlayerRoles.FirstPersonControl;

namespace CombatAI.API.Module
{
    public class Chaser : Core.BaseCombatAIModule
    {
        private CoroutineHandle followLoop;
        private IFpcRole fpc;

        public Player CurrentTarget;

        public float PathUpdateFrequency = 0.5f;   // 경로/목적지 재계산 주기
        public float MoveTickFrequency = 0.2f;    // 도착판정 주기

        public float KitingRange = 7f;
        public float KitingTolerance = 1.5f;
        public int KiteSampleCount = 12;
        public float MeleeRetreatStep = 2f;
        public LayerMask PathfindMask = LayerMasks.CharacterCollision;// 벽/문 레이어 — 환경 맞게 직접 세팅
        public LayerMask BulletMask = LayerMasks.Bullet;

        private bool IsMelee => KitingRange < 3f;

        public Chaser(BaseCombatAI owner) : base(owner)
        {
            fpc = owner.Npc.RoleManager.CurrentRole as IFpcRole;
        }

        public void StartChase()
        {
            if (!followLoop.IsRunning)
                followLoop = Timing.RunCoroutine(FollowCoroutine());
        }

        public void StopChase()
        {
            Timing.KillCoroutines(followLoop);
        }

        public IEnumerator<float> FollowCoroutine()
        {
            while (true)
            {
                if (CurrentTarget != null && Owner?.Npc != null && CurrentTarget.IsAlive)
                {
                    if (fpc.FpcModule == null)
                    {
                        fpc = Owner.Npc.RoleManager.CurrentRole as IFpcRole;
                        yield return Timing.WaitForSeconds(PathUpdateFrequency);
                        continue;
                    }
                    Vector3 selfPos = Owner.Npc.Position;
                    Vector3 targetPos = CurrentTarget.Position;
                    float dist = Vector3.Distance(selfPos, targetPos);

                    Vector3 destination = targetPos;
                    bool needMove = true;

                    if (dist < KitingRange - KitingTolerance)
                    {
                        if (IsMelee)
                        {
                            Vector3 back = (selfPos - targetPos).normalized;
                            destination = selfPos + back * MeleeRetreatStep;
                        }
                        else if (!TryFindKitePosition(out destination))
                        {
                            // 유효 후보 없음 → 단순 후퇴 폴백
                            Vector3 back = (selfPos - targetPos).normalized;
                            destination = selfPos + back * MeleeRetreatStep;
                        }
                    }
                    else if (dist > KitingRange + KitingTolerance)
                    {
                        destination = targetPos;
                    }
                    else
                    {
                        needMove = false;
                        fpc.FpcModule.Motor.ReceivedPosition = new RelativePosition(selfPos + new Vector3(Random.value * 5 - 2.5f, 0, Random.value * 5 - 2.5f));
                        yield return Timing.WaitForSeconds(MoveTickFrequency);
                        continue;
                    }

                    if (needMove && TryGetWaypoints(destination, out var waypoints))
                    {
                        float timeStamp = Time.time;
                        foreach (Vector3 waypointPosition in waypoints)// Follow waypoints
                        {
                            if (Time.time - timeStamp > 3) break;

                            while (true)
                            {
                                Vector3 direction = waypointPosition - Owner.Npc.Position;
                                direction.y = 0;
                                if (direction.sqrMagnitude > 0)
                                {
                                    fpc.FpcModule.Motor.ReceivedPosition = new RelativePosition(Owner.Npc.Position + direction.normalized);
                                }
                                if (direction.sqrMagnitude < 0.25 || Time.time - timeStamp > 3)
                                {
                                    break;
                                }
                                yield return Timing.WaitForSeconds(MoveTickFrequency);
                            }
                        }
                    }
                }
                yield return Timing.WaitForSeconds(PathUpdateFrequency);
            }
        }

        private bool TryFindKitePosition(out Vector3 result)
        {
            Vector3 self = Owner.Npc.Position;
            Vector3 targetPos = CurrentTarget.Position;
            Vector3 origin = targetPos;

            result = targetPos;

            float bestInRange = float.NegativeInfinity;
            bool foundInRange = false;

            float bestAny = float.NegativeInfinity;
            bool foundAny = false;
            Vector3 anyResult = self;

            for (int i = 0; i < KiteSampleCount; i++)
            {
                float angle = (360f / KiteSampleCount) * i;
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

                // 적 중심으로 후보 점
                Vector3 candidate = targetPos + dir * KitingRange;

                // 레이캐스트 하기
                if (Physics.Raycast(origin, dir, out var rayHit, KitingRange, BulletMask))
                {
                    candidate = rayHit.point + rayHit.normal;
                    candidate.y = targetPos.y;
                }

                if (!NavMesh.SamplePosition(candidate, out var navHit, 2f, NavMesh.AllAreas))
                    continue;

                float distToTarget = Vector3.Distance(navHit.position, targetPos);
                float priority = -Vector3.Distance(self, navHit.position);  // 가까울수록 큼

                // 전체 후보 중 AI와 최근접
                if (priority > bestAny)
                {
                    bestAny = priority;
                    anyResult = navHit.position;
                    foundAny = true;
                }

                // 조건부 최근접
                if (distToTarget <= KitingRange && priority > bestInRange)
                {
                    bestInRange = priority;
                    result = navHit.position;
                    foundInRange = true;
                }
            }

            if (foundInRange) return true;
            if (foundAny) { result = anyResult; return true; }
            return false;
        }

        private bool TryGetWaypoints(Vector3 targetPosition, out Vector3[] waypoints)
        {
            waypoints = null;
            if (Owner == null || Owner.Npc == null)
                return false;

            if (!NavMesh.SamplePosition(Owner.Npc.Position, out var startHit, 25f, NavMesh.AllAreas))
                return false;
            if (!NavMesh.SamplePosition(targetPosition, out var targetHit, 25f, NavMesh.AllAreas))
                return false;

            var navMeshPath = new NavMeshPath();
            if (NavMesh.CalculatePath(startHit.position, targetHit.position, NavMesh.AllAreas, navMeshPath))
            {
                if (navMeshPath.status == NavMeshPathStatus.PathComplete)
                {
                    waypoints = navMeshPath.corners;
                    return true;
                }
            }
            AntiStuck();
            return false;
        }

        private void AntiStuck()
        {
            Owner.Npc.Position += Vector3.up * 2;
        }

        public override void Destroy()
        {
            StopChase();
        }
    }
}