using MapleLib.WzLib;
using Serilog;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class ImageContext : IDisposable
    {
        public ImageContext(WzImage image)
        {
            Image = image;
        }

        public WzImage Image { get; set; }


        Dictionary<string, PendingItems> _pendingItems = [];


        public void AddNewItem(WzImageProperty item)
        {
            _pendingItems[item.Name] = new(PendingType.NewNode, item);
        }
        //public void HanldeNewItems()
        //{
        //    foreach (var item in _newItems)
        //    {
        //        if (_processItems.Contains(item.Key))
        //            continue;

        //        Image.RemoveProperty(item.Key);
        //        Image.AddProperty(item.Value);
        //        _processItems.Add(item.Key);
        //    }
        //}

        public void AddPendingItem(string nodeName, WzImageProperty property)
        {
            if (_pendingItems.TryGetValue(nodeName, out var data))
            {
                data.SubProps.Add(property);
            }
            else
            {
                _pendingItems[nodeName] = data = new PendingItems(PendingType.PropertyChanged, property);
                data.SubProps.Add(property);
            }
        }

        public bool TryGetPendingItemsByIndex(int index, out string? value)
        {
            value = null;
            if (_pendingItems.Count == 0)
            {
                return false;
            }
            while (index < 0)
            {
                index += _pendingItems.Count;
            }
            while (index >= _pendingItems.Count)
            {
                index -= _pendingItems.Count;
            }

            value = _pendingItems.Keys.ElementAt(index);
            return true;
        }

        internal Dictionary<string, PendingItems> GetUnhandleItems()
        {
            return _pendingItems;
        }

        public void HandlePendingItems(string img, string path, string value)
        {
            if (_pendingItems.TryGetValue(img, out var list))
            {
                var prop = list.SubProps.FirstOrDefault(x => x.FullPath == path);
                if (prop != null)
                {
                    prop.SetValue(value);
                    Image.RemoveProperty(path);
                    Image.AddProperty(prop);
                }
            }
        }

        /// <summary>
        /// 不覆盖属性
        /// </summary>
        /// <returns></returns>
        public bool IsIgnorePropertyOverwrite()
        {
            return Image.Name.Equals("Check.img", StringComparison.OrdinalIgnoreCase) || Image.Name.Equals("Act.img", StringComparison.OrdinalIgnoreCase);
        }

        public void ApplyQuestItemProperty(string imgName, WzImageProperty s1, WzImageProperty? s2)
        {
            foreach (var prop in s1.WzProperties)
            {
                if (prop.PropertyType == WzPropertyType.SubProperty)
                {
                    ApplyQuestItemProperty(imgName, prop, s2?.GetFromPath(prop.Name));
                }
                else if (prop.PropertyType == WzPropertyType.String)
                {
                    SetImgPropertyValue(imgName, s1, s2, prop.Name);
                }
            }
        }

        public void SetImgPropertyValue(string nodeName, WzImageProperty oldItem, WzImageProperty? newItem, string path)
        {
            var iTag = oldItem.GetFromPath(path);

            var newTag = newItem?.GetFromPath(path);

            if (iTag != null)
            {
                if (iTag.PropertyType != WzPropertyType.String)
                {
                    Log.Logger.Verbose("{Path}：当前更新的节点不是String节点，跳过", iTag.FullPath);
                    return;
                }

                if (newTag == null)
                {
                    Log.Logger.Warning("{Path}：用于更新的文件不包含该节点", iTag.FullPath, iTag.WzValue);

                    AddPendingItem(nodeName, iTag);
                }

                else if (iTag.PropertyType == newTag.PropertyType)
                {
                    var isOldHasLetter = iTag.GetString().Any(x => !char.IsDigit(x));
                    var isNewHasLetter = newTag.GetString().Any(x => !char.IsDigit(x));

                    if (isOldHasLetter ^ isNewHasLetter)
                    {
                        AddPendingItem(nodeName, iTag);
                    }
                    else
                    {
                        iTag.SetValue(newTag.WzValue);
                    }
                }

                else
                {
                    AddPendingItem(nodeName, iTag);
                }
            }
            else
            {
                if (newTag == null)
                    return;
                else
                {
                    Log.Logger.Verbose("{Path}：用于更新的文件额外包含这个节点，跳过", newTag.FullPath);
                }
            }
        }
        public void Dispose()
        {
            Image.Dispose();
            _pendingItems.Clear();
        }
    }
}
