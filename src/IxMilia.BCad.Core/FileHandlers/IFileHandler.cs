// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel;
using System.IO;

namespace IxMilia.BCad.FileHandlers
{
    public interface IFileHandler
    {
        bool ReadDrawing(string fileName, Stream fileStream, out Drawing drawing, out ViewPort viewPort);

        bool WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, INotifyPropertyChanged fileSettings);
        INotifyPropertyChanged GetFileSettingsFromDrawing(Drawing drawing);
    }
}
