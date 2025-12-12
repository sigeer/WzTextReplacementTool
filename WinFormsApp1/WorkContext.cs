using MapleLib.WzLib;
using WeifenLuo.WinFormsUI.Docking;

namespace WinFormsApp1
{
    internal class WorkContext
    {
        internal static WorkContext? Instance { get; set; }
        public WorkContext(WzFile sourceFile)
        {
            SourceFile = sourceFile;
        }

        public WzFile SourceFile { get; }

        public Dictionary<string, WzImage?> NewData = [];
        public Dictionary<string, ImageContext?> FinalData = [];

        public void SetNewData(WzFile file)
        {
            foreach (var item in NewData)
            {
                item.Value?.Dispose();
            }

            foreach (var item in file.WzDirectory.WzImages)
            {
                NewData[item.Name]?.Dispose();
                NewData[item.Name] = item;

                FinalData[item.Name]?.Dispose();
                FinalData[item.Name] = new ImageContext(
                    SourceFile.WzDirectory.GetImageByName(item.Name).DeepClone());

                ApplyQuestImage(FinalData[file.Name]!, NewData[file.Name]!);
            }
        }

        public void SetNewData(WzImage file)
        {
            NewData.GetValueOrDefault(file.Name)?.Dispose();
            FinalData.GetValueOrDefault(file.Name)?.Dispose();

            NewData[file.Name] = file;
            FinalData[file.Name] = new ImageContext(
                SourceFile.WzDirectory.GetImageByName(file.Name).DeepClone());

            ApplyQuestImage(FinalData[file.Name]!, NewData[file.Name]!);
        }

        void ApplyQuestImage(ImageContext context, WzImage newData)
        {
            var allData = context.Image.WzProperties.Select(x => x.Name)
                .Union(newData.WzProperties.Select(x => x.Name)).OrderBy(x => x).ToHashSet();

            foreach (var item in allData)
            {
                var targetItem = context.Image[item];
                var newItem = newData[item]?.DeepClone();

                if (targetItem == null && newItem != null)
                {
                    context.AddNewItem(newItem);
                }
                else if (targetItem != null)
                {
                    if (newItem != null)
                    {
                        bool suspect = false;
                        if (targetItem.WzProperties.Count != newItem.WzProperties.Count)
                        {
                            context.AddPendingItem(item, targetItem);
                            suspect = true;
                        }
                        foreach (var prop in targetItem.WzProperties)
                        {
                            if (targetItem.GetFromPath(prop.Name)?.PropertyType != prop.PropertyType)
                            {
                                context.AddPendingItem(item, prop);
                                suspect = true;
                                break;
                            }
                        }

                        if (!suspect && !context.IsIgnorePropertyOverwrite())
                        {
                            context.ApplyQuestItemProperty(item, targetItem, newItem);
                        }
                    }
                }
            }
        }
    }
}
