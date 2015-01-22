using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using KSPPluginFramework;
using FingerboxLib;

namespace CrewQ
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class SpaceCenterModule : SceneModule
    {
        private bool acSpawned;

        protected override void Awake()
        {
            GameEvents.onGUILaunchScreenVesselSelected.Add(onVesselSelected);
            GameEvents.onGUIAstronautComplexSpawn.Add(onGUIAstronautComplexSpawn);
            GameEvents.onGUIAstronautComplexDespawn.Add(onGUIAstronautComplexDespawn);
        }

        protected override void Update()
        {
            base.Update(); // Important

            // TODO - Change labels of crew on vacation. Find them like this;
            // List<CrewItemContainer> CrewItemContainers = GameObject.FindObjectsOfType<CrewItemContainer>().Where(x => x.GetCrewRef().rosterStatus == ProtoCrewMember.RosterStatus.Available).ToList();
        }

        private void onGUIAstronautComplexDespawn()
        {
            acSpawned = false;
        }

        private void onGUIAstronautComplexSpawn()
        {
            acSpawned = true;
        }

        private void onVesselSelected(ShipTemplate shipTemplate)
        {
            CleanManifest();
            HijackUIElements();
        }
    }
}
