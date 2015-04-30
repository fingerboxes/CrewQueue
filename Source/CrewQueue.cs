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
namespace CrewQueue
{
    [KSPAddon(KSPAddon.Startup.EveryScene, true)]
    public class CrewQueue : MonoBehaviourExtended
    {
        // ITS OVER NINE THOUSAND!!!!111
        internal const ProtoCrewMember.RosterStatus ROSTERSTATUS_VACATION = (ProtoCrewMember.RosterStatus)9001;

        // Singleton boilerplate
        private static CrewQueue _Instance;
        public static CrewQueue Instance
        {
            get
            {
                if (_Instance == null)
                {
                    throw new Exception("ERROR: Attempted to access CrewQueue before it was loaded");
                }
                return _Instance;
            }
        }

        // MonoBehaviour Methods
        protected override void Awake()
        {
            DontDestroyOnLoad(this);
            _Instance = this;            
            GameEvents.OnVesselRecoveryRequested.Add(OnVesselRecoveryRequested);
        }

        // KSP Events
        void OnVesselRecoveryRequested(Vessel vessel)
        {         
            foreach (ProtoCrewMember kerbal in vessel.GetVesselCrew())
            {
                kerbal.SetLastMissionData(vessel.missionTime, Planetarium.GetUniversalTime());
            }
        }

        internal IEnumerable<ProtoCrewMember> GetCrewForPart(Part partPrefab, bool preferVeterans = false)
        {
            IList<ProtoCrewMember> partCrew = new List<ProtoCrewMember>();
            IEnumerable<ProtoCrewMember> availableCrew = (preferVeterans ? CrewQueueRoster.Instance.MostExperiencedCrew : CrewQueueRoster.Instance.LeastExperiencedCrew);
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
                    Logging.Debug("Adding candidate: " + candidate.name);
                    partCrew.Add(candidate);
                }

                availableCrew = availableCrew.Except(partCrew);
            }

            return partCrew;
        } 
    }

    public class ModuleCrewQ : PartModule
    {
        [KSPField]
        public string crewComposition;
    }
}
