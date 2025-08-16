using System.ComponentModel.DataAnnotations;

namespace IntelliPM.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class DynamicDurationAttribute : Attribute
    {
        private readonly string _configKey;

        public DynamicDurationAttribute(string configKey)
        {
            _configKey = configKey ?? throw new ArgumentNullException(nameof(configKey));
        }

        public string GetConfigKey() => _configKey;
    }
}