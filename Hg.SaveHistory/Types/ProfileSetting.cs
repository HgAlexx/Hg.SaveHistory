using Hg.SaveHistory.API;

namespace Hg.SaveHistory.Types
{
    public abstract class ProfileSetting
    {
        #region Fields & Properties

        public EngineSettingKind Kind;
        public string Name;

        #endregion
    }
}