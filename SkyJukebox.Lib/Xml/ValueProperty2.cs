using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyJukebox.Lib.Xml
{
    public class ValueProperty2<T> : Property2 where T : struct
    {
        public ValueProperty2() { }
        public ValueProperty2(T defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public override object Value
        {
            get
            {
                return (T)(InnerValue ?? (InnerValue = (T?)DefaultValue));
            }
            set
            {
                InnerValue = (T?)value;
            }
        }

        protected new T? InnerValue;
    }
}
