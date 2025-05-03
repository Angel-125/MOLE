using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;
using WBIResources;
using KSP.Localization;

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
    internal struct ConsumptionResource
    {
        public Vessel vessel;
        public ModuleResource resource;
    }

    public class WBIWetWorkshop : ExtendedPartModule, IModuleInfo
    {
        #region Fields
        [KSPField]
        public string managedObjects = string.Empty;

        [KSPField(isPersistant = true)]
        public bool isDeployed;

        [KSPField]
        public int crewCapacity;

        [KSPField]
        public string convertToTankName = "Convert to fuel tank";

        [KSPField]
        public string convertToWorkshopName = "Convert to workshop";

        [KSPField]
        public bool allowTankConversionInFlight;

        #region Requirements
        /// <summary>
        /// The skill required to unpack the part.
        /// </summary>
        [KSPField]
        public string skillToCheck = string.Empty;

        /// <summary>
        /// The minimum skill level required to unpack the box. Default is 0.
        /// </summary>
        [KSPField]
        public int minimumSkillLevel = 0;

        /// <summary>
        /// Flag to indicate whether or not when checking resources,
        /// resources can come from other vessels.
        /// Default is true.
        /// </summary>
        [KSPField]
        public bool canUseRemoteResources = true;
        #endregion

        #endregion

        #region Housekeeping
        bool debugMode;
        bool payToReconfigure;
        bool requiresSkillCheck;
        List<ConsumptionResource> consumptionResources;
        #endregion

        #region IModuleInfo
        public string GetModuleTitle()
        {
            return Localizer.Format("#LOC_MOLE_shopTitle");
        }

        public Callback<Rect> GetDrawModulePanelCallback()
        {
            return null;
        }

        public string GetPrimaryField()
        {
            return "";
        }

        public override string GetModuleDisplayName()
        {
            return GetModuleTitle();
        }

        public override string GetInfo()
        {
            StringBuilder info = new StringBuilder();

            info.AppendLine(Localizer.Format("#LOC_MOLE_shopDesc"));
            if (resHandler.inputResources.Count > 0)
            {
                info.AppendLine(" ");
                info.AppendLine(Localizer.Format("#LOC_MOLE_shopRequires"));
                int count = resHandler.inputResources.Count;
                for (int index = 0; index < count; index++)
                {
                    info.AppendLine("<color=white>- " + resHandler.inputResources[index].resourceDef.displayName + ": " + resHandler.inputResources[index].amount.ToString("n2") + "</color>");
                }
            }

            return info.ToString();
        }
        #endregion

        #region Overrides
        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight)
                return;

            Events["ConvertToTank"].guiName = convertToTankName;
            Events["ConvertToWorkshop"].guiName = convertToWorkshopName;

            if (HighLogic.LoadedSceneIsEditor)
            {
                if (isDeployed)
                {
                    Events["ConvertToTank"].active = true;
                    Events["ConvertToWorkshop"].active = false;
                }
                else
                {
                    Events["ConvertToTank"].active = false;
                    Events["ConvertToWorkshop"].active = true;
                }

                updateManagedModules();
                updateManagedResources();
            }
        }

        public override void OnAwake()
        {
            base.OnAwake();
            debugMode = WBIResourcesSettings.EnableDebugLogging;
            payToReconfigure = WBIResourcesSettings.PayToReconfigure;
            requiresSkillCheck = WBIResourcesSettings.RequiresSkillCheck;
        }
        #endregion

        #region Events
        [KSPEvent(guiName = "Convert to workshop", externalToEVAOnly = false, guiActiveUnfocused = true, unfocusedRange = 10, guiActiveEditor = true)]
        public virtual void ConvertToWorkshop()
        {
            if (!canReconfigure())
                return;

            isDeployed = true;
            consumeResources();

            if (HighLogic.LoadedSceneIsEditor)
            {
                Events["ConvertToTank"].active = true;
                Events["ConvertToWorkshop"].active = false;
            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                Events["ConvertToTank"].active = allowTankConversionInFlight;
                Events["ConvertToWorkshop"].active = false;
                part.CrewCapacity = crewCapacity;
                part.SpawnIVA();
            }

            updateManagedModules();
            updateManagedResources();
            setObjectsVisible(true);

            //Dirty the GUI
            MonoUtilities.RefreshContextWindows(part);
        }

        [KSPEvent(guiName = "Convert to fuel tank", externalToEVAOnly = false, guiActiveUnfocused = true, unfocusedRange = 10, guiActiveEditor = true)]
        public virtual void ConvertToTank()
        {
            isDeployed = false;
            consumeResources();

            if (HighLogic.LoadedSceneIsEditor)
            {
                Events["ConvertToTank"].active = false;
                Events["ConvertToWorkshop"].active = true;
            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                Events["ConvertToTank"].active = false;
                Events["ConvertToWorkshop"].active = true;
                part.CrewCapacity = 0;
                part.DespawnIVA();
            }

            updateManagedModules();
            updateManagedResources();
            setObjectsVisible(false);

            //Dirty the GUI
            MonoUtilities.RefreshContextWindows(part);
        }
        #endregion

        #region Helpers
        bool canReconfigure()
        {
            // Check for skill requirements
            if (!hasRequiredSkill())
                return false;

            // Check for resource requirements
            if (!hasRequiredResources())
                return false;

            return true;
        }

        bool hasRequiredResources()
        {
            consumptionResources = new List<ConsumptionResource>();

            if (HighLogic.LoadedSceneIsEditor || !payToReconfigure || resHandler.inputResources.Count <= 0)
                return true;

            int count = resHandler.inputResources.Count;
            double amount, maxAmount;
            ModuleResource moduleResource;
            Vessel supplyVessel;
            int loadedVesselCount;
            bool resourceIsAvailable;
            ConsumptionResource consumptionResource;
            for (int index = 0; index < count; index++)
            {
                moduleResource = resHandler.inputResources[index];
                resourceIsAvailable = false;

                // Check locally first.
                vessel.GetConnectedResourceTotals(moduleResource.id, out amount, out maxAmount);
                if (amount >= moduleResource.amount)
                {
                    // Add the resource and vessel to our resource consumption list.
                    consumptionResource = new ConsumptionResource();
                    consumptionResource.vessel = vessel;
                    consumptionResource.resource = moduleResource;
                    consumptionResources.Add(consumptionResource);
                    resourceIsAvailable = true;
                }

                // Not locally found. Check remote vessels
                else if (canUseRemoteResources)
                {
                    // If the supplyVessel has enough of the resource then add the vessel and resource to our consumption list.
                    loadedVesselCount = FlightGlobals.VesselsLoaded.Count;
                    for (int loadedVesselIndex = 0; loadedVesselIndex < loadedVesselCount; loadedVesselIndex++)
                    {
                        supplyVessel = FlightGlobals.VesselsLoaded[loadedVesselIndex];
                        if (supplyVessel == part.vessel)
                            continue;

                        supplyVessel.GetConnectedResourceTotals(moduleResource.id, out amount, out maxAmount);
                        if (amount >= moduleResource.amount)
                        {
                            // Add the resource and vessel to our resource consumption list.
                            consumptionResource = new ConsumptionResource();
                            consumptionResource.vessel = supplyVessel;
                            consumptionResource.resource = moduleResource;
                            consumptionResources.Add(consumptionResource);
                            resourceIsAvailable = true;
                            break;
                        }
                    }
                }

                // Final check
                if (!resourceIsAvailable)
                {
                    string[] messages = { string.Format("{0:n1}", moduleResource.amount), moduleResource.resourceDef.displayName, Events["Toggle"].guiName };
                    string errorMessage = Localizer.Format("#LOC_MOLE_missingResource", messages);
                    ScreenMessages.PostScreenMessage(errorMessage, 5.0f, ScreenMessageStyle.UPPER_CENTER);
                    return false;
                }
            }

            return true;
        }

        bool hasRequiredSkill()
        {
            if (!requiresSkillCheck || string.IsNullOrEmpty(skillToCheck))
                return true;

            ProtoCrewMember astronaut;
            int highestSkill;

            // Make sure that we have sufficient skill.
            if (vessel.isEVA)
                highestSkill = getHighestRank(vessel, out astronaut);
            else
                highestSkill = getHighestRank(vessel, out astronaut);

            if (astronaut == null || highestSkill < minimumSkillLevel)
            {
                string[] messages = { skillToCheck, convertToWorkshopName };
                string errorMessage = Localizer.Format("#LOC_MOLE_missingSkill", messages);
                ScreenMessages.PostScreenMessage(errorMessage, 5.0f, ScreenMessageStyle.UPPER_CENTER);
                return false;
            }

            return true;
        }

        int getHighestRank(Vessel vessel, out ProtoCrewMember astronaut)
        {
            astronaut = null;
            if (string.IsNullOrEmpty(skillToCheck))
                return 0;
            try
            {
                if (vessel.GetCrewCount() == 0)
                    return 0;
            }
            catch
            {
                return 0;
            }

            string[] skillsToCheck = skillToCheck.Split(new char[] { ';' });
            string checkSkill;
            int highestRank = 0;
            int crewRank = 0;
            bool hasABadass = false;
            bool hasAVeteran = false;
            bool hasAHero = false;
            for (int skillIndex = 0; skillIndex < skillsToCheck.Length; skillIndex++)
            {
                checkSkill = skillsToCheck[skillIndex];

                //Find the highest racking kerbal with the desired skill (if any)
                ProtoCrewMember[] vesselCrew = vessel.GetVesselCrew().ToArray();
                for (int index = 0; index < vesselCrew.Length; index++)
                {
                    if (vesselCrew[index].HasEffect(checkSkill))
                    {
                        if (vesselCrew[index].isBadass)
                            hasABadass = true;
                        if (vesselCrew[index].veteran)
                            hasAVeteran = true;
                        if (vesselCrew[index].isHero)
                            hasAHero = true;
                        crewRank = vesselCrew[index].experienceTrait.CrewMemberExperienceLevel();
                        if (crewRank > highestRank)
                        {
                            highestRank = crewRank;
                            astronaut = vesselCrew[index];
                        }
                    }
                }
            }

            if (hasABadass)
                highestRank += 1;
            if (hasAVeteran)
                highestRank += 1;
            if (hasAHero)
                highestRank += 1;

            return highestRank;
        }

        void consumeResources()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            // Give resources back
            if (isDeployed && resHandler.outputResources.Count > 0)
            {
                int resCount = resHandler.outputResources.Count;
                for (int index = 0; index < resCount; index++)
                {
                    part.RequestResource(resHandler.outputResources[index].resourceDef.name, -resHandler.outputResources[index].amount, ResourceFlowMode.STAGE_PRIORITY_FLOW, false);
                }
                return;
            }

            // Consume resournces
            int count = consumptionResources.Count;
            if (count <= 0)
                return;

            ConsumptionResource consumptionResource;
            for (int index = 0; index < count; index++)
            {
                consumptionResource = consumptionResources[index];
                consumptionResource.vessel.RequestResource(consumptionResource.vessel.rootPart, consumptionResource.resource.id, consumptionResource.resource.amount, true);
            }
        }

        string getResourceInfo(ModuleResource resource)
        {
            PartResourceDefinition definition = PartResourceLibrary.Instance.GetDefinition(resource.name.GetHashCode());
            string resourceName = definition == null ? resource.title : definition.displayName;

            return Localizer.Format("#LOC_MOLE_resourceInfo", new string[2] { resourceName, string.Format("{0:n2}", resource.amount) });
        }

        public void setObjectsVisible(bool isVisible)
        {
            if (string.IsNullOrEmpty(managedObjects))
                return;

            char[] delimiters = { ',' };
            string[] transformNames = managedObjects.Replace(" ", "").Split(delimiters);
            Transform[] targets;

            //Sanity checks
            if (transformNames == null || transformNames.Length == 0)
            {
                Debug.Log("transformNames are null");
                return;
            }

            //Go through all the named panels and find their transforms.
            foreach (string transformName in transformNames)
            {
                //Get the targets
                targets = part.FindModelTransforms(transformName);
                if (targets == null)
                {
                    Debug.Log("No targets found for " + transformName);
                    continue;
                }

                foreach (Transform target in targets)
                {
                    target.gameObject.SetActive(isVisible);
                    Collider collider = target.gameObject.GetComponent<Collider>();
                    if (collider != null)
                        collider.enabled = isVisible;
                }
            }
        }

        void updateManagedModules()
        {
            ConfigNode node = getPartConfigNode();
            if (node == null)
                return;
            List<PartModule> managedModules = getManagedModules(node);
            int count = managedModules.Count;
            if (count <= 0)
                return;

            PartModule module;
            for (int index = 0; index < count; index++)
            {
                module = managedModules[index];

                if (isDeployed)
                {
                    module.moduleIsEnabled = true;
                    module.enabled = true;
                    module.isEnabled = true;
                    module.OnActive();
                    if (module is BaseConverter)
                    {
                        BaseConverter converter = (BaseConverter)module;
                        converter.EnableModule();
                    }
                }
                else
                {
                    module.OnInactive();
                    module.isEnabled = false;
                    module.enabled = false;
                    module.moduleIsEnabled = false;
                    if (module is BaseConverter)
                    {
                        BaseConverter converter = (BaseConverter)module;
                        converter.DisableModule();
                    }
                }
            }

            //Dirty the GUI
            MonoUtilities.RefreshContextWindows(part);
        }

        void updateManagedResources()
        {
            if (debugMode && part.partInfo != null)
                Debug.Log("[WBIWetWorkshop] - updateManagedResources called for " + part.partInfo.title);

            ConfigNode node = getPartConfigNode();
            if (node == null)
            {
                if (debugMode)
                    Debug.Log("[WBIWetWorkshop] - part config node not found");
                return;
            }

            if (!node.HasNode("MANAGED_RESOURCE"))
            {
                if (debugMode)
                    Debug.Log("[WBIWetWorkshop] - MANAGED_RESOURCE not found");
                return;
            }

            ConfigNode[] resourceNodes = node.GetNodes("MANAGED_RESOURCE");
            ConfigNode resourceNode;
            string resourceName;
            double maxAmount;
            double amount;
            bool isTankResource = false;
            for (int index = 0; index < resourceNodes.Length; index++)
            {
                resourceNode = resourceNodes[index];
                if (resourceNode.HasValue("name") == false || resourceNode.HasValue("maxAmount") == false)
                {
                    if (debugMode)
                        Debug.Log("[WBIWetWorkshop] - MANAGED_RESOURCE: name or maxAmount not found");
                    continue;
                }

                isTankResource = false;
                if (resourceNode.HasValue("isTankResource"))
                    bool.TryParse(resourceNode.GetValue("isTankResource"), out isTankResource);

                // Remove the resource if the part isn't deployed.
                resourceName = resourceNode.GetValue("name");
                if (part.Resources.Contains(resourceName) && ((isDeployed == false && isTankResource == false) || (isDeployed && isTankResource)))
                {
                    if (debugMode)
                        Debug.Log("[WBIWetWorkshop] - removing resource: " + resourceName);

                    part.Resources.Remove(resourceName);

                    continue;
                }

                maxAmount = 0;
                if (double.TryParse(resourceNode.GetValue("maxAmount"), out maxAmount) == false)
                {
                    if (debugMode)
                        Debug.Log("[WBIWetWorkshop] - cannot parse maxAmount from managed resource named " + resourceName);
                    continue;
                }

                // Add resource if needed.
                if (part.Resources.Contains(resourceName) == false && isDeployed)
                {
                    if (debugMode)
                        Debug.Log("[WBIWetWorkshop] - adding resource: " + resourceName);

                    amount = 0;
                    if (HighLogic.LoadedSceneIsEditor)
                        amount = maxAmount;

                    part.Resources.Add(resourceName, amount, maxAmount, true, true, false, true, PartResource.FlowMode.Both);
                }
            }

            //Dirty the GUI
            MonoUtilities.RefreshContextWindows(part);
        }


        List<PartModule> getManagedModules(ConfigNode node)
        {
            List<PartModule> managedModules = new List<PartModule>();
            if (!node.HasNode("MANAGED_MODULES"))
            {
                if (debugMode)
                    Debug.Log("[WBIWetWorkshop] - MANAGED_MODULES not found");
                return managedModules;
            }
            ConfigNode modulesNode = node.GetNode("MANAGED_MODULES");

            string[] moduleNames = modulesNode.GetValues("moduleName");
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < moduleNames.Length; index++)
                builder.Append(moduleNames[index] + ";");
            string managedModuleNames = builder.ToString();
            if (debugMode)
                Debug.Log("[WBIWetWorkshop] - Potential modules to manage: " + managedModuleNames);

            int count = part.Modules.Count;
            PartModule partModule;
            for (int index = 0; index < count; index++)
            {
                partModule = part.Modules[index];
                if (managedModuleNames.Contains(partModule.moduleName))
                    managedModules.Add(partModule);
            }

            if (debugMode)
                Debug.Log("[WBIWetWorkshop] - found " + managedModules.Count + " modules to manage.");
            return managedModules;
        }

        #endregion
    }
}
