using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FingerboxLib;

namespace CrewQueue
{
    internal class Roster
    {
        // Singleton boilerplate
        private static Roster _Instance;
        internal static Roster Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new Roster();
                }

                return _Instance;
            }
        }

        private HashSet<KerbalExtData> _ExtDataSet = new HashSet<KerbalExtData>();

        public IEnumerable<KerbalExtData> ExtDataSet
        {
            get
            {
                return _ExtDataSet.Where(x => x.ProtoReference != null);
            }
        }

        public IEnumerable<ProtoCrewMember> CrewOnVacation
        {
            get
            {
                return ExtDataSet.Where(x => x.OnVacation).Select(x => x.ProtoReference);
            }
        }

        public IEnumerable<ProtoCrewMember> CrewAvailable
        {
            get
            {
                return HighLogic.CurrentGame.CrewRoster.Crew.Except(CrewOnVacation);
            }
        }

        // Returns the ExtData node for the specified Kerbal name
        internal KerbalExtData GetExtForKerbal(ProtoCrewMember kerbal)
        {
            return GetExtForKerbal(kerbal.name);
        }

        internal KerbalExtData GetExtForKerbal(string kerbal)
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

        internal bool UpdateExtName(string oldName, string newName)
        {
            //make sure that the new name is available
            if (HighLogic.CurrentGame.CrewRoster.Crew.Where(x => x.name == newName).ToList().Count == 0)
            {
                GetExtForKerbal(oldName).Name = newName;
                return true;
            }
            else
            {
                return false;
            }
        }

        // Our storage node type.
        internal class KerbalExtData
        {
            // This is the Kerbal which this ExtData is attached to.
            // Any mods which change the name of a Kerbal will need to update this value
            // TODO - add method to update values to API
            internal string Name;

            // This is the time value at which this Kerbal's last mission started.
            // When a mission is started, the LastMissionEndTime must also be reset
            internal double LastMissionStartTime = 0;
            internal double LastMissionEndTime = 0;

            // Returns the duration of the last mission
            internal double LastMissionDuration
            {
                get
                {
                    return (LastMissionEndTime - LastMissionStartTime).Clamp(0, LastMissionEndTime);
                }
            }

            internal double LastMissionVacationExpiry
            {
                get
                {
                    Settings settings = Settings.Instance;
                    return (settings.VacationScalar * LastMissionDuration).Clamp(settings.MinimumVacationDays, settings.MaximumVacationDays);
                }
            }

            internal bool OnVacation
            {
                get
                {
                    return LastMissionVacationExpiry > Planetarium.GetUniversalTime() ? true : false;
                }
            }

            // Should return null if this is an unattached element
            internal ProtoCrewMember ProtoReference
            {
                get
                {
                    return HighLogic.CurrentGame.CrewRoster.Crew.Where(k => k.name == Name).FirstOrDefault<ProtoCrewMember>();
                }
            }

            internal KerbalExtData(string newName)
            {
                Name = newName;
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

    internal static class RosterExtensions
    {
        public static double GetLastMissionStartTime(this ProtoCrewMember kerbal)
        {
            return Roster.Instance.GetExtForKerbal(kerbal).LastMissionStartTime;
        }

        public static double GetLastMissionEndTime(this ProtoCrewMember kerbal)
        {
            return Roster.Instance.GetExtForKerbal(kerbal).LastMissionEndTime;
        }

        public static double LastMissionDuration(this ProtoCrewMember kerbal)
        {
            return Roster.Instance.GetExtForKerbal(kerbal).LastMissionDuration;
        }

        public static double LastMissionVacationExpiry(this ProtoCrewMember kerbal)
        {
            return Roster.Instance.GetExtForKerbal(kerbal).LastMissionVacationExpiry;
        }
    }
}
