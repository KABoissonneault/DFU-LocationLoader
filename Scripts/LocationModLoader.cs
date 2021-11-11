using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features

namespace LocationLoader
{
    public class LocationModLoader : MonoBehaviour
    {
        public static Mod mod { get; private set; }

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            // Get mod
            mod = initParams.Mod;
            // Add script to the scene.

            new GameObject("LocationLoader").AddComponent<LocationLoader>();
            mod.IsReady = true;

            const int ladderModelId = 41409;
            PlayerActivate.RegisterCustomActivation(mod, ladderModelId, OnLadderActivated);
        }

        static void OnLadderActivated(RaycastHit hit)
        {
            // Player must not be inside a building (already handled by DFU)
            PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;
            if (playerEnterExit.IsPlayerInsideBuilding)
                return;

            Transform ladderTransform = hit.transform;
            Transform prefabTransform = ladderTransform.parent;
            GameObject prefabObject = prefabTransform.gameObject;
            LocationData data = prefabObject.GetComponent<LocationData>();

            PlayerMotor playerMotor = GameManager.Instance.PlayerMotor;
            bool foundBottom = data.FindClosestMarker(EditorMarkerTypes.LadderBottom, playerMotor.transform.position, out Vector3 bottomMarker);
            bool foundTop = data.FindClosestMarker(EditorMarkerTypes.LadderTop, playerMotor.transform.position, out Vector3 topMarker);

            Vector2 ladderPlanarPos = new Vector2(ladderTransform.position.x, ladderTransform.position.z);
            Vector2 bottomMarkerPlanarPos = new Vector2(bottomMarker.x, bottomMarker.z);
            Vector2 topMarkerPlanarPos = new Vector2(topMarker.x, topMarker.z);

            float bottomPlanarDistance = Vector2.Distance(ladderPlanarPos, bottomMarkerPlanarPos);
            float topPlanarDistance = Vector2.Distance(ladderPlanarPos, topMarkerPlanarPos);

            const float MaxMarkerDistance = PlayerActivate.DefaultActivationDistance * 2;
            foundBottom = foundBottom && bottomPlanarDistance < MaxMarkerDistance;
            foundTop = foundTop && topPlanarDistance < MaxMarkerDistance;

            float bottomDistance = Vector3.Distance(playerMotor.transform.position, bottomMarker);
            float topDistance = Vector3.Distance(playerMotor.transform.position, topMarker);

            // Teleport to top marker
            if (foundTop && (!foundBottom || topDistance > bottomDistance))
            {
                playerMotor.transform.position = topMarker;
                playerMotor.FixStanding();
            }
            else if (foundBottom)
            {
                playerMotor.transform.position = bottomMarker;
                playerMotor.FixStanding();
            }
        }
    }
}