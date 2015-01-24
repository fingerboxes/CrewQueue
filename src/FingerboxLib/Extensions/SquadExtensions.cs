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
            IEnumerable<int> openSeatIndexes = crewManifest.GetPartCrew().Select((x, index) => new { x, index }).Where(x => x.x == null).Select(x => x.index);

            // Need to know the number of excluded elements
            int openSeatIndexesCount = openSeatIndexes.ToArray().Length;

            // Get the members we are going to assign
            IEnumerator<ProtoCrewMember> crewEnumerator = crewMembers.Take(openSeatIndexesCount).GetEnumerator();


            Logging.Debug("indexLength:" + openSeatIndexes.ToArray().Length);
            foreach (int i in openSeatIndexes)
            {
                Logging.Debug("indexes:" + i);
            }

            foreach (ProtoCrewMember crew in crewMembers.Take(openSeatIndexesCount))
            {
                Logging.Debug("crew:" + crew.name);
            }

            foreach (int index in openSeatIndexes)
            {
                if (crewEnumerator.MoveNext())
                {
                    crewManifest.AddCrewToSeat(crewEnumerator.Current, index);
                }
                else
                {
                    return crewMembers.Skip(openSeatIndexesCount); ;
                }
            }

            // return the remainders
            return crewMembers.Skip(openSeatIndexesCount); ;
        }
    }
}
