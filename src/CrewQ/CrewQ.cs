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

        private IList<ProtoCrewMember> _assignedCrewBuffer;

        private bool _releaseOnce;

        protected override void Awake()
        {
            Logging.Debug("Loading...");
            DontDestroyOnLoad(this);
            _Instance = this;

            _assignedCrewBuffer = new List<ProtoCrewMember>();

            GameEvents.onKerbalRemoved.Add(OnKerbalRemoved);
            GameEvents.onLevelWasLoaded.Add(OnLevelWasLoaded);
            GameEvents.OnVesselRecoveryRequested.Add(OnVesselRecoveryRequested);

            Logging.Debug("Loaded");
        }

        protected override void Update()
        {
            if (_releaseOnce && CrewQData.Instance != null)
            {
                Logging.Debug("Releasing crew once, just in case.");
                ReleaseCrew();

                _releaseOnce = false;
            }
        }

        void OnVesselRecoveryRequested(Vessel vessel)
        {
            Logging.Debug("Vessel Recovery Event Recieved");
            OnCrewRecovery(vessel.GetVesselCrew(), vessel.missionTime);
        }

        void OnKerbalRemoved(ProtoCrewMember crewMember)
        {
            Logging.Debug("Kerbal Remove Event");
            if (CrewQData.Instance != null)
            {
                Logging.Info("Attempting to remove crew from roster... " + ((CrewQData.Instance.RemoveCrewIfDeadOrFired(crewMember)) ? "Success" : "Failure"));
            }
        }

        void OnLevelWasLoaded(GameScenes scene)
        {
            Logging.Debug("Scene Loaded Event");
            if (scene != GameScenes.EDITOR)
            {
                // Just in case!
                Logging.Debug("Queueing release event");
                _releaseOnce = true;
            }

            Logging.Debug("Clearing _assignedCrewBuffer");
            _assignedCrewBuffer.Clear();
        }

        // Our methods
        public void OnCrewRecovery(IEnumerable<ProtoCrewMember> crewMembers, double missionTime)
        {
            CrewQData settings = CrewQData.Instance;

            double scaledVacationTime =  Planetarium.GetUniversalTime() + missionTime * settings.settingVacationScalar,
                   minimumVacationTime = Utilities.GetDayLength * settings.settingMinimumVacationDays,
                   maximumVacationTime = Utilities.GetDayLength * settings.settingMaximumVacationDays;

            scaledVacationTime.Clamp(minimumVacationTime, maximumVacationTime);

            foreach (ProtoCrewMember crewMember in crewMembers)
            {
                CrewQData.Instance.AddOrUpdateCrew(crewMember, scaledVacationTime);
            }            
        }

        public void ClearAssignedBuffer()
        {
            _assignedCrewBuffer.Clear();
        }

        public void RemoveFromBuffer(IEnumerable<ProtoCrewMember> excessCrew)
        {
            Logging.Debug("Removing from assigned crew buffer");
            _assignedCrewBuffer = _assignedCrewBuffer.Except(excessCrew).ToList();

            Logging.Debug("Buffer now contains:");
            foreach (ProtoCrewMember crewMember in _assignedCrewBuffer)
            {
                Logging.Debug(crewMember.name);
            }
        }

        public void AddToBuffer(IEnumerable<ProtoCrewMember> assignedCrew)
        {
            Logging.Debug("Adding to assigned crew buffer");
            _assignedCrewBuffer = _assignedCrewBuffer.Union(assignedCrew).ToList();

            Logging.Debug("Buffer now contains:");
            foreach (ProtoCrewMember crewMember in _assignedCrewBuffer)
            {
                Logging.Debug(crewMember.name);
            }
        }

        public void SuppressCrew()
        {
            if (CrewQData.Instance != null && CrewQData.Instance.settingVacationHardlock)
            {              
                foreach (ProtoCrewMember crewMember in CrewQData.Instance.VacationingCrewRoster)
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
                foreach (ProtoCrewMember crewMember in CrewQData.Instance.VacationingCrewRoster)
                {
                    Logging.Debug("Unhiding: " + crewMember.name);
                    crewMember.rosterStatus = ProtoCrewMember.RosterStatus.Available;
                }
            }
        }

        public IEnumerable<ProtoCrewMember> GetCrewForPart(Part part, bool veteran = false)
        {
            ProtoCrewMember[] crewBuffer = new ProtoCrewMember[part.CrewCapacity];

            string[] crewSequence = new string[] { "Pilot", "Engineer", "Scientist" };

            for (int i = 0; i < part.CrewCapacity; i++)
            {
                int selector;
                if (i < crewSequence.Length)
                {
                    selector = i;
                }
                else
                {
                    selector = new System.Random().Next(0, crewSequence.Length);
                }
                if (veteran)
                {
                    crewBuffer[i] = GetExperiencedCrewMember(crewSequence[selector]);
                }
                else
                {
                    crewBuffer[i] = GetCrewMember(crewSequence[selector]);
                } 
            }

            return crewBuffer;
        }

        private IEnumerable<ProtoCrewMember> GetRosterByJobTitle(string jobTitle)
        {
            IEnumerable<ProtoCrewMember> crewRoster = null;

            if (CrewQData.Instance != null)
            {
                CrewQData data = CrewQData.Instance;

                switch (jobTitle)
                {
                    case "Pilot":
                        crewRoster = data.AvailablePilotRoster.Except(_assignedCrewBuffer);
                        break;
                    case "Engineer":
                        crewRoster = data.AvailableEngineerRoster.Except(_assignedCrewBuffer);
                        break;
                    case "Scientist":
                        crewRoster = data.AvailableScientistRoster.Except(_assignedCrewBuffer);
                        break;
                    default:
                        crewRoster = data.AvailableCrewRoster.Except(_assignedCrewBuffer);
                        break;
                }
            }

            return crewRoster;
        }

        private ProtoCrewMember GetCrewMember(string jobTitle)
        {
            Logging.Debug("Getting a crew member...");
            ProtoCrewMember selectedCrew = null;
            if (CrewQData.Instance != null)
            {
                IEnumerable<ProtoCrewMember> crewRoster = GetRosterByJobTitle(jobTitle).OrderBy(x => x.experienceLevel).ToList().TakePercent(25);
                selectedCrew = crewRoster.RandomElement(new System.Random());
            }
            return selectedCrew;
        }

        private ProtoCrewMember GetExperiencedCrewMember(string jobTitle)
        {
            Logging.Debug("Getting an experienced crew member...");
            ProtoCrewMember selectedCrew = null;
            if (CrewQData.Instance != null)
            {
                IEnumerable<ProtoCrewMember> crewRoster = GetRosterByJobTitle(jobTitle);
                crewRoster = crewRoster.GroupBy(x => x.experienceLevel).OrderByDescending(x => x.Key).FirstOrDefault();
                selectedCrew = crewRoster.RandomElement(new System.Random());                
            }
            return selectedCrew;
        }
    }
}
