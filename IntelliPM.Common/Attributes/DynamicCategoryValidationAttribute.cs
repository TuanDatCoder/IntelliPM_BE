using System;

namespace IntelliPM.Common.Attributes
{
   
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class DynamicCategoryValidationAttribute : Attribute
    {      
        public string CategoryGroup { get; }

        public bool Required { get; set; } = true;

        public DynamicCategoryValidationAttribute(string categoryGroup)
        {
            if (string.IsNullOrWhiteSpace(categoryGroup))
                throw new ArgumentException("CategoryGroup cannot be null or empty", nameof(categoryGroup));

            CategoryGroup = categoryGroup;
        }
    }
}
