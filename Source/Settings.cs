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

namespace CrewQueue
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] { GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION })]
    class Settings : ScenarioModule
    {
        // Singleton boilerplate
        private static Settings _Instance;
        internal static Settings Instance
        {
            get
            {
                if (_Instance == null)
                {
                    throw new Exception("ERROR: Attempted to query CrewQueue.Data before it was loaded.");
                }

                return _Instance;
            }
        }

        [KSPField(isPersistant = true)]
        public bool HideSettingsIcon = false;

        [KSPField(isPersistant = true)]
        public bool StrictCrewCompositions = true;

        [KSPField(isPersistant = true)]
        public bool AssignCrews = true;

        [KSPField(isPersistant = true)]
        public bool VacationHardLock = true;

        [KSPField(isPersistant = true)]
        public double VacationScalar = 0.1;

        [KSPField(isPersistant = true)]
        public int MinimumVacationDays = 7;

        [KSPField(isPersistant = true)]
        public int MaximumVacationDays = 28;

        public override void OnAwake()
        {
            _Instance = this;
        }

        void Destroy()
        {
            _Instance = null;
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

    
}
