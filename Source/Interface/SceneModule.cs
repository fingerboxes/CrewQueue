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

namespace CrewQ.Interface
{
    public abstract class SceneModule : MonoBehaviourExtended
    {
        private bool remapCrewActive, releaseTrigger;
        public bool RemapCrew
        {
            get
            {
                return remapCrewActive;
            }

            set
            {
                if (value == false)
                {
                    releaseTrigger = true;
                }
                remapCrewActive = value;
            }
        }

        // Monobehaviour Methods
        sealed protected override void Update()
        {
            if (remapCrewActive)
            {
                CrewQ.Instance.SuppressCrew();
            }
            else if (releaseTrigger)
            {
                CrewQ.Instance.ReleaseCrew();
                releaseTrigger = false;
            }

            OnUpdate();
        }

        virtual protected void OnUpdate() { }

        public void CleanManifest()
        {
            if (CMAssignmentDialog.Instance != null && CrewQData.Instance != null)
            {
                VesselCrewManifest originalVesselManifest = CMAssignmentDialog.Instance.GetManifest();
                IList<PartCrewManifest> partCrewManifests = originalVesselManifest.GetCrewableParts();

                CrewQ.Instance.ClearAssignedBuffer();

                if (partCrewManifests != null && partCrewManifests.Count > 0)
                {
                    PartCrewManifest partManifest = partCrewManifests[0];

                    if (CrewQData.Instance.settingRemoveDefaultCrews || CrewQData.Instance.settingDoCustomAssignment)
                    {
                        foreach (ProtoCrewMember crewMember in partManifest.GetPartCrew())
                        {
                            if (crewMember != null)
                            {
                                // Clean the root part
                                partManifest.RemoveCrewFromSeat(partManifest.GetCrewSeat(crewMember));
                            }
                        }
                        if (CrewQData.Instance.settingDoCustomAssignment)
                        {
                            IEnumerable<ProtoCrewMember> newCrew = CrewQ.Instance.GetCrewForPart(partManifest.PartInfo.partPrefab, true);

                            partManifest.AddCrewToOpenSeats(newCrew);
                        }
                    }
                }

                CMAssignmentDialog.Instance.RefreshCrewLists(originalVesselManifest, true, true);
            }
        }

        public void RemapFillButton()
        {
            BTButton[] buttons = MiscUtils.GetFields<BTButton>(CMAssignmentDialog.Instance);
            buttons[0].AddInputDelegate(new EZInputDelegate(OnFillButton));
        }

        public void OnFillButton(ref POINTER_INFO eventPointer)
        {
            if (eventPointer.evt == POINTER_INFO.INPUT_EVENT.TAP)
            {
                Logging.Debug("Fill Button Pressed");
                if (CrewQData.Instance.settingDoCustomAssignment)
                {
                    VesselCrewManifest vesselManifest = CMAssignmentDialog.Instance.GetManifest();

                    foreach (PartCrewManifest partManifest in vesselManifest)
                    {
                        bool firstPart = (partManifest == vesselManifest.GetCrewableParts()[0]);

                        partManifest.AddCrewToOpenSeats(CrewQ.Instance.GetCrewForPart(partManifest.PartInfo.partPrefab, firstPart));
                    }
                }
                else
                {
                    CMAssignmentDialog.Instance.ButtonFill(ref eventPointer);
                }
            }
        }
    }
}
