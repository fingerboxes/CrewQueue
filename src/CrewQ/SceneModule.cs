using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using FingerboxLib;
using KSPPluginFramework;

using System.Reflection;

namespace CrewQ
{
    public abstract class SceneModule : MonoBehaviourExtended
    {
        private bool _remapCrewActive;
        public bool RemapCrew { get; set; }

        protected override void Update()
        {
            if (_remapCrewActive)
            {     
                // TODO - Set our 'vacation roster' to an invalid RosterStatus here, if settings tells us not to allow them to be assigned. Like this;
                // HighLogic.CurrentGame.CrewRoster.Crew.First(c => c.name == "Bob Kerman").rosterStatus = (ProtoCrewMember.RosterStatus)9001;

                // Refresh the listing.
                CMAssignmentDialog.Instance.RefreshCrewLists(CMAssignmentDialog.Instance.GetManifest(), true, true);
            }
            else
            {
                // TODO - 
            } 
        }

        public void CleanManifest()
        {          
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

            

            CMAssignmentDialog.Instance.RefreshCrewLists(vcm, true, true);
        }

        public void HijackUIElements()
        {
            BTButton[] buttons = MiscUtils.GetFields<BTButton>(CMAssignmentDialog.Instance);
            Logging.Debug("Found some buttons, Count:" + buttons.Length);

            buttons[0].AddInputDelegate(new EZInputDelegate(onFillButton));
            Logging.Debug("Fill Button Hijack potentially successful");
        }

        public void onFillButton(ref POINTER_INFO ptr)
        {
            if (ptr.evt == POINTER_INFO.INPUT_EVENT.TAP)
            {
                Logging.Debug("Fill Button Hijack Success!");
                if (CrewQData.instance.DoCustomAssignment)
                {
                    // TODO - replace the crew with our selections
                }
                else
                {
                    CMAssignmentDialog.Instance.ButtonFill(ref ptr);
                }
            }
        }
    }
}
