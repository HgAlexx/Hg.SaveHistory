namespace Hg.SaveHistory.API
{
    public class EngineSetting
    {
        #region Fields & Properties

        public int Index;
        public string Caption { get; set; }
        public string Description { get; set; }
        public string HelpTooltip { get; set; }
        public EngineSettingKind Kind { get; set; }
        public string Name { get; set; }

        #endregion

        #region Members

        public EngineSetting()
        {
            Kind = EngineSettingKind.Setup;
        }

        #endregion
    }
}