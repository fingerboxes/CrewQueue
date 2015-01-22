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
        private bool _remapCrewActive;
        public bool RemapCrew { get; set; }

        protected override void Update()
        {
            if (_remapCrewActive)
            {   
                CrewQ.instance.SuppressVacationingCrew();

                // Refresh the listing.
                CMAssignmentDialog.Instance.RefreshCrewLists(CMAssignmentDialog.Instance.GetManifest(), true, true);
            }
            else
            {
                CrewQ.instance.UnsuppressVacationingCrew();
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
                        if (CrewQDataStore.instance.DoCustomAssignment)
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
                if (CrewQDataStore.instance.DoCustomAssignment)
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
