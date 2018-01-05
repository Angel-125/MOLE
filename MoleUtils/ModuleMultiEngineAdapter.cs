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

Portions of this software use code from the Firespitter plugin by Snjo, used with permission. Thanks Snjo for sharing how to switch meshes. :)

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
namespace WildBlueIndustries
{
    public  class ModuleMultiEngineAdapter : WBIMeshHelper
    {
        [KSPField(isPersistant = true)]
        public int nodeIndex = 0;

        [KSPField(guiActive = false, guiActiveEditor = true, guiName = "Nodes")]
        public string nodeType = " ";

        [KSPField()]
        public string nodes = string.Empty;

        [KSPField()]
        public string labels = string.Empty;

        [KSPField()]
        public string nodePrefix = string.Empty;

        [KSPField()]
        public float nodeRadius;

        [KSPField()]
        public int maxNodes;

        protected int[] nodeValues;
        protected string[] labelValues;

        [KSPEvent(guiActiveEditor = true, guiName = "Toggle Nodes")]
        public void ToggleNodes()
        {
            //Make sure that we don't have any parts attached to the engine mount nodes.
            foreach (AttachNode node in this.part.attachNodes)
            {
                if (node.id.Contains(nodePrefix))
                {
                    if (node.attachedPart != null)
                    {
                        ScreenMessages.PostScreenMessage("Remove all parts attached to the mounts before changing the number of nodes.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                        return;
                    }
                }
            }

            //Increment the index and wrap around as needed.
            nodeIndex = (nodeIndex + 1) % nodeValues.Length;

            //Setup the nodes and GUI
            SetupNodes();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (string.IsNullOrEmpty(nodes) || 
                string.IsNullOrEmpty(labels) || 
                string.IsNullOrEmpty(nodePrefix))
            {
                Events["ToggleNodes"].active = false;
                return;
            }

            //Get the values we need
            char[] delimiter = new char []{ ',' };
            string[] values = nodes.Split(delimiter);
            int index;
            
            //Node values
            nodeValues = new int[values.Length];
            for (index = 0; index < values.Length; index++)
                nodeValues[index] = int.Parse(values[index]);

            //Label values
            labelValues = labels.Split(delimiter);

            //Set up the nodes and GUI
            SetupNodes();

            if (HighLogic.LoadedSceneIsEditor)
                this.part.OnEditorAttach += onEditorAttach;
        }

        protected void onEditorAttach()
        {
            SetupNodes();
        }

        public virtual void SetupNodes()
        {
            int numberOfNodes = nodeValues[nodeIndex];
            float angle;
            Dictionary<string, AttachNode> engineMounts = new Dictionary<string, AttachNode>();
            AttachNode engineMount;

            //Set stack symmetry
            if (numberOfNodes > 0)
                this.part.stackSymmetry = numberOfNodes - 1;
            else
                this.part.stackSymmetry = 0;
            
            //Set the appropriate mesh
            setObject(nodeIndex);

            //Setup GUI
            nodeType = labelValues[nodeIndex];

            //Find the nodes
            foreach (AttachNode node in this.part.attachNodes)
            {
                if (node.id.Contains(nodePrefix))
                {
                    engineMounts.Add(node.id, node);
                }
            }

            //Move em!
            for (int curNode = 1; curNode <= numberOfNodes; curNode++)
            {
                angle = Mathf.PI * 2.0f * (curNode - 1) / numberOfNodes;

                engineMount = engineMounts[nodePrefix + curNode];
                engineMount.position.x = Mathf.Cos(angle) * nodeRadius;
                engineMount.position.z = Mathf.Sin(angle) * nodeRadius;
            }

            //Ditch the rest
            for (int doomedNode = numberOfNodes + 1; doomedNode <= maxNodes; doomedNode++)
            {
                engineMount = engineMounts[nodePrefix + doomedNode];

                if (HighLogic.LoadedSceneIsFlight)
                {
                    this.part.attachNodes.Remove(engineMount);
                }

                else
                {
                    engineMount.position.x = 10000f;
                    engineMount.position.z = 10000f;
                }
            }
        }
    }
}
