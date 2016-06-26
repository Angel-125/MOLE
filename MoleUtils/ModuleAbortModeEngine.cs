using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

/*
Source code copyright 2016, by Michael Billard (Angel-125)
License: GNU General Public License Version 3
License URL: http://www.gnu.org/licenses/
Wild Blue Industries is trademarked by Michael Billard and may be used for non-commercial purposes. All other rights reserved.
Note that Wild Blue Industries is a ficticious entity 
created for entertainment purposes. It is in no way meant to represent a real entity.
Any similarity to a real entity is purely coincidental.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
namespace WildBlueIndustries
{
    public class ModuleAbortModeEngine : PartModule    
    {
        MultiModeEngine multiModeEngine;
        ModuleDecouple decoupler;

        [KSPField(guiName = "Abort Mode", isPersistant = true, guiActiveEditor = true, guiActive = true)]
        [UI_Toggle(disabledText = "Disarmed", enabledText = "Armed")]
        public bool abortModeArmed;

        [KSPAction("Arm Abort Mode")]
        public void ArmAbortMode(KSPActionParam param)
        {
            abortModeArmed = true;
        }

        [KSPAction("Disarm Abort Mode")]
        public void DisarmAbortMode(KSPActionParam param)
        {
            abortModeArmed = false;
        }

        [KSPAction("Activate Abort Mode", KSPActionGroup.Abort)]
        public void ToggleAbortAction(KSPActionParam param)
        {
            if (!abortModeArmed)
                return;

            if (multiModeEngine != null)
            {
                multiModeEngine.Events["ModeEvent"].Invoke();
                List<ModuleEnginesFX> engines = this.part.FindModulesImplementing<ModuleEnginesFX>();
                foreach (ModuleEnginesFX engine in engines)
                {
                    if (engine.engineID == multiModeEngine.secondaryEngineID)
                    {
                        engine.Activate();
                        engine.part.force_activate();
                        engine.staged = true;
                    }
                }
            }
        }

        [KSPAction("Decouple")]
        public void DecoupleAction(KSPActionParam param)
        {
            this.Decouple();
        }

        [KSPEvent(guiActive = true, guiName = "Decouple")]
        public void Decouple()
        {
            if (decoupler != null)
            {
                decoupler.isEnabled = true;
                decoupler.enabled = true;
                decoupler.stagingEnabled = true;
                decoupler.Decouple();
            }
        }

        public override string GetInfo()
        {
            return base.GetInfo();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            multiModeEngine = this.part.FindModuleImplementing<MultiModeEngine>();
            if (multiModeEngine != null)
            {
                multiModeEngine.Fields["mode"].guiActiveEditor = false;
                multiModeEngine.Fields["mode"].guiActive = false;
                multiModeEngine.Events["DisableAutoSwitch"].guiActive = false;
                multiModeEngine.Events["DisableAutoSwitch"].guiActiveEditor = false;
                multiModeEngine.Events["EnableAutoSwitch"].guiActive = false;
                multiModeEngine.Events["EnableAutoSwitch"].guiActiveEditor = false;
                multiModeEngine.Events["ModeEvent"].guiActive = false;
                multiModeEngine.Events["ModeEvent"].guiActiveEditor = false;
            }

            decoupler = this.part.FindModuleImplementing<ModuleDecouple>();
            if (decoupler != null)
            {
                decoupler.isEnabled = false;
                decoupler.enabled = false;
                decoupler.stagingEnabled = false;
            }
        }
    }
}
