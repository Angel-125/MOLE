using System.Linq;
using UnityEngine;

/*
Source code copyright 2026, by Michael Billard (Angel-125)
License: GNU General Public License Version 3
License URL: http://www.gnu.org/licenses/

Wild Blue Industries is trademarked by Michael Billard and may be used for non-commercial purposes. All other rights reserved.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.
*/
namespace WildBlueIndustries
{
    /// <summary>
    /// MOLE-local animated light module. This replaces the subset of WBIResources.WBILight
    /// used by MOLE's station light and drydocks without requiring WBIResources.
    /// </summary>
    [KSPModule("Light")]
    public class WBIModuleLight : PartModule
    {
        private const int DefaultAnimationLayer = 3;

        [KSPField]
        public int animationLayer = DefaultAnimationLayer;

        [KSPField]
        public string animationName = string.Empty;

        [KSPField]
        public string startEventGUIName = "Light On";

        [KSPField]
        public string endEventGUIName = "Light Off";

        [KSPField(isPersistant = true)]
        public bool guiIsVisible = true;

        [KSPField(isPersistant = true)]
        public bool isDeployed;

        [KSPField]
        public string colorPanelName = string.Empty;

        [KSPField(isPersistant = true)]
        public double ecRequired;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "red")]
        [UI_FloatRange(stepIncrement = 0.05f, maxValue = 1f, minValue = 0f)]
        public float red = 1f;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "green")]
        [UI_FloatRange(stepIncrement = 0.05f, maxValue = 1f, minValue = 0f)]
        public float green = 1f;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "blue")]
        [UI_FloatRange(stepIncrement = 0.05f, maxValue = 1f, minValue = 0f)]
        public float blue = 1f;

        [KSPField]
        public float intensity = 1f;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "level")]
        [UI_FloatRange(stepIncrement = 0.05f, maxValue = 1f, minValue = 0f)]
        public float level = 1f;

        private Animation animation;
        private Light[] lights;
        private float previousRed;
        private float previousGreen;
        private float previousBlue;
        private float previousLevel;
        private WBIModuleLightFixedUpdateHelper fixedUpdateHelper;

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Toggle Lights", active = true,
            externalToEVAOnly = false, unfocusedRange = 3f, guiActiveUnfocused = true)]
        public void ToggleLights()
        {
            setLightState(!isDeployed, true);
        }

        [KSPAction("Toggle Lights", KSPActionGroup.Light)]
        public void ToggleLightsAction(KSPActionParam param)
        {
            ToggleLights();
        }

        public void TurnOnLights()
        {
            if (!isDeployed)
                setLightState(true, true);
        }

        public void TurnOffLights()
        {
            if (isDeployed)
                setLightState(false, true);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight)
                return;

            configureFields();
            setupAnimation();
            lights = part.gameObject.GetComponentsInChildren<Light>();
            setupLights();

            Events["ToggleLights"].guiActive = guiIsVisible;
            Events["ToggleLights"].guiActiveEditor = guiIsVisible;
            Events["ToggleLights"].guiActiveUnfocused = guiIsVisible;
            updateEventName();

            if (fixedUpdateHelper == null)
            {
                fixedUpdateHelper = part.gameObject.AddComponent<WBIModuleLightFixedUpdateHelper>();
                fixedUpdateHelper.onFixedUpdate = onFixedUpdate;
            }
            fixedUpdateHelper.enabled = true;
        }

        private void configureFields()
        {
            if (part.partInfo == null || part.partInfo.partConfig == null)
                return;

            ConfigNode moduleNode = part.partInfo.partConfig.GetNodes("MODULE")
                .FirstOrDefault(node => node.GetValue("name") == ClassName);
            if (moduleNode == null)
                return;

            setFieldVisibility("red", moduleNode.HasValue("red"));
            setFieldVisibility("green", moduleNode.HasValue("green"));
            setFieldVisibility("blue", moduleNode.HasValue("blue"));
            setFieldVisibility("level", moduleNode.HasValue("intensity"));
        }

        private void setFieldVisibility(string fieldName, bool isVisible)
        {
            BaseField field = Fields[fieldName];
            if (field == null)
                return;

            field.guiActive = isVisible;
            field.guiActiveEditor = isVisible;
        }

        private void setupAnimation()
        {
            if (string.IsNullOrEmpty(animationName))
                return;

            Animation[] animators = part.FindModelAnimators(animationName);
            if (animators == null || animators.Length == 0)
                return;

            animation = animators[0];
            AnimationState state = animation[animationName];
            if (state == null)
            {
                animation = null;
                return;
            }

            state.layer = animationLayer;
            state.normalizedTime = isDeployed ? 1f : 0f;
            state.speed = isDeployed ? 10000f : -10000f;
            animation.Play(animationName);
        }

        private void setLightState(bool turnOn, bool playAnimation)
        {
            isDeployed = turnOn;

            if (playAnimation && animation != null)
            {
                AnimationState state = animation[animationName];
                if (state != null)
                {
                    state.speed = turnOn ? 1f : -1f;
                    if (!turnOn)
                        state.time = state.length;
                    animation.Play(animationName);
                }
            }

            updateEventName();
            setupLights();
        }

        private void updateEventName()
        {
            Events["ToggleLights"].guiName = isDeployed ? endEventGUIName : startEventGUIName;
        }

        private void onFixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight && isDeployed && ecRequired > 0d)
            {
                double requested = ecRequired * TimeWarp.fixedDeltaTime;
                if (requested > 0d)
                {
                    double received = part.RequestResource("ElectricCharge", requested, ResourceFlowMode.ALL_VESSEL);
                    if (received / requested < 0.999d)
                        TurnOffLights();
                }
            }

            if (previousRed != red || previousGreen != green || previousBlue != blue || previousLevel != level)
                setupLights();
        }

        private void setupLights()
        {
            float selectedIntensity = intensity * level;
            Color color = new Color(red, green, blue, selectedIntensity);

            if (lights != null)
            {
                foreach (Light light in lights)
                {
                    if (light == null)
                        continue;
                    light.color = color;
                    light.intensity = isDeployed ? selectedIntensity : 0f;
                }
            }

            previousRed = red;
            previousGreen = green;
            previousBlue = blue;
            previousLevel = level;

            if (!isDeployed || string.IsNullOrEmpty(colorPanelName))
                return;

            Transform[] targets = part.FindModelTransforms(colorPanelName);
            if (targets == null)
                return;

            foreach (Transform target in targets)
            {
                if (target == null)
                    continue;
                Renderer renderer = target.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.SetColor("_EmissiveColor", color);
            }
        }
    }

    public class WBIModuleLightFixedUpdateHelper : MonoBehaviour
    {
        public Callback onFixedUpdate;

        public void FixedUpdate()
        {
            if (onFixedUpdate != null)
                onFixedUpdate();
        }
    }
}
