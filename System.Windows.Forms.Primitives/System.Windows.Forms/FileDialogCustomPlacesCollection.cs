// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using System.IO;

namespace System.Windows.Forms
{
    public class FileDialogCustomPlacesCollection : Collection<FileDialogCustomPlace>
    {
        public void Add(string? path) => Add(new FileDialogCustomPlace(path));

        public void Add(Guid knownFolderGuid) => Add(new FileDialogCustomPlace(knownFolderGuid));
    }
}