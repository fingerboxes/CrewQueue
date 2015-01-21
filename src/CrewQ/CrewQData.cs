using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using KSPPluginFramework;
using FingerboxLib;

namespace CrewQ
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] { GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.FLIGHT })]
    class CrewQData : ScenarioModule
    {
        // Singleton boilerplate
        private static CrewQData _instance;
        public static CrewQData instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CrewQData.FindObjectOfType<CrewQData>();
                }
                return _instance;
            }
        }

        [KSPField(isPersistant= true)]
        public bool DoCustomAssignment = false;

        public override void OnAwake()
        {
            _instance = this;
        }
    }
}
