using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

/*
Source code copyright 2018, by Michael Billard (Angel-125)
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
    public class ModuleDeployableWing : PartModule
    {
        [KSPField]
        public float deflectionLiftCoeffDeployed;

        [KSPField]
        public float deflectionLiftCoeffStowed;

        [KSPField]
        public string dragModelTypeDeployed;

        [KSPField]
        public string dragModelTypeStowed;

        ModuleAnimateGeneric anim;
        bool watchAnimation;
        bool isDeploying;

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Deploy Wing")]
        public void DeployWing()
        {
            Events["StowWing"].active = true;
            Events["DeployWing"].active = false;
            watchAnimation = true;
            isDeploying = true;
            anim.Toggle();
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Stow Wing")]
        public void StowWing()
        {
            Events["StowWing"].active = false;
            Events["DeployWing"].active = true;
            watchAnimation = true;
            isDeploying = false;
            anim.Toggle();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            anim = this.part.FindModuleImplementing<ModuleAnimateGeneric>();
            if (anim == null)
            {
                Debug.Log("[ModuleDeployableWing] ModuleAnimateGeneric not found!");
                return;
            }
            anim.Events["Toggle"].guiActive = false;
            anim.Events["Toggle"].guiActiveEditor = false;
            anim.Events["Toggle"].guiActiveUnfocused = false;

            ModuleLiftingSurface liftingSurface = this.part.FindModuleImplementing<ModuleLiftingSurface>();
            if (liftingSurface == null)
            {
                Debug.Log("[ModuleDeployableWing] ModuleLiftingSurface not found!");
                return;
            }

            //If we're showing the endEventGUIName then the wing is deployed.
            if (anim.Events["Toggle"].guiName == anim.startEventGUIName) //animation.endEventGUIName)
            {
                Events["DeployWing"].active = false; //true;
                Events["StowWing"].active = true; //false;

                liftingSurface.deflectionLiftCoeff = deflectionLiftCoeffDeployed; //deflectionLiftCoeffStowed
            }

            else
            {
                Events["DeployWing"].active = true; //false;
                Events["StowWing"].active = false; //true;

                liftingSurface.deflectionLiftCoeff = deflectionLiftCoeffStowed; //deflectionLiftCoeffDeployed
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (HighLogic.LoadedSceneIsFlight == false)
                return;
            if (watchAnimation == false)
                return;

            //Are we done deploying/stowing the wing?
            //If so, update the lift and the GUI.
            if (anim.aniState == ModuleAnimateGeneric.animationStates.MOVING)
            {
                ModuleLiftingSurface liftingSurface = this.part.FindModuleImplementing<ModuleLiftingSurface>();

                if (liftingSurface == null)
                    return;

                if (isDeploying)
                {
                    //Update dragModel & CoLOffset
                    Events["DeployWing"].active = false;
                    Events["StowWing"].active = true;
                    liftingSurface.deflectionLiftCoeff = deflectionLiftCoeffDeployed;
                    watchAnimation = false;
                }
                else
                {
                    //Update dragModel & CoLOffset
                    Events["DeployWing"].active = true;
                    Events["StowWing"].active = false;
                    liftingSurface.deflectionLiftCoeff = deflectionLiftCoeffStowed;
                    watchAnimation = false;
                }

            }
        }
    }
}
