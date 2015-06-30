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

using FingerboxLib;

namespace CrewQueue
{
    internal class CrewQueueRoster
    {
        // Singleton boilerplate
        private static CrewQueueRoster _Instance;
        public static CrewQueueRoster Instance
        {
            get
            {
                if (_Instance == null)
                {
                    Logging.Debug("Initializing Roster");
                    _Instance = new CrewQueueRoster();
                }

                return _Instance;
            }
        }

        // The basic idea here is going to be that the CrewQueue 'metadata' doesn't exist
        // until it is queried or set. When we query the data set, anything that doesn't
        // match an existing Kerbal is omitted, causing it to be deleted on the next on
        // the next save cycle.
        private HashSet<KerbalExtData> _ExtDataSet = new HashSet<KerbalExtData>();
        public IEnumerable<KerbalExtData> ExtDataSet
        {
            get
            {
                return _ExtDataSet.Where(x => x.ProtoReference != null);
            }
        }

        // These crew are the ones which are not available according to the current mod settings
        public IEnumerable<ProtoCrewMember> UnavailableCrew
        {
            get
            {
                return ExtDataSet.Where(k => k.OnVacation).Select(k => k.ProtoReference);
            }
        }

        // These crew are the ones which are available according to the current mod settings
        public IEnumerable<ProtoCrewMember> AvailableCrew
        {
            get
            {
                return HighLogic.CurrentGame.CrewRoster.Crew.Where(k => k.rosterStatus == ProtoCrewMember.RosterStatus.Available).Except(UnavailableCrew);                
            }
        }

        public IOrderedEnumerable<ProtoCrewMember> MostExperiencedCrew
        {
            get
            {
                return AvailableCrew.OrderByDescending(k => k.experienceLevel).ThenBy(k => k.GetLastMissionEndTime());
            }
        }

        public IOrderedEnumerable<ProtoCrewMember> LeastExperiencedCrew
        {
            get
            {
                return AvailableCrew.OrderBy(k => k.experienceLevel).ThenByDescending(k => k.GetLastMissionEndTime());
            }
        }

        public KerbalExtData GetExtForKerbal(string kerbal)
        {
            // Set only allows non-unique elements, so just try to add it anyway
            _ExtDataSet.Add(new KerbalExtData(kerbal));

            // Now find the element
            foreach (KerbalExtData data in _ExtDataSet)
            {
                if (data.Name == kerbal)
                {
                    return data;
                }
            }

            // This will never happen
            Logging.Error("Something dun broke, unsuccessful KerbalExtData lookup");
            return null;
        }

        public KerbalExtData GetExtForKerbal(ProtoCrewMember kerbal)
        {
            return GetExtForKerbal(kerbal.name);
        }

        public bool AddExtElement(KerbalExtData newElement)
        {
            return _ExtDataSet.Add(newElement);
        }

        public void Flush()
        {
            _ExtDataSet = new HashSet<KerbalExtData>();
        }

        public static void HideVacationingCrew()
        {
            foreach (ProtoCrewMember kerbal in CrewQueueRoster.Instance.UnavailableCrew.Where(k => k.rosterStatus == ProtoCrewMember.RosterStatus.Available))
            {
                kerbal.rosterStatus = CrewQueue.ROSTERSTATUS_VACATION;
            }
            CMAssignmentDialog.Instance.RefreshCrewLists(CMAssignmentDialog.Instance.GetManifest(), true, true);
        }

        public static void RestoreVacationingCrew()
        {
            foreach (ProtoCrewMember kerbal in HighLogic.CurrentGame.CrewRoster.Crew.Where(k => k.rosterStatus == CrewQueue.ROSTERSTATUS_VACATION))
            {
                kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
            }
            CMAssignmentDialog.Instance.RefreshCrewLists(CMAssignmentDialog.Instance.GetManifest(), true, true);
        }

        // Our storage node type.
        public class KerbalExtData
        {
            // This is the Kerbal which this ExtData is attached to.
            // Any mods which change the name of a Kerbal will need to update this value
            // TODO - add method to update values to API
            public string Name;

            // This will be true if the Kerbal has been sent on a mission while already on vacation.
            public bool ExtremelyFatigued = false;

            // Returns the duration of the last mission
            public double LastMissionDuration = -1;
            public double LastMissionEndTime = -1;

            public double VacationExpiry
            {
                get
                {
                    if (LastMissionDuration > -1)
                    {
                        double VacationScalar = CrewQueueSettings.Instance.VacationScalar,
                               MinimumVacationDays = CrewQueueSettings.Instance.MinimumVacationDays * Utilities.GetDayLength,
                               MaximumVacationDays = CrewQueueSettings.Instance.MaximumVacationDays * Utilities.GetDayLength,
                               Expiry = LastMissionEndTime + (LastMissionDuration * VacationScalar).Clamp(MinimumVacationDays, MaximumVacationDays);

                        Logging.Debug("EXPIRY: " + Expiry);

                        return Expiry;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            public bool OnVacation
            {
                get
                {
                    return VacationExpiry > Planetarium.GetUniversalTime() ? true : false;
                }
            }

            // Should return null if this is an unattached element
            public ProtoCrewMember ProtoReference
            {
                get
                {
                    return HighLogic.CurrentGame.CrewRoster.Crew.Where(k => k.name == Name).FirstOrDefault<ProtoCrewMember>();
                }
            }

            public ConfigNode ConfigNode
            {
                get
                {
                    ConfigNode _thisNode = new ConfigNode("KERBAL");

                    _thisNode.AddValue("Name", Name);
                    _thisNode.AddValue("GetLastMissionDuration", LastMissionDuration);
                    _thisNode.AddValue("LastMissionEndTime", LastMissionEndTime);
                    _thisNode.AddValue("ExtremelyFatigued", ExtremelyFatigued);

                    return _thisNode;
                }
            }

            public KerbalExtData(string newName)
            {
                Name = newName;
            }

            public KerbalExtData(ConfigNode configNode)
            {
                Name = configNode.GetValue("Name");
                LastMissionDuration = Convert.ToDouble(configNode.GetValue("GetLastMissionDuration"));
                LastMissionEndTime = Convert.ToDouble(configNode.GetValue("LastMissionEndTime"));
                ExtremelyFatigued = Convert.ToBoolean(configNode.GetValue("ExtremelyFatigued"));
            }

            public override bool Equals(object obj)
            {
                if (obj != null && (obj as KerbalExtData) != null && (obj as KerbalExtData).Name == Name)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }

            public override string ToString()
            {
                return Name;
            }
        }
    }

    public static class RosterExtensions
    {
        public static void SetLastMissionData(this ProtoCrewMember kerbal, double newMissionDuration, double currentTime)
        {
            CrewQueueRoster.Instance.GetExtForKerbal(kerbal).LastMissionDuration = newMissionDuration;
            CrewQueueRoster.Instance.GetExtForKerbal(kerbal).LastMissionEndTime = currentTime;
            GamePersistence.SaveGame("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
        }

        public static double GetLastMissionDuration(this ProtoCrewMember kerbal)
        {
            return CrewQueueRoster.Instance.GetExtForKerbal(kerbal).LastMissionDuration;
        }

        public static double VacationExpiry(this ProtoCrewMember kerbal)
        {
            return CrewQueueRoster.Instance.GetExtForKerbal(kerbal).VacationExpiry;
        }

        public static bool IsOnVacation(this ProtoCrewMember kerbal)
        {
            return CrewQueueRoster.Instance.GetExtForKerbal(kerbal).OnVacation;
        }

        public static bool IsForcedVacation(this ProtoCrewMember kerbal)
        {
            return CrewQueueRoster.Instance.GetExtForKerbal(kerbal).ExtremelyFatigued;
        }

        public static double GetLastMissionEndTime(this ProtoCrewMember kerbal)
        {
            return CrewQueueRoster.Instance.GetExtForKerbal(kerbal).LastMissionEndTime;
        }
    }
}
