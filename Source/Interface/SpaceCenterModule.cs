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
using KSPPluginFramework;
using FingerboxLib;

namespace CrewQueue.Interface
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class SpaceCenterModule : SceneModule
    {
        private bool astronautComplexSpawned;

        protected override void Awake()
        {
            base.Awake();
            GameEvents.onGUILaunchScreenVesselSelected.Add(onVesselSelected);
            GameEvents.onGUIAstronautComplexSpawn.Add(onGUIAstronautComplexSpawn);
            GameEvents.onGUIAstronautComplexDespawn.Add(onGUIAstronautComplexDespawn);
            GameEvents.onGUILaunchScreenSpawn.Add(onGUILaunchScreenSpawn);
            GameEvents.onGUILaunchScreenDespawn.Add(onGUILaunchScreenDespawn);
        }

        protected override void Update()
        {
            if (astronautComplexSpawned)
            {
                Logging.Debug("AC is spawned...");
                IEnumerable<CrewItemContainer> crewItemContainers = GameObject.FindObjectsOfType<CrewItemContainer>().Where(x => x.GetCrewRef().rosterStatus == ProtoCrewMember.RosterStatus.Available);

                foreach (CrewItemContainer crewContainer in crewItemContainers)
                {
                    if (crewContainer.GetCrewRef().type == ProtoCrewMember.KerbalType.Crew && crewContainer.GetCrewRef().IsOnVacation())
                    {
                        // TODO - This needs attention
                        Logging.Debug("relabeling: " + crewContainer.GetName());
                        string label = "Ready In: " + Utilities.GetFormattedTime(crewContainer.GetCrewRef().VacationExpiry() - Planetarium.GetUniversalTime());
                        crewContainer.SetLabel(label);
                    }
                }
            }
        }

        private void onGUIAstronautComplexDespawn()
        {
            astronautComplexSpawned = false;
        }

        private void onGUIAstronautComplexSpawn()
        {
            astronautComplexSpawned = true;
        }

        private void onGUILaunchScreenSpawn(GameEvents.VesselSpawnInfo info)
        {
            CrewQueueRoster.HideVacationingCrew();
        }

        private void onGUILaunchScreenDespawn()
        {
            CrewQueueRoster.RestoreVacationingCrew();
        }

        private void onVesselSelected(ShipTemplate shipTemplate)
        {
            CleanManifest();
            RemapFillButton();
        }
    }
}
