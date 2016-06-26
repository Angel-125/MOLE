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
    public class ModuleBrumbyTextureSwitch : PartModule
    {
        [KSPField(isPersistant = true)]
        public bool useReplacementTexture;

        [KSPField]
        public string replaceTextureForParts = string.Empty;

        [KSPField]
        public string objectTransforms = string.Empty;

        [KSPField]
        public string replacementTexture = string.Empty;

        [KSPField]
        public string replacementNormal = string.Empty;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (HighLogic.LoadedSceneIsEditor)
                GameEvents.onEditorShipModified.Add(vesselModified);

            if (useReplacementTexture)
                setTextures();
        }

        private void OnDestroy()
        {
            if (HighLogic.LoadedSceneIsEditor)
                GameEvents.onEditorShipModified.Remove(vesselModified);
        }

        private void vesselModified(ShipConstruct data)
        {
            foreach (AttachNode node in this.part.attachNodes)
            {
                if (node.attachedPart != null)
                {
                    string attachedPartName = node.attachedPart.partInfo.name.Replace("_", ".");
                    if (replaceTextureForParts.Contains(attachedPartName))
                    {
                        ModuleBrumbyTextureSwitch textureSwitch = node.attachedPart.FindModuleImplementing<ModuleBrumbyTextureSwitch>();
                        if (textureSwitch != null && textureSwitch.useReplacementTexture == false)
                            return;

                        useReplacementTexture = true;
                        setTextures();
                        return;
                    }
                }
            }
        }

        protected void setTextures()
        {
            if (string.IsNullOrEmpty(objectTransforms))
            {
                return;
            }

            char[] delimiters = { ',' };
            string[] transformNames = objectTransforms.Replace(" ", "").Split(delimiters);
            Transform[] targets;
            Texture textureForDecal;
            Renderer rendererMaterial;

            //Sanity checks
            if (transformNames == null)
            {
                ScreenMessages.PostScreenMessage("No transformNames", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            //Go through all the named objects and find their transforms.
            //Then replace their textures.
            foreach (string transformName in transformNames)
            {
                //Get the targets
                targets = part.FindModelTransforms(transformName);
                if (targets == null)
                {
                    ScreenMessages.PostScreenMessage("No targets", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                    continue;
                }

                //Now, replace the textures in each target
                foreach (Transform target in targets)
                {
                    rendererMaterial = target.GetComponent<Renderer>();

                    textureForDecal = GameDatabase.Instance.GetTexture(replacementTexture, false);
                    if (textureForDecal == null)
                        ScreenMessages.PostScreenMessage("can't find replacementTexture", 10.0f, ScreenMessageStyle.UPPER_CENTER);

                    if (textureForDecal != null)
                        rendererMaterial.material.SetTexture("_MainTex", textureForDecal);

                    textureForDecal = GameDatabase.Instance.GetTexture(replacementNormal, false);
                    if (textureForDecal == null)
                        ScreenMessages.PostScreenMessage("can't find replacementNormal", 10.0f, ScreenMessageStyle.UPPER_CENTER);

                    if (textureForDecal != null)
                        rendererMaterial.material.SetTexture("_BumpMap", textureForDecal);
                }
            }
        }
    }
}
