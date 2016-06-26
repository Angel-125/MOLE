using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;
using KSP.UI.Screens;

/*
Source code copyrighgt 2016, by Michael Billard (Angel-125)
License: GNU General Public License Version 3
License URL: http://www.gnu.org/licenses/
If you want to use this code, give me a shout on the KSP forums! :)
Wild Blue Industries is trademarked by Michael Billard and may be used for non-commercial purposes. All other rights reserved.
Note that Wild Blue Industries is a ficticious entity 
created for entertainment purposes. It is in no way meant to represent a real entity.
Any similarity to a real entity is purely coincidental.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
namespace WildBlueIndustries
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class MOLEConfigMenu : MonoBehaviour
    {
        static protected ApplicationLauncherButton appLauncherButton = null;
        static protected bool buttonAdded;
        static protected Texture2D appIcon = null;
        static protected bool pathfinderInstalled;
        protected MOLESettings moleSettings = new MOLESettings();

        public void OnGUI()
        {
            if (moleSettings.IsVisible())
                moleSettings.DrawWindow();
        }

        public void Awake()
        {
            pathfinderInstalled = Utils.IsModInstalled("Pathfinder");
            if (pathfinderInstalled)
            {
                this.enabled = false;
            }
        }

        public void Update()
        {
            addButton();
        }

        protected virtual void addButton()
        {
            if (ApplicationLauncher.Ready && !buttonAdded)
            {
                appLauncherButton = InitializeApplicationButton();

                if (appLauncherButton != null)
                    appLauncherButton.VisibleInScenes = ApplicationLauncher.AppScenes.SPACECENTER;

                buttonAdded = true;
            }
        }

        public void OnDestroy()
        {
            if (appLauncherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(appLauncherButton);
                appLauncherButton = null;
                buttonAdded = false;
            }
        }

        protected ApplicationLauncherButton InitializeApplicationButton()
        {
            ApplicationLauncherButton appButton = null;
            appIcon = GameDatabase.Instance.GetTexture("WildBlueIndustries/MOLE/Icons/MoleIcon", false);

            if (appIcon != null)
            {
                appButton = ApplicationLauncher.Instance.AddModApplication(
                    OnAppLauncherTrue,
                    OnAppLauncherFalse,
                    null,
                    null,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.SPACECENTER,
                    appIcon);

                buttonAdded = true;
            }

            return appButton;
        }

        void OnAppLauncherTrue()
        {
            moleSettings.SetVisible(true);
        }

        void OnAppLauncherFalse()
        {
            moleSettings.SetVisible(false);
        }
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class MOLEConfigMenuFlight : MOLEConfigMenu
    {
        protected override void addButton()
        {
            base.addButton();
            if (buttonAdded)
                appLauncherButton.VisibleInScenes = ApplicationLauncher.AppScenes.FLIGHT;
        }
    }
}
