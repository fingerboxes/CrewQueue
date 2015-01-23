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

using UnityEngine;
using KSPPluginFramework;
using FingerboxLib;

namespace CrewQ
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] { GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION })]
    class CrewQDataStore : ScenarioModule
    {
        public const string VACATION_LABEL_SOFT = "Available for emergency missions";
        public const string VACATION_LABEL_HARD = "Not available for missions";

        // Singleton boilerplate
        private static CrewQDataStore _instance;
        public static CrewQDataStore instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CrewQDataStore.FindObjectOfType<CrewQDataStore>();
                }
                return _instance;
            }
        }

        [KSPField(isPersistant = true)]
        public bool settingHideSettingsIcon = false;

        [KSPField(isPersistant = true)]
        public bool settingUseCrewCompositions = true;

        [KSPField(isPersistant = true)]
        public bool settingDoCustomAssignment = true;

        [KSPField(isPersistant = true)]
        public bool settingVacationHardlock = false;

        [KSPField(isPersistant = true)]
        public bool settingRemoveDefaultCrews = true;

        [KSPField(isPersistant = true)]
        public double settingVacationScalar = 0.1;

        [KSPField(isPersistant = true)]
        public int settingMinimumVacationDays = 15;

        public List<CrewNode> CrewList;       

        public override void OnAwake()
        {
            _instance = this;
            CrewList = new List<CrewNode>();
        }

        public override void OnLoad(ConfigNode node)
        {
            if (node.HasNode("CrewList"))
            {
                ConfigNode CrewListNode = node.GetNode("CrewList");

                ConfigNode [] elements = CrewListNode.GetNodes();

                foreach (ConfigNode e in elements)
                {
                    CrewList.Add(new CrewNode(e));
                }
            }
        }

        public override void OnSave(ConfigNode node)
        {
            Logging.Debug("Attempting to Save..");

            node.RemoveNode("CrewList");
            ConfigNode CrewListNode = new ConfigNode("CrewList");

            foreach (CrewNode v in CrewList)
            {
                CrewListNode.AddNode(v.asNode);
            }

            node.AddNode(CrewListNode);

            Logging.Debug("Saved...");
        }
    }

    public class CrewNode
    {
        public string name;
        public double expiration;

        public ConfigNode asNode
        {
            get
            {
                ConfigNode node = new ConfigNode("KERBAL");

                node.AddValue("name", name);
                node.AddValue("expiration", expiration);

                return node;
            }
        }

        public double remaining
        {
            get
            {
                return expiration - Planetarium.GetUniversalTime();
            }
        }

        public ProtoCrewMember crewRef
        {
            get
            {
                return HighLogic.CurrentGame.CrewRoster.Crew.First(x => x.name == name);
            }
        }

        public bool vacation
        {
            get
            {
                return expiration > Planetarium.GetUniversalTime();
            }
        }

        public CrewNode(string name, double expiration)
        {
            this.name = name;
            this.expiration = expiration;
        }

        public CrewNode(ConfigNode n)
        {
            name = n.GetValue("name");
            expiration = Double.Parse(n.GetValue("expiration"));
        }
    }
}
