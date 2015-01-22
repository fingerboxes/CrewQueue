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
using System.Text;

using UnityEngine;
using KSPPluginFramework;
using FingerboxLib;

namespace CrewQ
{
    [KSPAddon(KSPAddon.Startup.EveryScene, true)]
    public class CrewQ : MonoBehaviourExtended
    {
        // ITS OVER NINE THOUSAND!!!!111
        private const ProtoCrewMember.RosterStatus VACATION = (ProtoCrewMember.RosterStatus)9001;

        // Singleton boilerplate
        private static CrewQ _instance;
        public static CrewQ instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<CrewQ>();
                }
                return _instance;
            }
        }

        protected override void Awake()
        {
            DontDestroyOnLoad(this);
            _instance = this;
        }

        // Our methods

        public void SuppressVacationingCrew()
        {
            if (CrewQData.instance.VacationHardLock)
            {
                Logging.Debug("VacationHardLock is enabled, suppressing crew...");
                List<ProtoCrewMember> vacationList = GetVacationingCrew();

                foreach (ProtoCrewMember crew in vacationList)
                {
                    crew.rosterStatus = VACATION;
                }
            }
            else
            {
                Logging.Debug("VacationHardLock is disabled");
            }
        }

        public void UnsuppressVacationingCrew()
        {
            // This doesn't need to be qualified, doing so might introduce bugs.

            List<ProtoCrewMember> vacationList = GetVacationingCrew();

            foreach (ProtoCrewMember crew in vacationList)
            {
                crew.rosterStatus = ProtoCrewMember.RosterStatus.Available;
            }
        }

        List<ProtoCrewMember> GetVacationingCrew()
        {
            return CrewQData.instance.CrewOnVacation;
        }
    }
}
