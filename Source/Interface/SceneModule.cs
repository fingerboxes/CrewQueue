/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2015 Alexander Taylor
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using FingerboxLib;
using KSPPluginFramework;

using System.Reflection;

namespace CrewQueue.Interface
{
    public abstract class SceneModule : MonoBehaviourExtended
    {
        public void CleanManifest()
        {
            if (CMAssignmentDialog.Instance != null)
            {
                VesselCrewManifest originalVesselManifest = CMAssignmentDialog.Instance.GetManifest();
                IList<PartCrewManifest> partCrewManifests = originalVesselManifest.GetCrewableParts();

                if (partCrewManifests != null && partCrewManifests.Count > 0)
                {
                    PartCrewManifest partManifest = partCrewManifests[0];
                    foreach (ProtoCrewMember crewMember in partManifest.GetPartCrew())
                    {
                        if (crewMember != null)
                        {
                            // Clean the root part
                            partManifest.RemoveCrewFromSeat(partManifest.GetCrewSeat(crewMember));
                        }
                    }
                    if (CrewQueueSettings.Instance.AssignCrews)
                    {
                        partManifest.AddCrewToOpenSeats(CrewQueue.Instance.GetCrewForPart(partManifest.PartInfo.partPrefab, new List<ProtoCrewMember>(), true));
                    }
                }

                CMAssignmentDialog.Instance.RefreshCrewLists(originalVesselManifest, true, true);
            }
        }
        
        public void RemapFillButton()
        {
            BTButton[] buttons = MiscUtils.GetFields<BTButton>(CMAssignmentDialog.Instance);
            buttons[0].RemoveInputDelegate(new EZInputDelegate(CMAssignmentDialog.Instance.ButtonFill));
            buttons[0].AddInputDelegate(new EZInputDelegate(OnFillButton));
        }

        public void OnFillButton(ref POINTER_INFO eventPointer)
        {
            if (eventPointer.evt == POINTER_INFO.INPUT_EVENT.TAP)
            {
                VesselCrewManifest manifest = CMAssignmentDialog.Instance.GetManifest();

                Logging.Debug("Attempting to fill...");

                foreach (PartCrewManifest partManifest in manifest.GetCrewableParts())
                {
                    Logging.Debug("Attempting to fill part - " + partManifest.PartInfo.name);
                    bool vets = (partManifest == manifest.GetCrewableParts()[0]) ? true : false;
                    partManifest.AddCrewToOpenSeats(CrewQueue.Instance.GetCrewForPart(partManifest.PartInfo.partPrefab, manifest.GetAllCrew(false), vets));
                }

                CMAssignmentDialog.Instance.RefreshCrewLists(manifest, true, true);
            }
        }
    }    
}
