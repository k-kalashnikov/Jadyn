using System;

namespace Jadyn.Common.Attributes
{
    public class ExcelPropetyAttribute : Attribute
    {
        public string Name { get; set; }
        public bool IsLink { get; set; }
    }
}
