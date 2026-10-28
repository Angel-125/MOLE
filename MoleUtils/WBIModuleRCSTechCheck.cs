using UnityEngine;

/*
Source code copyright 2018, 2026, by Michael Billard (Angel-125)
License: GNU General Public License Version 3
License URL: http://www.gnu.org/licenses/
*/
namespace WildBlueIndustries
{
    /// <summary>
    /// Shows or hides an upgraded RCS model based on the enabled state of the
    /// part's ModuleRCSFX. This is the MOLE-local replacement for the original
    /// WildBlueTools implementation and has no WBIMeshHelper dependency.
    /// </summary>
    [KSPModule("RCS Tech Check")]
    public class WBIModuleRCSTechCheck : PartModule
    {
        [KSPField]
        public string objects = string.Empty;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            ModuleRCSFX rcsModule = part.FindModuleImplementing<ModuleRCSFX>();
            if (rcsModule == null)
                return;

            bool isCareerOrScience = HighLogic.CurrentGame != null &&
                (HighLogic.CurrentGame.Mode == Game.Modes.CAREER ||
                 HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX);

            if (!isCareerOrScience)
            {
                rcsModule.moduleIsEnabled = true;
                rcsModule.enabled = true;
                rcsModule.isEnabled = true;
            }

            setObjectsVisible(rcsModule.moduleIsEnabled);

            isEnabled = false;
            enabled = false;
        }

        private void setObjectsVisible(bool isVisible)
        {
            if (string.IsNullOrEmpty(objects))
                return;

            string[] transformNames = objects.Split(new[] { ',', ';' });
            foreach (string transformNameValue in transformNames)
            {
                string transformName = transformNameValue.Trim();
                if (string.IsNullOrEmpty(transformName))
                    continue;

                Transform[] transforms = part.FindModelTransforms(transformName);
                if (transforms == null)
                    continue;

                foreach (Transform modelTransform in transforms)
                {
                    if (modelTransform != null)
                        modelTransform.gameObject.SetActive(isVisible);
                }
            }
        }
    }
}
