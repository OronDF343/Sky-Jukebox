using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SkyJukebox.Lib.Xml
{
    public class GuidProperty2 : ValueProperty2<Guid>
    {
        public GuidProperty2() { }
        public GuidProperty2(Guid defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public override void ReadXml(XmlReader reader)
        {
            var s = reader.ReadContentAsString();
            Value = new Guid(s);
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteString(Value.ToString());
        }
    }
}
