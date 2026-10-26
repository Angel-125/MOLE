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
            get { return HighLogic.CurrentGame.Parameters.CustomParams<MoleUtilsSettings>().enableDebugLogging; }
            set { HighLogic.CurrentGame.Parameters.CustomParams<MoleUtilsSettings>().enableDebugLogging = value; }
        }

        public static bool PayToReconfigure
        {
            get { return HighLogic.CurrentGame.Parameters.CustomParams<MoleUtilsSettings>().payToReconfigure; }
            set { HighLogic.CurrentGame.Parameters.CustomParams<MoleUtilsSettings>().payToReconfigure = value; }
        }

        public static bool RequiresSkillCheck
        {
            get { return HighLogic.CurrentGame.Parameters.CustomParams<MoleUtilsSettings>().requiresSkillCheck; }
            set { HighLogic.CurrentGame.Parameters.CustomParams<MoleUtilsSettings>().requiresSkillCheck = value; }
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
