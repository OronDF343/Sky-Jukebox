﻿namespace SkyJukebox.Lib.Xml
{
    public abstract class ValueProperty<T> : PropertyBase<T> where T : struct
    {
        public override T Value
        {
            get
            {
                return (T) (InnerValue ?? (InnerValue = DefaultValue));
            }
            set
            {
                InnerValue = value;
                OnPropertyChanged();
            }
        }

        protected T? InnerValue;
    }
}
