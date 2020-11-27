// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System.Resources;
using System.Runtime.CompilerServices;

namespace System
{
    internal partial class SR
    {
        internal static string Format(string resourceFormat, params object?[]? args)
        {
            return string.Format(resourceFormat, args);
        }
    }
}
