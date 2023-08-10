using System.ComponentModel;

namespace TestPlugin
{
    public class Config
    {
        [Description("Description of bool config property.")]
        public bool SomeBool { get; set; }
    }
}