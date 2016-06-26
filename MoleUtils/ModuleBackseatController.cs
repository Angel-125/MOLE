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
    public class ModuleBackseatController : WBIAnimation
    {
        [KSPField]
        public string upgradeTech = string.Empty;

        protected bool isDeploying = false;

        protected void checkForUpgrade()
        {
            //If the player hasn't unlocked the upgradeTech node yet, then hide the RCS functionality.
            if (ResearchAndDevelopment.Instance != null && !string.IsNullOrEmpty(upgradeTech))
            {
                WBIConvertibleStorage storage = this.part.FindModuleImplementing<WBIConvertibleStorage>();

                //If the tech node hasn't been researched yet then hide the RCS module and meshes
                if (ResearchAndDevelopment.GetTechnologyState(upgradeTech) == RDTech.State.Available)
                    storage.SetGUIVisible(true);
                else
                    storage.SetGUIVisible(false);
            }
        }

        public void RemoveParentHeatShielding()
        {
            List<PartResource> doomedResources = new List<PartResource>();

            if (this.part.parent == null)
                return;

            //Look for ablative shielding in the parent part and remove it.
            //The backseat will have the shielding.
            foreach (PartResource resource in this.part.parent.Resources)
            {
                if (resource.resourceName == "Ablator")
                {
                    resource.amount = 0.001;
                    resource.maxAmount = 0.001;
                    doomedResources.Add(resource);
                }
            }
            foreach (PartResource doomed in doomedResources)
                this.part.Resources.list.Remove(doomed);
        }

        public void OnEditorAttach()
        {
            RemoveParentHeatShielding();
        }

        public override void SetupAnimations()
        {
            base.SetupAnimations();

            Events["ToggleAnimation"].guiActive = false;
            Events["ToggleAnimation"].guiActiveEditor = false;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            this.part.OnEditorAttach += OnEditorAttach;

            RemoveParentHeatShielding();

            //Upgrade: carry cargo
            checkForUpgrade();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            //If RCS is activated, deploy the periscope
            if (isDeploying == false && isDeployed == false && vessel.ActionGroups[KSPActionGroup.RCS])
            {
                Log("Deploying scope");
                isDeploying = true;
                isDeployed = true;
                PlayAnimation(!isDeployed);
            }

            //If RCS is disabled, retract the periscope
            else if (isDeploying == false && isDeployed == true && !vessel.ActionGroups[KSPActionGroup.RCS])
            {
                Log("Retracting scope");
                isDeploying = true;
                isDeployed = false;
                PlayAnimation(!isDeployed);
            }

            //Done playing animation?
            if (animationState.normalizedTime == 0f)
                isDeploying = false;
        }
    }
}
