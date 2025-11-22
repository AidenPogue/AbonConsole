using System;
using UnityEngine;

namespace Terasievert.AbonConsole.BuiltInCommands
{
    [ConsoleMemberPrefix("LookingAt")]
    public static class LookingAtConsoleMembers
    {
        /// <summary>
        /// Function that will get the current camera of the player. Must be set to use the commands.
        /// </summary>
        public static Func<Camera> GetCameraFunc { get; set; }

        private static RaycastHit DoRaycast()
        {
            if (GetCameraFunc is null)
            {
                throw new InvalidOperationException(nameof(GetCameraFunc) + " must be set for these commands to work. This isn't your fault! Tells the devs!");
            }

            var cam = GetCameraFunc();

            if (cam == null)
            {
                throw new InvalidOperationException("Couldn't get the player camera. Are you in game?");
            }

            var tansform = GetCameraFunc().transform;
            Physics.Raycast(tansform.position, tansform.forward, out RaycastHit hitInfo, float.PositiveInfinity, int.MaxValue);
            return hitInfo;
        }

        [ConsoleMember("Casts a ray from the camera and returns the collider it hit.")]
        public static Collider Collider => DoRaycast().collider;

        [ConsoleMember("Casts a ray from the camera and returns the GameObject it hit.")]
        public static GameObject GameObject => DoRaycast().collider.gameObject;

        [ConsoleMember("Casts a ray from the camera and tries returns the position it hit.")]
        public static Vector3 Position => DoRaycast().point;
    }
}