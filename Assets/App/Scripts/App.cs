using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_STANDALONE_WIN
using NRatel.Win32;
#endif

namespace NRatel.TextureUnpacker
{
    public class App
    {
        internal enum UnpackMode
        {
            Split = 0,
            Restore = 1,
            All = 2,
            FindAllPlistAndSplit = 3,
            DeepFindAllPlistAndSplit = 4,
        }

        private UnpackMode currentUnpackMode;
        private Main main;
        private AppUI appUI;
        private bool isStop = false;
        private bool isExecuting = false;
        private bool isMultipleSplitMode = false;
        private Loader loader;
        private Plist plist;
        private Texture2D bigTexture;
        private long totalTime;

        public App(Main main)
        {
            this.main = main;
            appUI = main.GetComponent<AppUI>().Init();
            currentUnpackMode = (UnpackMode)appUI.m_Dropdown_SelectMode.value;

            //编辑器下测试用
#if UNITY_EDITOR
            // plistFilePath = @"C:\Users\Administrator\Desktop\plist&png\test.plist";
            // pngFilePath = @"C:\Users\Administrator\Desktop\plist&png\test.png";
            // main.StartCoroutine(LoadFiles());
#endif

            RegisterEvents();
        }

        public static string GetSaveDir()
        {
            return m_SavePath;
        }

        private void RegisterEvents()
        {
#if UNITY_STANDALONE_WIN
            //TODO:完成多平台的拖入文件功能
            // main.GetComponent<FilesOrFolderDragInto>().AddEventListener((List<string> aPathNames) =>
            // {
            //     if (isExecuting)
            //     {
            //         appUI.SetTip("正在执行\n请等待结束");
            //         return;
            //     }

            //     if (aPathNames.Count > 1)
            //     {
            //         appUI.SetTipWarn("只可拖入一个文件");
            //         return;
            //     }
            //     else
            //     {
            //         string path = aPathNames[0];
            //         if (path.EndsWith(".plist"))
            //         {
            //             plistFilePath = path;
            //             pngFilePath = Path.GetDirectoryName(path) + @"\" + Path.GetFileNameWithoutExtension(path) + ".png";
            //             if (!File.Exists(pngFilePath))
            //             {
            //                 appUI.SetTipErr("不存在与当前plist文件同名的png文件");
            //                 return;
            //             }
            //         }
            //         else if (path.EndsWith(".png"))
            //         {
            //             pngFilePath = path;
            //             plistFilePath = Path.GetDirectoryName(path) + @"\" + Path.GetFileNameWithoutExtension(path) + ".plist";
            //             if (!File.Exists(plistFilePath))
            //             {
            //                 appUI.SetTipErr("不存在与当前png文件同名的plist文件");
            //                 return;
            //             }
            //         }
            //         else
            //         {
            //             appUI.SetTipWarn("请放入 plist或png 文件");
            //             return;
            //         }

            //         main.StartCoroutine(LoadFiles());
            //     }
            // });
#endif

            appUI.m_Btn_Excute.onClick.AddListener(() =>
            {
                if (isExecuting)
                {
                    appUI.SetTipInfo("正在执行\n请等待结束");
                    return;
                }

                isExecuting = true;
                var path = appUI.m_InputField.text;
                if (!string.IsNullOrEmpty(path))
                {
                    if (currentUnpackMode == UnpackMode.FindAllPlistAndSplit)
                        FindAllPlistAndSplit(path);
                    else if (currentUnpackMode == UnpackMode.DeepFindAllPlistAndSplit)
                        FindAllPlistAndSplit(path, true);
                    else
                    {
                        if (path.EndsWith(".plist"))
                        {
                            var pngFilePath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".png");
                            if (!File.Exists(pngFilePath))
                            {
                                appUI.SetTipErr($"不存在与当前plist件同名png文的文件.\n{pngFilePath}");
                                isExecuting = false;
                            }
                            else
                                main.StartCoroutine(Unpack(pngFilePath, path));
                        }
                        else if (path.EndsWith(".png"))
                        {
                            var plistFilePath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".plist");
                            if (!File.Exists(path))
                            {
                                appUI.SetTipErr($"不存在与当前png文件同名的plist文件.\n{path}");
                                isExecuting = false;
                            }
                            else
                                main.StartCoroutine(Unpack(path, plistFilePath));
                        }
                        else
                        {
                            appUI.SetTipErr($"File {path} is not exist!");
                            isExecuting = false;
                        }
                    }
                }
            });

            appUI.m_Btn_Stop.onClick.AddListener(() =>
            {
                if (isExecuting)
                    isStop = true;
            });

            appUI.m_Dropdown_SelectMode.onValueChanged.AddListener((value) =>
            {
                currentUnpackMode = (UnpackMode)value;
            });
        }

        private void LoadFiles(string pngFilePath, string plistFilePath)
        {
            try
            {
                loader = Loader.LookingForLoader(plistFilePath);
                if (loader != null)
                {
                    plist = loader.LoadPlist(plistFilePath);
                    bigTexture = loader.LoadTexture(pngFilePath, plist.metadata);
                    appUI.SetImage(bigTexture);
                    appUI.SetTip($"名称: {plist.metadata.textureFileName} | 类型: format_{plist.metadata.format} | 大小: {plist.metadata.size.width} * {plist.metadata.size.height}");
                    m_SavePath = Path.Combine(isMultipleSplitMode ? m_RootPath : Path.GetDirectoryName(plist.path), "__output__", Path.GetFileNameWithoutExtension(plist.metadata.textureFileName));
                }
                else
                {
                    appUI.SetTipErr("无法识别的plist类型!!!\n请联系作者");
                }
            }
            catch
            {
                appUI.SetTipErr("出错了!!!\n请联系作者\n↓");
            }
        }

        private IEnumerator Unpack(string pngFilePath, string plistFilePath)
        {
            LoadFiles(pngFilePath, plistFilePath);
            if (loader == null || plist == null)
            {
                appUI.SetTipErr("没有指定可执行的plist&png");
                isExecuting = false;
                yield break;
            }

            int total = plist.frames.Count;
            int count = 0;
            var watch = new System.Diagnostics.Stopwatch();
            appUI.isSplitTip = false;//first don't split process tip text
            watch.Start();

            switch (currentUnpackMode)
            {
                case UnpackMode.Restore:
                    foreach (var frame in plist.frames)
                    {
                        Core.Restore(bigTexture, frame);
                        appUI.SetTipProcess($"进度：{++count} / {total} {frame.textureName} {(count < total ? null : IsDone())}");
                        yield return null;
                    }
                    break;
                case UnpackMode.All:
                    foreach (var frame in plist.frames)
                    {
                        Core.Split(bigTexture, frame);
                        Core.Restore(bigTexture, frame);
                        appUI.SetTipProcess($"进度：{++count} / {total} {frame.textureName} {(count < total ? null : IsDone())}");
                        yield return null;
                    }
                    break;
                case UnpackMode.Split:
                case UnpackMode.FindAllPlistAndSplit:
                case UnpackMode.DeepFindAllPlistAndSplit:
                default:
                    foreach (var frame in plist.frames)
                    {
                        Core.Split(bigTexture, frame);
                        appUI.SetTipProcess($"进度：{++count} / {total} {frame.textureName} {(count < total ? null : IsDone())}");
                        yield return null;
                    }
                    break;
            }

            if (isMultipleSplitMode)
            {
                if (m_queue == null || m_queue.Count == 0 || isStop)
                {
                    m_queue = null;
                    isExecuting = false;
                    isMultipleSplitMode = false;
                    if (isStop)
                    {
                        isStop = false;
                        appUI.SetTipWarn("Process has been force stop! (it's not imediatily)");
                    }
                    appUI.SetTipInfo($"Split is done!    Time: {(totalTime / 1000f):0.00}s");
                    yield break;
                }
                var (_pngName, _plistName) = m_queue.Dequeue();
                main.StartCoroutine(Unpack(_pngName, _plistName));
            }
            else
                isExecuting = false;

            string IsDone()
            {
                watch.Stop();
                totalTime += watch.ElapsedMilliseconds;
                return $"\nRunning Time: {watch.ElapsedMilliseconds} ms.\n";
            }
        }

        private void FindAllPlistAndSplit(string path, bool isDeepFind = false)
        {
            if (!Directory.Exists(path))
            {
                appUI.SetTipErr($"Path {path} is not exist!");
                return;
            }

            isMultipleSplitMode = true;
            m_RootPath = path;
            m_queue = new Queue<(string pngName, string plistName)>();
            IEnumerable<string> plistFiles;
            if (isDeepFind)
                plistFiles = Directory.GetFiles(path, "*.plist", SearchOption.AllDirectories);
            else
                plistFiles = Directory.GetFiles(path).Where(name => name.EndsWith(".plist"));
            string pngFilePath;
            appUI.SetTipInfo($"Find {plistFiles.Count()} files from <b>{path}</b>:\n{string.Join("\n", plistFiles)}");

            //match all png file from plist files.
            foreach (var plistFileName in plistFiles)
            {
                pngFilePath = Path.Combine(Path.GetDirectoryName(plistFileName), Path.GetFileNameWithoutExtension(plistFileName) + ".png");
                if (!File.Exists(pngFilePath))
                {
                    var pvrcczFilePath = Path.Combine(Path.GetDirectoryName(plistFileName), Path.GetFileNameWithoutExtension(plistFileName) + ".pvr.ccz");
                    if (File.Exists(pvrcczFilePath))
                    {
                        appUI.SetTipInfo($"存在与当前plist件同名pvr.ccz文的文件.\n{pvrcczFilePath}");
                        //use `TexturePacker` command convert pvr.ccz to png file.
                        var args = $"{pvrcczFilePath} --sheet {pngFilePath}";
                        System.Diagnostics.Process.Start("TexturePacker", args);
                        // if (process.Start())//execute command
                        appUI.SetTipInfo($"Convert {pvrcczFilePath} to {pngFilePath} successful!");
                        // else
                        //     appUI.SetTipErr($"Command {"TexturePacker " + args} execute error!");
                    }
                    else
                        appUI.SetTipErr($"不存在与当前plist件同名png文的文件.\n{pngFilePath}");
                }
                // else
                //     appUI.SetTip($"存在与当前plist件同名png文的文件.\n{pngFilePath}");
                m_queue.Enqueue((pngFilePath, plistFileName));
            }

            isExecuting = true;
            var (_png, _plist) = m_queue.Dequeue();
            main.StartCoroutine(Unpack(_png, _plist));
        }

        private Queue<(string pngName, string plistName)> m_queue;
        private static string m_SavePath = string.Empty;
        private static string m_RootPath = string.Empty;
    }
}
