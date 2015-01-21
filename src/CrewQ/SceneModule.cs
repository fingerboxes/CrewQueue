using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using FingerboxLib;
using KSPPluginFramework;

namespace CrewQ
{
    public abstract class SceneModule : MonoBehaviourExtended
    {

        public void CleanManifest()
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

            CMAssignmentDialog.Instance.RefreshCrewLists(vcm, true, true);
        }

        public void HijackFillButton()
        {
            BTButton[] buttons = MiscUtils.GetFields<BTButton>(CMAssignmentDialog.Instance);
            Logging.Debug("Found some buttons, Count:" + buttons.Length);

            buttons[0].AddInputDelegate(new EZInputDelegate(onFillButton));
            Logging.Debug("Fill Button Hijack potentially successful");
        }

        public void onFillButton(ref POINTER_INFO ptr)
        {
            Logging.Debug("Fill Button Hijack Success!");

            if (ptr.evt == POINTER_INFO.INPUT_EVENT.TAP)
            {
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
