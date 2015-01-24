using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FingerboxLib
{
    public static class SquadExtensions
    {
        public static IEnumerable<ProtoCrewMember> AddCrewToOpenSeats(this PartCrewManifest crewManifest, IEnumerable<ProtoCrewMember> crewMembers)
        {
            // crewManifest.GetPartCrew() is a statically-sized array with null elements
            IEnumerable<int> openSeatIndexes = crewManifest.GetPartCrew().Where(x => x == null).Select(x => x.seatIdx);

            // Need to know the number of excluded elements
            int openSeatIndexesCount = openSeatIndexes.ToList().Count;

            // Get the members we are going to assign
            IEnumerator<ProtoCrewMember> crewEnumerator = crewMembers.Take(openSeatIndexesCount).GetEnumerator();

            foreach (int index in openSeatIndexes)
            {
                if (crewEnumerator.MoveNext())
                {
                    crewManifest.AddCrewToSeat(crewEnumerator.Current, index);
                }
                else
                {
                    return crewMembers.Skip(openSeatIndexesCount);;
                }
            }

            // return the remainders
            return crewMembers.Skip(openSeatIndexesCount);;
        }
    }
}
