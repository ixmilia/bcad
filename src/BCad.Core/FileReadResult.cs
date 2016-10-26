// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;

namespace BCad
{
    public class FileReadResult
    {
        public Stream Stream { get; private set; }

        public string FileName { get; private set; }

        public FileReadResult(Stream stream, string fileName)
        {
            Stream = stream;
            FileName = fileName;
        }
    }
}
