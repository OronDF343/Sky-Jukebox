﻿using System.Xml;

namespace SkyJukebox.Core.Xml
{
    public class BoolProperty : ValueProperty<bool>
    {
        public BoolProperty(bool defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public BoolProperty()
        {

        }

        public override void ReadXml(XmlReader reader)
        {
            Value = reader.ReadElementContentAsBoolean();
        }
    }
}
