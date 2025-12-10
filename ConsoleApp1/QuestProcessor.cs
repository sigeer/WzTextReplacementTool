using MapleLib.WzLib;
using Serilog;
using Spectre.Console;

namespace ConsoleApp1
{
    /// <summary>
    /// 仅用于替换Quest中的文本 （QuestInfo.img, Say.img, PQuest.img）
    /// </summary>
    public class QuestProcessor
    {
        public static string[] versions = Enum.GetNames<WzMapleVersion>();
        public static void Run()
        {
            var inputWZPath = AnsiConsole.Prompt(new TextPrompt<string>("要替换的Quest.wz文件："));

            var inputMajorVersion = Enum.Parse<WzMapleVersion>(AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("请选择该文件的类型：")
                    .AddChoices(versions)));

            var inputGameVersion = AnsiConsole.Prompt(new TextPrompt<short>("版本：").DefaultValue((short)83).ShowDefaultValue(true));

            var oldWz = new WzFile(inputWZPath, inputGameVersion, inputMajorVersion);
            oldWz.ParseWzFile();

            using var context = new UpdateContext(oldWz);

            while (true)
            {
                context.Handle();

                if (!AnsiConsole.Prompt(new ConfirmationPrompt("继续选择文件更新？否则输出更新后的文件")))
                {
                    break;
                }
            }
            context.Output();
        }


    }

    public class UpdateImageContext : IDisposable
    {
        public UpdateImageContext(WzImage image)
        {
            Image = image;
        }

        public WzImage Image { get; set; }
        Dictionary<string, WzImageProperty> _newItems = [];
        HashSet<string> _processItems = [];

        Dictionary<string, (WzImageProperty Original, WzImageProperty Newly)> _suspectItems = [];

        public void AddNewItem(WzImageProperty item)
        {
            _newItems[item.Name] = item;
            _processItems.Remove(item.Name);
        }
        public void HanldeNewItems()
        {
            foreach (var item in _newItems)
            {
                if (_processItems.Contains(item.Key))
                    continue;

                Image.RemoveProperty(item.Value);
                Image.AddProperty(item.Value);
                _processItems.Add(item.Key);
            }
        }

        public void AddSuspectItem(string name, WzImageProperty original, WzImageProperty newly)
        {
            _suspectItems[name] = new(original, newly);
            _processItems.Remove(name);
        }

        public void HandleSuspectItems()
        {
            var needHandles = _suspectItems.Keys.Except(_processItems).ToArray();
            if (needHandles.Length == 0)
            {
                return;
            }

            List<WzImageProperty> items = [];
            Log.Logger.Warning("检测到两个版本的一些项可能存在差异");
            foreach (var key in needHandles)
            {
                if (_processItems.Contains(key))
                    continue;

                var item = _suspectItems[key];
                var choices = new string[] { $"<原> {item.Original.GetFromPath("name")}", $"<新> {item.Newly.GetFromPath("name")}" };
                var selected = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title($"请选择该任务（Id={key}）应该使用哪个版本：")
                    .AddChoices(choices));

                var idx = choices.IndexOf(selected);
                var img = idx == 0 ? item.Original : item.Newly;
                Image.RemoveProperty(img);
                Image.AddProperty(img);
                _processItems.Add(key);
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

        public List<WzImageProperty> GetUnprocessedNewItems() => _newItems.Values.Where(x => !_processItems.Contains(x.Name)).ToList();

        public void ApplyQuestItemProperty(WzImageProperty o1, WzImageProperty o2, WzImageProperty s1, WzImageProperty? s2)
        {
            foreach (var prop in s1.WzProperties)
            {
                if (prop.PropertyType == WzPropertyType.SubProperty)
                {
                    ApplyQuestItemProperty(o1, o2, prop, s2?.GetFromPath(prop.Name));
                }
                else if (prop.PropertyType == WzPropertyType.String)
                {
                    SetImgPropertyValue(o1, o2, s1, s2, prop.Name);
                }
            }
        }

        public void SetImgPropertyValue(WzImageProperty oldRoot, WzImageProperty newRoot, WzImageProperty oldItem, WzImageProperty? newItem, string path)
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

                    var inputValue = AnsiConsole.Prompt(new TextPrompt<string>($"[red]{iTag.FullPath}, <原> {iTag.WzValue} 需要更换为：[/]")
                        .DefaultValue(iTag.GetString())
                        .ShowDefaultValue(true));
                    iTag.SetValue(inputValue);
                }

                else if (iTag.PropertyType == newTag.PropertyType)
                {
                    var isOldHasLetter = iTag.GetString().Any(x => !char.IsDigit(x));
                    var isNewHasLetter = newTag.GetString().Any(x => !char.IsDigit(x));

                    if (isOldHasLetter ^ isNewHasLetter)
                    {
                        var choices = new Dictionary<string, string>
                        {
                            { $"<原> {iTag.WzValue}", iTag.GetString() },
                            { $"<新> {newTag.WzValue}", newTag.GetString() },
                        };
                        var selected = AnsiConsole.Prompt(new SelectionPrompt<string>()
                                .Title($"{iTag.FullPath} 不匹配，需要手动选择：")
                                .AddChoices(choices.Keys));
                        iTag.SetValue(choices[selected]);
                    }
                    else
                    {
                        iTag.SetValue(newTag.WzValue);
                    }
                }

                else
                {
                    AddSuspectItem(oldRoot.Name, oldRoot, newRoot);
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
            _newItems.Clear();
        }
    }

    public class UpdateContext : IDisposable
    {
        public UpdateContext(WzFile source)
        {
            SourceFile = source;
        }

        /// <summary>
        /// 已更新的img
        /// </summary>


        public WzFile SourceFile { get; }
        Dictionary<string, UpdateImageContext> _updatedImgs = [];

        public bool IsCompleted { get; set; } = true;

        /// <summary>
        /// 判断是否可以结束更新
        /// </summary>
        /// <returns></returns>
        public bool CheckComplete()
        {
            var infoData = _updatedImgs.GetValueOrDefault("QuestInfo.img")?.GetUnprocessedNewItems()?.Select(x => x.Name) ?? [];
            var sayData = _updatedImgs.GetValueOrDefault("Say.img")?.GetUnprocessedNewItems()?.Select(x => x.Name) ?? [];
            var actData = _updatedImgs.GetValueOrDefault("Act.img")?.GetUnprocessedNewItems()?.Select(x => x.Name) ?? [];
            var checkData = _updatedImgs.GetValueOrDefault("Check.img")?.GetUnprocessedNewItems()?.Select(x => x.Name) ?? [];

            var sayMiss = infoData.Except(sayData);
            var infoMiss = sayData.Except(infoData);
            var allNew = infoData.Union(sayData);

            var actMiss = allNew.Except(actData);
            var checkMiss = allNew.Except(checkData);

            IsCompleted = true;
            if (infoMiss.Count() != 0)
            {
                Log.Logger.Warning("{Image} 缺少 {Items}", "QuestInfo.img", string.Join(',', infoMiss));
                IsCompleted = false;
            }

            if (sayMiss.Count() != 0)
            {
                Log.Logger.Warning("{Image} 缺少 {Items}", "Say.img", string.Join(',', sayMiss));
                IsCompleted = false;
            }

            if (actMiss.Count() != 0)
            {
                Log.Logger.Warning("{Image} 缺少 {Items}", "Act.img", string.Join(',', actMiss));
                IsCompleted = false;
            }

            if (checkMiss.Count() != 0)
            {
                Log.Logger.Warning("{Image} 缺少 {Items}", "Check.img", string.Join(',', checkMiss));
                IsCompleted = false;
            }
            return IsCompleted;
        }

        /// <summary>
        /// 处理怀疑项
        /// </summary>
        /// <returns></returns>
        public void HandleSuspectItems()
        {
            foreach (var kv in _updatedImgs)
            {
                kv.Value.HandleSuspectItems();
            }
        }
        /// <summary>
        /// 处理新增项
        /// </summary>
        public void HandleNewItems()
        {
            foreach (var kv in _updatedImgs)
            {
                kv.Value.HanldeNewItems();
            }
        }

        public void Handle()
        {
            var dataSourceType = new List<string> { "Quest.wz", "QuestInfo.img", "Say.img" };
            if (!IsCompleted)
            {
                dataSourceType.Add("Act.img");
                dataSourceType.Add("Check.img");
            }
            var selectedFileName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("请选择用什么文件更新：")
                    .AddChoices(dataSourceType));

            var usedFilePath = AnsiConsole.Prompt(new TextPrompt<string>($"用于更新的{selectedFileName}："));

            var usedMajorVersion = Enum.Parse<WzMapleVersion>(AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("请选择该文件的版本：")
                    .AddChoices(QuestProcessor.versions)));

            if (selectedFileName == "Quest.wz")
            {
                var usedGameVersion = AnsiConsole.Prompt(new TextPrompt<short>("wz文件版本：").DefaultValue((short)83).ShowDefaultValue(true));

                using var newWz = new WzFile(usedFilePath, usedGameVersion, usedMajorVersion);
                newWz.ParseWzFile();

                foreach (var newImg in newWz.WzDirectory.WzImages)
                {
                    var oldImg = SourceFile.WzDirectory.GetImageByName(newImg.Name);
                    if (oldImg == null)
                    {
                        // 原wz中没有，跳过
                        Log.Logger.Warning("{OriginalWZ}中没有{ImageName}", SourceFile.Name, newImg.Name);
                    }
                    else
                    {
                        if (!_updatedImgs.ContainsKey(newImg.Name))
                        {
                            _updatedImgs[newImg.Name] = new UpdateImageContext(oldImg.DeepClone());
                        }
                        ApplyQuestImage(_updatedImgs[newImg.Name], newImg.DeepClone());
                    }
                }
            }
            else
            {
                var selectedImg = SourceFile.WzDirectory.GetImageByName(selectedFileName);
                using var newImgStream = new FileStream(usedFilePath, FileMode.Open, FileAccess.Read);
                using var newImg = new WzImage(selectedFileName, newImgStream, SourceFile.MapleVersion);

                if (!_updatedImgs.ContainsKey(newImg.Name))
                {
                    _updatedImgs[newImg.Name] = new UpdateImageContext(selectedImg.DeepClone());
                }
                ApplyQuestImage(_updatedImgs[newImg.Name], newImg);
            }

            HandleSuspectItems();

            if (!CheckComplete())
            {
                if (AnsiConsole.Prompt(new ConfirmationPrompt($"检测到更新的img带来了新增项，你需要继续更新。否则将出现一些问题")))
                {
                    Handle();
                }
            }


            HandleNewItems();
        }

        public void ApplyQuestImage(UpdateImageContext context, WzImage newData)
        {
            var allData = context.Image.WzProperties.Select(x => x.Name).Union(newData.WzProperties.Select(x => x.Name)).OrderBy(x => x).ToHashSet();

            foreach (var item in allData)
            {
                var targetItem = context.Image[item];
                var newItem = newData[item].DeepClone();

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
                            context.AddSuspectItem(item, targetItem, newItem);
                            suspect = true;
                        }
                        foreach (var prop in targetItem.WzProperties)
                        {
                            if (targetItem.GetFromPath(prop.Name)?.PropertyType != prop.PropertyType)
                            {
                                context.AddSuspectItem(item, targetItem, newItem);
                                suspect = true;
                                break;
                            }
                        }

                        if (!suspect && !context.IsIgnorePropertyOverwrite())
                        {
                            context.ApplyQuestItemProperty(targetItem, newItem, targetItem, newItem);
                        }
                    }
                }
            }
        }


        public void Output()
        {
            var outputDir = AnsiConsole.Prompt(new TextPrompt<string>("输出目录（可选）").AllowEmpty());

            var outputTypes = new string[] { "1. 输出wz", "2. 输出更新过的img" };
            var option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("请选择操作：")
                    .AddChoices(outputTypes));

            WzMapleVersion outputMajorVersion = SourceFile.MapleVersion;
            if (option == outputTypes[0])
            {
                var outputWzPath = AnsiConsole.Prompt(new TextPrompt<string>("输出wz文件名").DefaultValue(SourceFile.WzDirectory.Name).ShowDefaultValue(true));
                outputMajorVersion = Enum.Parse<WzMapleVersion>(AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("请选择输出的版本：")
                        .AddChoices(QuestProcessor.versions)));
                var outputWZVersion = AnsiConsole.Prompt(new TextPrompt<short>("输出的wz版本：").DefaultValue(SourceFile.Version).ShowDefaultValue(true));


                var filePath = Path.Combine(outputDir, outputWzPath!);
                if (File.Exists(filePath))
                    File.Delete(filePath);

                using var outputWz = new WzFile(outputWZVersion, outputMajorVersion);
                foreach (var img in _updatedImgs)
                {
                    outputWz.WzDirectory.AddImage(img.Value.Image);
                }

                var otherImgs = SourceFile.WzDirectory.WzImages.Where(x => !_updatedImgs.ContainsKey(x.Name));
                foreach (var img in otherImgs)
                {
                    outputWz.WzDirectory.AddImage(img.DeepClone());
                }

                outputWz!.SaveToDisk(filePath);
            }
            else
            {
                outputMajorVersion = Enum.Parse<WzMapleVersion>(AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("请选择输出的版本：")
                        .AddChoices(QuestProcessor.versions)));

                foreach (var img in _updatedImgs)
                {
                    Utils.SaveImg(Path.Combine(outputDir, img.Key), img.Value.Image, outputMajorVersion);
                }
            }
        }

        public void Dispose()
        {
            SourceFile.Dispose();
            foreach (var img in _updatedImgs)
            {
                img.Value.Dispose();
            }
            _updatedImgs.Clear();
        }
    }
}
