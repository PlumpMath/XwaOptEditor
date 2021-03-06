﻿using System;
using System.Globalization;
using System.Windows.Data;
using JeremyAnsel.Xwa.Opt;

namespace XwaOptExplorer.Converters
{
    class VectorScaleConverter : BaseConverter, IValueConverter
    {
        public VectorScaleConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Vector v = (Vector)value;

            return new Vector(v.X * OptFile.ScaleFactor, v.Y * OptFile.ScaleFactor, v.Z * OptFile.ScaleFactor).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Vector v = Vector.Parse((string)value);
                return new Vector(v.X / OptFile.ScaleFactor, v.Y / OptFile.ScaleFactor, v.Z / OptFile.ScaleFactor);
            }
            catch
            {
                return null;
            }
        }
    }
}
