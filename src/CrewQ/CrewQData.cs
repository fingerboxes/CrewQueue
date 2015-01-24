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
        public static CrewQData Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = CrewQData.FindObjectOfType<CrewQData>();
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
       
        // Various derived rosters
        public IEnumerable<ProtoCrewMember> CrewRoster
        {
            get
            {
                IEnumerable<ProtoCrewMember> crewRoster = HighLogic.CurrentGame.CrewRoster.Crew.Where(x => x.rosterStatus == ProtoCrewMember.RosterStatus.Available);

                if (settingVacationHardlock)
                {
                    return crewRoster.Except(VacationingCrewRoster);
                }
                else
                {
                    return crewRoster;
                }
            }
        }

        public IEnumerable<ProtoCrewMember> AvailableCrewRoster
        {
            get
            {
                if (settingVacationHardlock)
                {
                    return CrewRoster;
                }
                else
                {
                    return CrewRoster.Except(VacationingCrewRoster);
                }
            }
        }

        public IEnumerable<ProtoCrewMember> VacationingCrewRoster
        {
            get
            {
                return _CrewList.Where(x => x.IsOnVacation).Select(x => x.ProtoCrewReference).ToArray();
            }
        }        

        public IEnumerable<ProtoCrewMember> AvailableScientistRoster
        {
            get
            {
                return AvailableCrewRoster.Where(x => x.experienceTrait.Title == "Scientist");
            }
        }

        public IEnumerable<ProtoCrewMember> AvailableEngineerRoster
        {
            get
            {
                return AvailableCrewRoster.Where(x => x.experienceTrait.Title == "Engineer");
            }
        }

        public IEnumerable<ProtoCrewMember> AvailablePilotRoster
        {
            get
            {
                return AvailableCrewRoster.Where(x => x.experienceTrait.Title == "Pilot");
            }
        }

        public override void OnAwake()
        {
            _Instance = this;
            _CrewList = new List<CrewNode>();
        }

        // ScenarioModule methods
        public override void OnLoad(ConfigNode rootNode)
        {
            if (rootNode.HasNode("_CrewList"))
            {
                ConfigNode crewListNode = rootNode.GetNode("_CrewList");

                IEnumerable<ConfigNode> crewNodes = crewListNode.GetNodes();

                foreach (ConfigNode crewNode in crewNodes)
                {
                    _CrewList.Add(new CrewNode(crewNode));
                }
            }
        }

        public override void OnSave(ConfigNode rootNode)
        {
            rootNode.RemoveNode("_CrewList");
            ConfigNode crewListNode = new ConfigNode("_CrewList");

            foreach (CrewNode crewNode in _CrewList)
            {
                crewListNode.AddNode(crewNode.GetConfigNode);
            }

            rootNode.AddNode(crewListNode);
        }
        
        // Our methods
        public void AddOrUpdateCrew(ProtoCrewMember inputProtoCrewReference, double inputVacationTime)
        {
            CrewNode existingCrewNode = _CrewList.FirstOrDefault(x => x.ProtoCrewReference == inputProtoCrewReference);

            if (existingCrewNode != null)
            {
                existingCrewNode.expiration = inputVacationTime;
            }
            else
            {
                _CrewList.Add(new CrewNode(inputProtoCrewReference.name, (Planetarium.GetUniversalTime() + inputVacationTime)));
            }
        }


        // We don't actually ever want remove crew who DO stuff. It isn't as if this list will ever get super-huge.
        public bool RemoveCrewIfDeadOrFired(ProtoCrewMember inputProtoCrewReference)
        {
            CrewNode existingCrewNode = _CrewList.FirstOrDefault(x => x.ProtoCrewReference == inputProtoCrewReference);

            if (existingCrewNode != null && HighLogic.CurrentGame.CrewRoster.Crew.Contains(inputProtoCrewReference))
            {
                _CrewList.Remove(existingCrewNode);
                return true;
            }
            else
            {
                return false;
            }
        }

        public double GetVacationTime(ProtoCrewMember protoCrewMember)
        {
            double value = 0;

            if (_CrewList.Exists(x => x.ProtoCrewReference == protoCrewMember))
            {
                value =  _CrewList.FirstOrDefault(x => x.ProtoCrewReference == protoCrewMember).expiration;
            }

            return value;
        }
    }

    // The idea here is that we always want to be dealing with ProtoCrewMember outside of this file.
    // By making our data available as extension methods, that sort of makes life easier.
    public static class CrewQExtensions
    {
        public static double GetVacationTime(this ProtoCrewMember protoCrewMember)
        {
            double value = 0;

            if (CrewQData.Instance != null)
            {
                 value = CrewQData.Instance.GetVacationTime(protoCrewMember);
            }

            return value;
        }

        public static bool IsOnVacation(this ProtoCrewMember protoCrewMember)
        {
            if (CrewQData.Instance != null && CrewQData.Instance.VacationingCrewRoster.Contains(protoCrewMember))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    // Our storage node type.
    class CrewNode
    {
        public string name;
        public double expiration;

        public ConfigNode GetConfigNode
        {
            get
            {
                ConfigNode node = new ConfigNode("KERBAL");

                node.AddValue("crewName", name);
                node.AddValue("targetExpiration", expiration);

                return node;
            }
        }

        public double RemainingTime
        {
            get
            {
                return expiration - Planetarium.GetUniversalTime();
            }
        }

        public ProtoCrewMember ProtoCrewReference
        {
            get
            {
                return HighLogic.CurrentGame.CrewRoster.Crew.FirstOrDefault(x => x.name == name);
            }
        }

        public bool IsOnVacation
        {
            get
            {
                return expiration > Planetarium.GetUniversalTime();
            }
        }

        public CrewNode(string crewName, double targetExpiration)
        {
            name = crewName;
            expiration = targetExpiration;
        }

        public CrewNode(ConfigNode sourceNode)
        {
            name = sourceNode.GetValue("crewName");
            expiration = Double.Parse(sourceNode.GetValue("targetExpiration"));
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
