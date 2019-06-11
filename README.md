# The SDSLite part of the project

This is simply SDSLite with one change. Instead of importing the methods from NetCDF.dll using the NuGet package DynamicInterop
they are import using the [DllImport("netcdf.dll", CallingConvention = CallingConvention.Cdecl)] instead. Otherwise it should be
the same as SDSLite found on Github. Clikc this link if you want the regular SDSLite: https://github.com/predictionmachines/SDSlite.
Setup for this is the same as SDSLite.
