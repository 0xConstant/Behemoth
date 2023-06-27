// Make sure to replace "YourProjectNamespace" with your actual project namespace
namespace Behemoth.Properties
{
    [System.Configuration.SettingsSerializeAs(System.Configuration.SettingsSerializeAs.Binary)]
    public sealed partial class Settings : System.Configuration.ApplicationSettingsBase
    {
        [System.Configuration.UserScopedSetting()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [System.Configuration.DefaultSettingValue("120")]
        public int CountdownTime
        {
            get
            {
                try
                {
                    return (int)this["CountdownTime"];
                }
                catch
                {
                    return 120; // Set default value if the setting is not available or cannot be parsed
                }
            }
            set { this["CountdownTime"] = value; }
        }
    }
}
