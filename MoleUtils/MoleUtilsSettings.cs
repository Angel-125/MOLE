/*
Source code copyright 2018, by Michael Billard (Angel-125)
License: GPLV3
*/
namespace WildBlueIndustries
{
    public class MoleUtilsSettings : GameParameters.CustomParameterNode
    {
        [GameParameters.CustomParameterUI("Require resources to reconfigure", toolTip = "If enabled, you'll need resources to reconfigure your parts.", autoPersistance = true)]
        public bool payToReconfigure = true;

        [GameParameters.CustomParameterUI("Require skills to reconfigure", toolTip = "If enabled, you need skills to reconfigure parts.", autoPersistance = true)]
        public bool requiresSkillCheck = true;

        [GameParameters.CustomParameterUI("Enable Debug Logging", toolTip = "If enabled, your logs will be spammed with debug info.", autoPersistance = true)]
        public bool enableDebugLogging = false;

        public static bool EnableDebugLogging
        {
            get
            {
                MoleUtilsSettings settings = CurrentSettings;
                return settings != null ? settings.enableDebugLogging : false;
            }
            set
            {
                MoleUtilsSettings settings = CurrentSettings;
                if (settings != null)
                    settings.enableDebugLogging = value;
            }
        }

        public static bool PayToReconfigure
        {
            get
            {
                MoleUtilsSettings settings = CurrentSettings;
                return settings != null ? settings.payToReconfigure : true;
            }
            set
            {
                MoleUtilsSettings settings = CurrentSettings;
                if (settings != null)
                    settings.payToReconfigure = value;
            }
        }

        public static bool RequiresSkillCheck
        {
            get
            {
                MoleUtilsSettings settings = CurrentSettings;
                return settings != null ? settings.requiresSkillCheck : true;
            }
            set
            {
                MoleUtilsSettings settings = CurrentSettings;
                if (settings != null)
                    settings.requiresSkillCheck = value;
            }
        }

        private static MoleUtilsSettings CurrentSettings
        {
            get
            {
                if (HighLogic.CurrentGame == null || HighLogic.CurrentGame.Parameters == null)
                    return null;

                return HighLogic.CurrentGame.Parameters.CustomParams<MoleUtilsSettings>();
            }
        }

        public override string DisplaySection
        {
            get { return Section; }
        }

        public override string Section
        {
            get { return "MOLE"; }
        }

        public override string Title
        {
            get { return "Reconfigure & Repair"; }
        }

        public override int SectionOrder
        {
            get { return 1; }
        }

        public override GameParameters.GameMode GameMode
        {
            get { return GameParameters.GameMode.ANY; }
        }

        public override bool HasPresets
        {
            get { return false; }
        }
    }
}
