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
using System.Text;
using System.Linq;

using UnityEngine;
using KSPPluginFramework;
using FingerboxLib;

namespace CrewQ
{
    [KSPAddon(KSPAddon.Startup.EveryScene, true)]
    public class CrewQ : MonoBehaviourExtended
    {
        // ITS OVER NINE THOUSAND!!!!111
        private const ProtoCrewMember.RosterStatus VACATION = (ProtoCrewMember.RosterStatus)9001;
        private const double KERBAL_DAY = 21600;

        // Singleton boilerplate
        private static CrewQ _instance;
        public static CrewQ instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<CrewQ>();
                }
                return _instance;
            }
        }

        private bool releaseOnce;
        private double lastMissionTime;

        protected override void Awake()
        {
            Logging.Debug("Loading...");
            DontDestroyOnLoad(this);
            _instance = this;

            GameEvents.onKerbalStatusChange.Add(onKerbalStatusChange);
            GameEvents.onKerbalRemoved.Add(onKerbalRemoved);
            GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
            GameEvents.OnVesselRecoveryRequested.Add(onVesselRecoveryRequested);

            Logging.Debug("Loaded");
        }

        protected override void Update()
        {
            if (releaseOnce)
            {
                Logging.Debug("Releasing crew once, just in case.");
                ReleaseCrew();
                if (CrewQDataStore.instance != null)
                {
                    releaseOnce = false;
                }                
            }
        }

        void onVesselRecoveryRequested(Vessel vessel)
        {
            lastMissionTime = vessel.missionTime;
        }

        void onKerbalStatusChange(ProtoCrewMember kerbal, ProtoCrewMember.RosterStatus oldStatus, ProtoCrewMember.RosterStatus newStatus)
        {
            Logging.Debug("Kerbal Status Change: " + kerbal.name + " - " + oldStatus + ":" + newStatus);
            if (CrewQDataStore.instance != null)
            {
                if (oldStatus == ProtoCrewMember.RosterStatus.Assigned && newStatus == ProtoCrewMember.RosterStatus.Available)
                {
                    double vacationTime = lastMissionTime * CrewQDataStore.instance.settingVacationScalar < (KERBAL_DAY * CrewQDataStore.instance.settingMinimumVacationDays) ? lastMissionTime * CrewQDataStore.instance.settingVacationScalar : (KERBAL_DAY * CrewQDataStore.instance.settingMinimumVacationDays);

                    CrewQDataStore.instance.CrewList.Add(new CrewNode(kerbal.name, (Planetarium.GetUniversalTime() + lastMissionTime)));
                }
            }
        }

        void onKerbalRemoved(ProtoCrewMember kerbal)
        {
            if (CrewQDataStore.instance != null)
            {
                CrewQDataStore.instance.CrewList.RemoveAll(x => x.crewRef == kerbal);
            }
        }

        void onLevelWasLoaded(GameScenes scene)
        {
            if (scene != GameScenes.EDITOR)
            {
                // Just in case!
                releaseOnce = true;
            }
        }

        // Our methods
        public void SuppressCrew()
        {
            if (CrewQDataStore.instance != null && CrewQDataStore.instance.settingVacationHardlock)
            {
                IEnumerable<ProtoCrewMember> suppressList = CrewQDataStore.instance.CrewList.Where(x => x.vacation == true).Select(x => x.crewRef);

                foreach (ProtoCrewMember kerbal in suppressList)
                {
                    Logging.Debug("Hiding: " + kerbal.name);
                    kerbal.rosterStatus = VACATION;
                }
            }
        }

        public void ReleaseCrew()
        {
            if (CrewQDataStore.instance != null)
            {
                IEnumerable<ProtoCrewMember> releaseList = CrewQDataStore.instance.CrewList.Where(x => x.crewRef.rosterStatus == VACATION).Select(x => x.crewRef);

                foreach (ProtoCrewMember kerbal in releaseList)
                {
                    Logging.Debug("Unhiding: " + kerbal.name);
                    kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
                }
            }
        }
    }
}
