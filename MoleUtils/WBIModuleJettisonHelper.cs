using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

/*
Source code copyrighgt 2015 - 2016, by Michael Billard (Angel-125)
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
    public class WBIModuleJettisonHelper : PartModule
    {
        [KSPField()]
        public string jettisonNames = string.Empty;

        protected Dictionary<string, ModuleJettison> namedJettisons = new Dictionary<string, ModuleJettison>();

        public override void OnStart(StartState state)
        {
            ModuleJettison jettisonModule;
            base.OnStart(state);

            List<ModuleJettison> jettisons = this.part.FindModulesImplementing<ModuleJettison>();

            if (HighLogic.LoadedSceneIsFlight)
            {
                List<AttachNode> doomedNodes = new List<AttachNode>();
                List<ModuleJettison> doomedJettisons = new List<ModuleJettison>();

                //Remove unused jettisons and nodes.
                foreach (ModuleJettison jettison in jettisons)
                    namedJettisons.Add(jettison.jettisonName, jettison);

                foreach (AttachNode node in this.part.attachNodes)
                {
                    if (namedJettisons.Keys.Contains(node.id) && node.attachedPart == null)
                    {
                        doomedNodes.Add(node);
                        doomedJettisons.Add(namedJettisons[node.id]);
                    }
                }

                foreach (AttachNode doomedNode in doomedNodes)
                    this.part.attachNodes.Remove(doomedNode);
                foreach (ModuleJettison doomed in doomedJettisons)
                    this.part.RemoveModule(doomed);
            }

            else if (HighLogic.LoadedSceneIsEditor)
            {
                if (string.IsNullOrEmpty(jettisonNames))
                    return;

                foreach (ModuleJettison jettison in jettisons)
                    namedJettisons.Add(jettison.jettisonName, jettison);

                string[] jettisonPairs = jettisonNames.Split(new char[] { ';' });
                foreach (string jettisonPair in jettisonPairs)
                {
                    string[] names = jettisonPair.Split(new char[] { ',' });
                    jettisonModule = namedJettisons[names[0]];
                    jettisonModule.Fields["shroudHideOverride"].guiName = names[1];
                }
            }
        }
    }
}
