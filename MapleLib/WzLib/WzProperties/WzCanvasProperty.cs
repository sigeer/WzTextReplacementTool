/*  MapleLib - A general-purpose MapleStory library
 * Copyright (C) 2009, 2010, 2015 Snow and haha01haha01
   
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

namespace MapleLib.WzLib.WzProperties
{
    /// <summary>
    /// A property that can contain sub properties and has one png image
    /// </summary>
    public class WzCanvasProperty : WzExtended, IPropertyContainer
    {
        public override WzPropertyType PropertyType => throw new NotImplementedException();

        public override string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override WzObject Parent { get => throw new NotImplementedException(); internal set => throw new NotImplementedException(); }

        public void AddProperties(WzPropertyCollection props)
        {
            throw new NotImplementedException();
        }

        public void AddProperty(WzImageProperty prop)
        {
            throw new NotImplementedException();
        }

        public void ClearProperties()
        {
            throw new NotImplementedException();
        }

        public override WzImageProperty DeepClone()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public void RemoveProperty(string propertyName)
        {
            throw new NotImplementedException();
        }

        public void RemoveProperty(WzImageProperty prop)
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