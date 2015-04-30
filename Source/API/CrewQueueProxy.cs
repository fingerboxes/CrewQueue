using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrewQueue
{
    class CrewQueueProxy
    {
        public static IEnumerable<ProtoCrewMember> AvailableCrew
        {
            get
            {
                return CrewQueueRoster.Instance.AvailableCrew;
            }
        }

        public static IEnumerable<ProtoCrewMember> UnavailableCrew
        {
            get
            {
                return CrewQueueRoster.Instance.UnavailableCrew;
            }
        }

        public static IEnumerable<ProtoCrewMember> LeastExperiencedCrew
        {
            get
            {
                return CrewQueueRoster.Instance.LeastExperiencedCrew;
            }
        }

        public static IEnumerable<ProtoCrewMember> MostExperiencedCrew
        {
            get
            {
                return CrewQueueRoster.Instance.MostExperiencedCrew;
            }
        }

        public static IEnumerable<ProtoCrewMember> GetCrewForPart(Part partPrefab, IEnumerable<ProtoCrewMember> exemptList, bool preferVeterans = false)
        {
            return CrewQueue.Instance.GetCrewForPart(partPrefab, exemptList, preferVeterans);
        }
    }
}
