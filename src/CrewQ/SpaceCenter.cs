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
    class SpaceCenter : SceneModule
    {
        protected override void Awake()
        {
            GameEvents.onGUILaunchScreenVesselSelected.Add(onVesselSelected);
        }

        private void onVesselSelected(ShipTemplate shipTemplate)
        {
            CleanManifest();
            HijackFillButton();
        }
    }
}
