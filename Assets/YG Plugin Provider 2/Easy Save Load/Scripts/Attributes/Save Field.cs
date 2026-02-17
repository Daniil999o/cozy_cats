using System;

namespace EasySaveLoad
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SaveField : Attribute
    {
        public string customKey;

        public SaveField() { }

        public SaveField(string customKey) => this.customKey = customKey;
    }
}