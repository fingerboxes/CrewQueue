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
    class CrewQData : ScenarioModule
    {
        // Singleton boilerplate
        private static CrewQData _Instance;
        internal static CrewQData Instance
        {
            get
            {
                if (_Instance == null)
                {
                    throw new Exception("ERROR: Attempted to query CrewQData before it was loaded.");
                }

                return _Instance;
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
        public int settingMinimumVacationDays = 7;

        [KSPField(isPersistant = true)]
        public int settingMaximumVacationDays = 28;

        private List<CrewNode> _CrewList;

        public override void OnAwake()
        {
            _Instance = this;
            _CrewList = new List<CrewNode>();

            GameEvents.onKerbalAdded.Add(onKerbalAdded);
            GameEvents.onKerbalRemoved.Add(onKerbalRemoved);
        }

        void Start()
        {
            // This should only ever run once, when the game is first created or the mod is installed.
            if (_CrewList.Count < HighLogic.CurrentGame.CrewRoster.Count)
            {
                foreach (ProtoCrewMember kerbal in HighLogic.CurrentGame.CrewRoster.Crew)
                {
                    if (!_CrewList.Select(x => x.ProtoCrewReference).Contains(kerbal))
                    {
                        _CrewList.Add(new CrewNode(kerbal.name));
                    }
                }
            }

            CrewQ.Instance.ShowVacationingCrew();
        }

        void Destroy()
        {
            _Instance = null;

            GameEvents.onKerbalAdded.Remove(onKerbalAdded);
            GameEvents.onKerbalRemoved.Remove(onKerbalRemoved);
        }

        // KSP Events

        // If a Kerbal gets added to the roster, add it to ours
        private void onKerbalAdded(ProtoCrewMember kerbal)
        {
            if (!_CrewList.Select(x => x.ProtoCrewReference).Contains(kerbal))
            {
                _CrewList.Add(new CrewNode(kerbal.name));
            }
        }

        // Likewise, if a Kerbal is removed from the roster, remove it from ours
        private void onKerbalRemoved(ProtoCrewMember kerbal)
        {
            if (_CrewList.Select(x => x.ProtoCrewReference).Contains(kerbal))
            {
                _CrewList.Remove(_CrewList.FirstOrDefault(x => x.ProtoCrewReference == kerbal));
            }
        }

        // ScenarioModule methods
        public override void OnLoad(ConfigNode rootNode)
        {
            if (rootNode.HasNode("CrewList"))
            {
                rootNode = rootNode.GetNode("CrewList");
                IEnumerable<ConfigNode> crewNodes = rootNode.GetNodes();

                foreach (ConfigNode crewNode in crewNodes)
                {
                    _CrewList.Add(new CrewNode(crewNode));
                }
            }
        }

        public override void OnSave(ConfigNode rootNode)
        {
            rootNode.RemoveNode("CrewList");
            ConfigNode crewNodes = new ConfigNode("CrewList");

            foreach (CrewNode crewNode in _CrewList)
            {
                crewNodes.AddNode(crewNode.AsConfigNode());
            }

            rootNode.AddNode(crewNodes);
        }
    
        // Accessor methods.
        public double GetVacationTimer(ProtoCrewMember kerbal)
        {
            return _CrewList.FirstOrDefault(x => x.ProtoCrewReference == kerbal).Expiration;
        }

        public bool OnVacation(ProtoCrewMember kerbal)
        {
            return ((GetVacationTimer(kerbal) - Planetarium.GetUniversalTime()) > 0);
        }

        public void SetVacationTimer(ProtoCrewMember kerbal, double timeout)
        {
            _CrewList.FirstOrDefault(x => x.ProtoCrewReference == kerbal).Expiration = timeout;
        }
    }

    // Our storage node type.
    class CrewNode
    {
        private string name;
        private double expiration;

        public ProtoCrewMember ProtoCrewReference
        {
            get
            {
                return HighLogic.CurrentGame.CrewRoster.Crew.FirstOrDefault(x => x.name == name);
            }
        }

        public double Expiration
        {
            get
            {
                return expiration;
            }

            set
            {
                expiration = value;

                Logging.Debug("expiration is now: " + expiration);
            }
        }

        public CrewNode(ConfigNode sourceNode)
        {
            name = sourceNode.GetValue("Name");
            expiration = Convert.ToDouble(sourceNode.GetValue("Expiration"));
        }

        public CrewNode(string crewName)
        {
            name = crewName;
        }

        public ConfigNode AsConfigNode()
        {
            ConfigNode _thisNode = new ConfigNode("KERBAL");

            _thisNode.AddValue("Name", name);
            _thisNode.AddValue("Expiration", expiration);

            Logging.Debug("Building ConfigNode: expiration: " + expiration);

            return _thisNode;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(CrewNode))
            {
                return false;
            }

            if ((obj as CrewNode).ProtoCrewReference == this.ProtoCrewReference)
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
            return ProtoCrewReference.GetHashCode();
        }
    }
}
