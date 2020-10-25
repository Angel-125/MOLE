using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

/*
Source code copyrighgt 2020, by Michael Billard (Angel-125)
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
    public class ModuleSRBThrustEnhancer: PartModule
    {
        [KSPField]
        public float thrustMultiplier = 0f;
    }

    public class WBIThrustProfile
    {
        public string name = string.Empty;
        public float maxThrust = 0f;
        public FloatCurve thrustCurve = new FloatCurve();

        public WBIThrustProfile(ConfigNode node)
        {
            if (node.HasValue("name"))
                name = node.GetValue("name");

            if (node.HasValue("maxThrust"))
                float.TryParse(node.GetValue("maxThrust"), out maxThrust);

            if (node.HasNode("thrustCurve"))
                thrustCurve.Load(node.GetNode("thrustCurve"));
        }
    }

    public class WBIModuleEnginesSRB: ModuleEnginesFX
    {
        #region Fields
        [KSPField]
        public string topNodeName = "top";

        [KSPField]
        public string bottomNodeName = "bottom";

        [KSPField(guiActive = false, guiActiveEditor = true, guiName = "Profile")]
        public string profileName = string.Empty;

        [KSPField(isPersistant = true)]
        public int thrustProfileIndex = 0;
        #endregion

        #region Housekeeping
        public List<WBIThrustProfile> thrustProfiles = new List<WBIThrustProfile>();
        #endregion

        #region Overrides
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            getThrustProfiles();
            setThrustProfile(thrustProfileIndex);
        }

        public override void Activate()
        {
            //Each SRB fuel stack contributes to the engine's thrust. Walk up the chain to see what we got.
            AttachNode topAttachNode = this.part.FindAttachNode(topNodeName);
            AttachNode bottomAttachNode = null;
            Part attachedPart = null;
            Part prevPart = this.part;
            ModuleSRBThrustEnhancer thrustEnhancer = null;
            float maxThrustMultiplier = 0f;
            while (topAttachNode != null)
            {
                attachedPart = topAttachNode.attachedPart;

                bottomAttachNode = attachedPart.FindAttachNode(bottomNodeName);
                if (bottomAttachNode.attachedPart != prevPart)
                    break;
                prevPart = attachedPart;

                thrustEnhancer = attachedPart.FindModuleImplementing<ModuleSRBThrustEnhancer>();
                if (thrustEnhancer == null)
                    break;

                maxThrustMultiplier += thrustEnhancer.thrustMultiplier;
                topAttachNode = attachedPart.FindAttachNode(topNodeName);
            }

            this.maxThrust = this.maxThrust * (1.0f + maxThrustMultiplier);
            this.maxFuelFlow = this.maxThrust / (this.atmosphereCurve.Evaluate(0.0f) * this.g);
            base.Activate();
        }
        #endregion

        #region Events
        [KSPEvent(guiActiveEditor = true, guiName = "Toggle thrust profile")]
        public void ToggleThrustProfile()
        {
            thrustProfileIndex += 1;
            if (thrustProfileIndex >= thrustProfiles.Count)
                thrustProfileIndex = 0;

            setThrustProfile(thrustProfileIndex);
        }
        #endregion

        #region Helpers
        protected void getThrustProfiles()
        {
            if (this.part.partInfo == null || this.part.partInfo.partConfig == null)
                return;
            ConfigNode[] nodes = this.part.partInfo.partConfig.GetNodes("MODULE");
            ConfigNode engineNode = null;
            ConfigNode node = null;
            string moduleName;
            List<string> optionNamesList = new List<string>();

            //Get the switcher config node.
            for (int index = 0; index < nodes.Length; index++)
            {
                node = nodes[index];
                if (node.HasValue("name"))
                {
                    moduleName = node.GetValue("name");
                    if (moduleName == this.ClassName)
                    {
                        engineNode = node;
                        break;
                    }
                }
            }
            if (engineNode == null)
                return;

            //Get the nodes we're interested in
            nodes = engineNode.GetNodes("THRUST_PROFILE");
            if (nodes.Length == 0)
                return;

            WBIThrustProfile profile;
            for (int index = 0; index < nodes.Length; index++)
            {
                profile = new WBIThrustProfile(nodes[index]);
                thrustProfiles.Add(profile);
            }
        }

        protected void setThrustProfile(int profileIndex)
        {
            if (thrustProfiles.Count == 0 || profileIndex >= thrustProfiles.Count)
                return;

            WBIThrustProfile profile = thrustProfiles[profileIndex];

            maxThrust = profile.maxThrust;
            thrustCurve = profile.thrustCurve;

            profileName = profile.name;
        }
        #endregion
    }
}
