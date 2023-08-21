using System.ComponentModel;

namespace SCPSLBot
{
    public class Config
    {
        [Description("Description of bool config property.")]
        public bool SomeBool { get; set; }
    }
}