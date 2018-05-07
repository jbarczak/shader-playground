﻿using System;
using System.IO;

namespace ShaderPlayground.Core.Util
{
    internal sealed class TempFile : IDisposable
    {
        public string FilePath { get; }

        public TempFile()
        {
            FilePath = Path.GetTempFileName();
        }

        public void Dispose()
        {
            File.Delete(FilePath);
        }

        public static implicit operator string(TempFile tf) => tf.FilePath;
    }
}