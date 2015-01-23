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

// Start reading here!
namespace CrewQ
{
    [KSPAddon(KSPAddon.Startup.EveryScene, true)]
    public class CrewQ : MonoBehaviourExtended
    {
        // ITS OVER NINE THOUSAND!!!!111
        private const ProtoCrewMember.RosterStatus VACATION = (ProtoCrewMember.RosterStatus)9001;

        // Singleton boilerplate
        private static CrewQ _Instance;
        public static CrewQ Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = GameObject.FindObjectOfType<CrewQ>();
                }
                return _Instance;
            }
        }

        private bool releaseOnce;

        protected override void Awake()
        {
            Logging.Debug("Loading...");
            DontDestroyOnLoad(this);
            _Instance = this;

            GameEvents.onKerbalRemoved.Add(OnKerbalRemoved);
            GameEvents.onLevelWasLoaded.Add(OnLevelWasLoaded);
            GameEvents.OnVesselRecoveryRequested.Add(OnVesselRecoveryRequested);

            Logging.Debug("Loaded");
        }

        protected override void Update()
        {
            if (releaseOnce)
            {
                Logging.Debug("Releasing crew once, just in case.");
                ReleaseCrew();
                if (CrewQData.Instance != null)
                {
                    releaseOnce = false;
                }                
            }
        }

        void OnVesselRecoveryRequested(Vessel vessel)
        {
            OnCrewRecovery(vessel.GetVesselCrew(), vessel.missionTime);
        }

        void OnKerbalRemoved(ProtoCrewMember crewMember)
        {
            Logging.Debug("Crew removed from Roster: " + crewMember.name);
            if (CrewQData.Instance != null)
            {
                CrewQData.Instance.CrewList.RemoveAll(x => x.ProtoCrewReference == crewMember);
            }
        }

        void OnLevelWasLoaded(GameScenes scene)
        {
            if (scene != GameScenes.EDITOR)
            {
                // Just in case!
                releaseOnce = true;
            }
        }

        // Our methods
        public void OnCrewRecovery(IEnumerable<ProtoCrewMember> crewMembers, double missionTime)
        {
            CrewQData settings = CrewQData.Instance;

            double scaledVacationTime = missionTime * settings.settingVacationScalar,
                   minimumVacationTime = Utilities.GetDayLength * settings.settingMinimumVacationDays;

            double vacationTime = scaledVacationTime > minimumVacationTime ? scaledVacationTime : minimumVacationTime;

            foreach (ProtoCrewMember crewMember in crewMembers)
            {
                CrewNode existingCrewNode = settings.CrewList.First(x => x.ProtoCrewReference == crewMember);

                if (existingCrewNode != null)
                {
                    existingCrewNode.expiration = vacationTime;
                }
                else
                {
                    CrewQData.Instance.CrewList.Add(new CrewNode(crewMember.name, (Planetarium.GetUniversalTime() + vacationTime)));
                }
            }            
        }

        public void SuppressCrew()
        {
            if (CrewQData.Instance != null && CrewQData.Instance.settingVacationHardlock)
            {
                IEnumerable<ProtoCrewMember> vacationList = CrewQData.Instance.CrewList.Where(x => x.IsOnVacation == true).Select(x => x.ProtoCrewReference);

                foreach (ProtoCrewMember crewMember in vacationList)
                {
                    Logging.Debug("Hiding crew member: " + crewMember.name);
                    crewMember.rosterStatus = VACATION;
                }
            }
        }

        public void ReleaseCrew()
        {
            if (CrewQData.Instance != null)
            {
                IEnumerable<ProtoCrewMember> vacationList = CrewQData.Instance.CrewList.Where(x => x.ProtoCrewReference.rosterStatus == VACATION).Select(x => x.ProtoCrewReference);

                foreach (ProtoCrewMember crewMember in vacationList)
                {
                    Logging.Debug("Unhiding: " + crewMember.name);
                    crewMember.rosterStatus = ProtoCrewMember.RosterStatus.Available;
                }
            }
        }
    }
}
