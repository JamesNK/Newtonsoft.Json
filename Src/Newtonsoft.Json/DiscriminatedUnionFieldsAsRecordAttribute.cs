﻿using System;

namespace Newtonsoft.Json
{
#if HAVE_FSHARP_TYPES
    /// <summary>
    /// Instructs the <see cref="Converters.DiscriminatedUnionConverter"/> to 
    /// treat fields defined on discriminated union cases as a record with label and value pairs 
    /// instead of an array of values.
    /// </summary>
    /// <remarks>
    /// Attribute intended for use with F# Discriminated Unions
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DiscriminatedUnionFieldsAsRecordAttribute : Attribute { }
#endif
}
