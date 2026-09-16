using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using KeybindsPlus.Helpers;
using CSFramework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;
using CSVector3 = FFXIVClientStructs.FFXIV.Common.Math.Vector3;

namespace KeybindsPlus.Interop
{
    public static unsafe class TargetingInterop
    {
        private const float MaxDistanceToTarget = 49.5f; // Maximum distance the game can target someone legitimately
        private const float LineOfSightOffset = 2f; // Offset to raise the raycast origin above the ground (borrowed from BetterTargetingSystem)
        private const uint AutoAttackActionId = 142; // The action ID for auto-attack

        public static bool CanAttack(IGameObject target)
        {
            if (target.Address == nint.Zero) return false;
            return ActionManager.CanUseActionOnTarget(AutoAttackActionId, (GameObject*)target.Address);
        }
        public static float GetDistanceToPlayer(IGameObject player, IGameObject target)
        {
            var distance = Vector3.Distance(player.Position, target.Position) - target.HitboxRadius;
            return MathF.Max(0f, distance);
        }
        public static bool InLineOfSight(IGameObject target, bool useCamera = true)
        {
            var framework = CSFramework.Instance();
            if (framework == null)
            {
                LogHelper.LogError("CS Framework returned null, cannot check line of sight.");
                return false;
            }

            CSVector3 sourcePos;
            if (useCamera)
            {
                var cameraManager = CameraManager.Instance();
                if (cameraManager == null || cameraManager->CurrentCamera == null)
                {
                    LogHelper.LogWarning("CameraManager.Instance() or camera returned null, cannot check line of sight.");
                    return false;
                }
                sourcePos = cameraManager->CurrentCamera->Object.Position;
            }
            else
            {
                // Use player position for raycast
                var player = Plugin.ObjectTable.LocalPlayer;
                if (player == null) return false;

                var playerObject = (GameObject*)player.Address;
                if (playerObject == null)
                {
                    LogHelper.LogWarning("CS GameObject for local player returned null, cannot check line of sight.");
                    return false;
                }

                sourcePos = playerObject->Position;
                sourcePos.Y += LineOfSightOffset;
            }

            var targetObject = (GameObject*)target.Address;
            if (targetObject == null)
            {
                LogHelper.LogWarning("CS GameObject for target returned null, cannot check line of sight.");
                return false;
            }
            var targetPos = targetObject->Position;
            targetPos.Y += LineOfSightOffset;

            var direction = targetPos - sourcePos;
            var distance = direction.Magnitude;

            direction = direction.Normalized;
            var originVect = new Vector3(sourcePos.X, sourcePos.Y, sourcePos.Z);
            var directionVect = new Vector3(direction.X, direction.Y, direction.Z);

            RaycastHit hit;
            var flags = stackalloc int[] { 0x4000, 0, 0x4000, 0 }; // Borrowed from BetterTargetingSystem
            var isLineOfSightBlocked = framework->BGCollisionModule->RaycastMaterialFilter(
                &hit,
                &originVect,
                &directionVect,
                distance,
                1,
                flags
                );
            return !isLineOfSightBlocked;
        }
        public static bool WithinRange(IGameObject player, IGameObject target) =>
            GetDistanceToPlayer(player, target) <= MaxDistanceToTarget;
    }
}
