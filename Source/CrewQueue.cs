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
        internal const ProtoCrewMember.RosterStatus VACATION = (ProtoCrewMember.RosterStatus)9001;

        // Singleton boilerplate
        private static CrewQ _Instance;
        public static CrewQ Instance
        {
            get
            {
                if (_Instance == null)
                {
                    throw new Exception("ERROR: Attempted to access CrewQ before it was loaded");
                }
                return _Instance;
            }
        }

        // MonoBehaviour Methods
        protected override void Awake()
        {
            Logging.Debug("Loading...");

            DontDestroyOnLoad(this);

            _Instance = this;

            GameEvents.OnVesselRecoveryRequested.Add(OnVesselRecoveryRequested);

            Logging.Debug("Loaded");
        }

        // KSP Events
        void OnVesselRecoveryRequested(Vessel vessel)
        {
            double adjustedTime = vessel.missionTime + Planetarium.GetUniversalTime();

            adjustedTime = adjustedTime.Clamp(Planetarium.GetUniversalTime() + CrewQData.Instance.settingMinimumVacationDays * Utilities.GetDayLength, 
                                              Planetarium.GetUniversalTime() + CrewQData.Instance.settingMaximumVacationDays * Utilities.GetDayLength);

            foreach (ProtoCrewMember kerbal in vessel.GetVesselCrew())
            {
                kerbal.SetVacationTimer(adjustedTime);
            }

            GamePersistence.SaveGame("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
        }

        // Our public methods
        internal IEnumerable<ProtoCrewMember> AvailableCrew
        {
            get
            {
                IEnumerable<ProtoCrewMember> _AvailableCrew;

                if (CrewQData.Instance.settingVacationHardlock)
                {
                    _AvailableCrew = HighLogic.CurrentGame.CrewRoster.Crew.Where(x => x.OnVacation() == false && x.rosterStatus == ProtoCrewMember.RosterStatus.Available);
                }
                else
                {
                    _AvailableCrew = HighLogic.CurrentGame.CrewRoster.Crew.Where (x => x.rosterStatus == ProtoCrewMember.RosterStatus.Available);
                }

                try
                {
                    _AvailableCrew.Except(CMAssignmentDialog.Instance.GetManifest().GetAllCrew(false));
                }
                catch (Exception)
                {
                    //nothing to see here, move along
                }          

                return _AvailableCrew.OrderBy(x => x.GetVacationTimer());
            }
        }

        internal IEnumerable<ProtoCrewMember> UnavailableCrew
        {
            get
            {
                return HighLogic.CurrentGame.CrewRoster.Crew.Except(AvailableCrew);
            }
        }

        internal IOrderedEnumerable<ProtoCrewMember> NewbieCrew
        {
            get
            {
                return AvailableCrew.OrderBy(x => x.experienceLevel).ThenBy(x => x.GetVacationTimer());
            }
        }

        internal IOrderedEnumerable<ProtoCrewMember> VeteranCrew
        {
            get
            {
                return AvailableCrew.OrderByDescending(x => x.experienceLevel).ThenBy(x => x.GetVacationTimer());
            }
        }

        internal void HideVacationingCrew()
        {
            if (CrewQData.Instance.settingVacationHardlock)
            {
                foreach (ProtoCrewMember kerbal in UnavailableCrew)
                {
                    Logging.Debug("HIDING:" + kerbal.name);
                    kerbal.rosterStatus = VACATION;
                }
            }
        }

        internal void ShowVacationingCrew()
        {
            foreach (ProtoCrewMember kerbal in HighLogic.CurrentGame.CrewRoster.Crew.Where(x => x.rosterStatus == VACATION))
            {
                Logging.Debug("UNHIDING:" + kerbal.name);
                kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
            }
        }

        internal IEnumerable<ProtoCrewMember> GetCrewForPart(Part partPrefab, bool preferVeterans = false)
        {
            IList<ProtoCrewMember> partCrew = new List<ProtoCrewMember>();
            IEnumerable<ProtoCrewMember> availableCrew = (preferVeterans ? VeteranCrew : NewbieCrew);
            string[] crewComposition;
            int numToSelect = partPrefab.CrewCapacity;
            ProtoCrewMember candidate;

            //Get Crew Composition
            if (partPrefab.Modules.OfType<ModuleCrewQ>().Any())
            {
                crewComposition = partPrefab.Modules["ModuleCrewQ"].Fields.GetValue<string>("crewComposition").Split(',').Select(x => x.Trim()).ToArray();
            }
            else
            {
                crewComposition = new string[] { "Pilot", "Engineer", "Scientist" };
            }

            for (int i = 0; i < numToSelect; i++)
            {
                if (i < crewComposition.Length)
                {
                    candidate = availableCrew.Where(x => x.experienceTrait.Title == crewComposition[i]).FirstOrDefault();                    
                }
                else
                {
                    candidate = availableCrew.Where(x => x.experienceTrait.Title == crewComposition[new System.Random().Next(crewComposition.Length)]).FirstOrDefault();           
                }

                if (candidate != null)
                {
                    partCrew.Add(candidate);
                }

                availableCrew = availableCrew.Except(partCrew);
            }

            return partCrew;
        } 
    }

    // The idea here is that we always want to be dealing with ProtoCrewMember outside of this class.
    // By making our data available as extension methods, that makes life easier.
    public static class CrewQExtensions
    {
        internal static double GetVacationTimer(this ProtoCrewMember kerbal)
        {
            return CrewQData.Instance.GetVacationTimer(kerbal);
        }

        internal static bool OnVacation(this ProtoCrewMember kerbal)
        {
            return CrewQData.Instance.OnVacation(kerbal);
        }

        internal static void SetVacationTimer(this ProtoCrewMember kerbal, double timeout)
        {
            Logging.Debug("Attempting to set vacation timer: " + timeout);
            CrewQData.Instance.SetVacationTimer(kerbal, timeout);
        }
    }

    public class ModuleCrewQ : PartModule
    {
        [KSPField]
        public string crewComposition;
    }
}
