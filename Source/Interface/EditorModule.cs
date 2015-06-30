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
using System.Reflection;

using UnityEngine;
using KSPPluginFramework;
using FingerboxLib;

namespace CrewQueue.Interface
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class EditorModule : SceneModule
    {
        bool rootExists, cleanedRoot;

        // Monobehaviour Methods
        protected override void Awake()
        {
            base.Awake();
            GameEvents.onEditorShipModified.Add(OnEditorShipModified);
            GameEvents.onEditorScreenChange.Add(OnEditorScreenChanged);
        }

        // KSP Events
        protected void OnEditorShipModified(ShipConstruct ship)
        {
            rootExists = CheckRoot(ship);                       
        }

        protected void OnEditorScreenChanged(EditorScreen screen)
        {
            if (screen == EditorScreen.Crew)
            {
                RemapFillButton();
                CrewQueueRoster.HideVacationingCrew();
            }
            else
            {
                CrewQueueRoster.RestoreVacationingCrew();
            }
        }

        // Our methods
        protected override void Update()
        {
            if (CrewQueueSettings.Instance.AssignCrews)
            {
                try
                {
                    if (rootExists && !cleanedRoot)
                    {
                        CleanManifest();
                        cleanedRoot = true;
                    }
                    else if (!rootExists && cleanedRoot)
                    {
                        cleanedRoot = false;
                    }
                }
                catch (Exception)
                {
                    // No worries!
                    Logging.Debug("If there is a problem with clearing the roster, look here.");
                }
            }
        }

        protected bool CheckRoot(ShipConstruct ship)
        {
            return (ship != null) && (ship.Count > 0);
        }
    }
}