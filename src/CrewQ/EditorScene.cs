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
    class EditorScene : MonoBehaviourExtended
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
                Logging.Debug("Root seems to exist, and has not been cleaned");

                VesselCrewManifest vcm = CMAssignmentDialog.Instance.GetManifest();
                Logging.Debug("Obtained vcm");

                List<PartCrewManifest> pcmList = vcm.GetCrewableParts();
                Logging.Debug("Obtained pcm");

                if (pcmList != null && pcmList.Count > 0)
                {
                    PartCrewManifest pcm = pcmList[0];
                    foreach (ProtoCrewMember cm in pcm.GetPartCrew())
                    {
                        if (cm != null)
                        {
                            // Clean the root part
                            pcm.RemoveCrewFromSeat(pcm.GetCrewSeat(cm));

                            // TODO - replace the crew with our selections
                            if (CrewQData.instance.DoCustomAssignment)
                            {

                            }
                        }
                    }
                }
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
                BTButton[] buttons = MiscUtils.GetFields<BTButton>(CMAssignmentDialog.Instance);
                Logging.Debug("Found some buttons, Count:" + buttons.Length);

                buttons[0].AddInputDelegate(new EZInputDelegate(onFillButton));
                Logging.Debug("Fill Button Hijack potentially successful");
            }
        }

        public void onFillButton(ref POINTER_INFO ptr)
        {
            Logging.Debug("Fill Button Hijack Success!");

            if (ptr.evt == POINTER_INFO.INPUT_EVENT.TAP) 
            {
                if (CrewQData.instance.DoCustomAssignment)
                {

                }
                else
                {
                    CMAssignmentDialog.Instance.ButtonFill(ref ptr);
                }
            }           
        }

        // Our methods

        protected bool CheckRoot(ShipConstruct ship)
        {
            return (ship != null) && (ship.Count > 0);
        }
    }
}