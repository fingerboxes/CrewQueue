using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using FingerboxLib;
using KSPPluginFramework;

namespace CrewQ.Interface
{
    [KSPAddon(KSPAddon.Startup.EveryScene,true)]
    class AppLauncher : FingerboxLib.Interface.ProtoAppLauncher
    {
        public override Texture AppLauncherIcon
        {
            get { return GameDatabase.Instance.GetTexture("Fingerboxes/CrewQ/Icons/appLauncher", false); }
        }

        private ApplicationLauncher.AppScenes _visibility;
        public override ApplicationLauncher.AppScenes Visibility
        {
            get
            {
                return _visibility;
            }
            set
            {
                _visibility = value;
            }
        }
    }
}
