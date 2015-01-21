using System;

namespace KSPPluginFramework
{
    public class ConfigNodeUtils
    {
        /*
         * Returns the internal name of a part given a valid ConfigNode of the part. Parts saved to a craft file are saved as "part = $partname_$idNumber", 
         * while parts from an active Vessel are saved as "name = $partname". This can handle both situations
         * */
        public static string PartNameFromNode(ConfigNode part)
        {
            string name = "";
            if (part.HasValue("part"))
            {
                name = part.GetValue("part");
                name = name.Split('_')[0];
            }
            else if (part.HasValue("name"))
                name = part.GetValue("name");
            return name;
        }

        /*
         * Finds an AvailablePart from the LoadedPartsList based on the part name stored in the ConfigNode
         * */
        public static AvailablePart AvailablePartFromNode(ConfigNode part)
        {
            return PartLoader.LoadedPartsList.Find(aP => aP.name == PartNameFromNode(part));
        }

        /*
         * Tests to see if two ConfigNodes have the same information. Currently requires same ordering of values and subnodes
         * */
        public static Boolean ConfigNodesAreEquivalent(ConfigNode node1, ConfigNode node2)
        {
            //Check that the number of subnodes are equal
            if (node1.GetNodes().Length != node2.GetNodes().Length)
                return false;
            //Check that all the values are identical
            foreach (string valueName in node1.values.DistinctNames())
            {
                if (!node2.HasValue(valueName))
                    return false;
                if (node1.GetValue(valueName) != node2.GetValue(valueName))
                    return false;
            }

            //Check all subnodes for equality
            for (int index = 0; index < node1.GetNodes().Length; ++index)
            {
                if (!ConfigNodesAreEquivalent(node1.nodes[index], node2.nodes[index]))
                    return false;
            }

            //If all these tests pass, we consider the nodes to be equivalent
            return true;
        }
    }
}
