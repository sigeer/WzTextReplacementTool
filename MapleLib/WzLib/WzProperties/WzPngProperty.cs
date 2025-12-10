/*  MapleLib - A general-purpose MapleStory library
 * Copyright (C) 2009, 2010, 2015 Snow and haha01haha01
 * 2018 - 2025, lastbattle
   
 * This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

 * This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

 * You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/

using MapleLib.WzLib.Util;
using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;

namespace MapleLib.WzLib.WzProperties
{
    /// <summary>
    /// A property that contains the information for a bitmap
    /// https://docs.microsoft.com/en-us/windows/win32/direct3d9/compressed-texture-resources
    /// http://www.sjbrown.co.uk/2006/01/19/dxt-compression-techniques/
    /// https://en.wikipedia.org/wiki/S3_Texture_Compression
    /// </summary>
    public class WzPngProperty : WzImageProperty
    {
        public override WzPropertyType PropertyType => throw new NotImplementedException();

        public override string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override WzObject Parent { get => throw new NotImplementedException(); internal set => throw new NotImplementedException(); }

        public override WzImageProperty DeepClone()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object value)
        {
            throw new NotImplementedException();
        }

        public override void WriteValue(WzBinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}