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
    public class ModuleRCSDisabler : PartModule
    {
        [KSPField()]
        public string disableRCSOnParts = string.Empty;

        bool wasAttached;
        ModuleRCS parentRCS;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (!HighLogic.LoadedSceneIsFlight)
                return;
            if (this.part.parent == null)
                return;

            GameEvents.onVesselChange.Add(onVesselChange);

            findParentRCS(this.part.parent);
        }

        public void OnDestroy()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            GameEvents.onVesselChange.Remove(onVesselChange);
        }

        protected void onVesselChange(Vessel changedVessel)
        {
            try
            {
                if (wasAttached && this.part.isAttached == false && parentRCS != null)
                    parentRCS.rcsEnabled = true;
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        protected void findParentRCS(Part parentPart)
        {
            string parentPartName = null;
            ModuleRCS rcsModule = null;

            if (parentPart == null)
                return;

            //Find parent part. If it has RCS, then disable its RCS.
            if (parentPart.partInfo != null)
            {
                parentPartName = parentPart.partInfo.name.Replace('.', '_');
                if (disableRCSOnParts.Contains(parentPartName))
                {
                    rcsModule = parentPart.FindModuleImplementing<ModuleRCS>();
                    if (rcsModule == null)
                        return;
                    rcsModule.rcsEnabled = false;
                    parentRCS = rcsModule;
                    wasAttached = this.part.isAttached;
                }

                else
                {
                    //Walk up the chain
                    findParentRCS(parentPart.parent);
                }
            }
        }
    }
}
