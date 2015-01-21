using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;
using KSPPluginFramework;
using FingerboxLib;

namespace CrewQ
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class EditorScene : SceneModule
    {
        bool rootExists, cleanedRoot;

        protected override void Awake()
        {
            GameEvents.onEditorShipModified.Add(onEditorShipModified);
            GameEvents.onEditorScreenChange.Add(onEditorScreenChanged);
        }

        protected override void Update()
        {            
            if (rootExists && !cleanedRoot)
            {
                CleanManifest();
                cleanedRoot = true;
            }
            else if (!rootExists && cleanedRoot)
            {
                cleanedRoot = false;
            }
        }

        // KSP Events
        protected void onEditorShipModified(ShipConstruct ship)
        {
            Logging.Debug("Entering Method");
            rootExists = CheckRoot(ship);                       
        }

        protected void onEditorScreenChanged(EditorScreen screen)
        {
            if (screen == EditorScreen.Crew)
            {
                HijackFillButton();
            }
        }

        // Our methods

        protected bool CheckRoot(ShipConstruct ship)
        {
            return (ship != null) && (ship.Count > 0);
        }
    }
}