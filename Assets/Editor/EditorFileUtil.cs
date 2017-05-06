using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[Flags]
public enum FileWalkOption
{
    File = 1,
    Directory = 2,
}

public class EditorFileUtil
{
    public static void FileWalker(string path, Func<string, bool, bool> check, Action<string, bool> action, FileWalkOption option, bool recursive = false)
    {
        if (File.Exists(path) && ( ( option & FileWalkOption.File ) != 0 ))
        {
            if (check(path, true))
            {
                action(path, true);
            }
            return;
        }

        if (!Directory.Exists(path))
            return;

        if (( option & FileWalkOption.File ) != 0)
        {
            var files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                if (check(files[i], true))
                {
                    action(files[i], true);
                }
            }
        }

        if (( option & FileWalkOption.Directory ) != 0)
        {
            var dirs = Directory.GetDirectories(path);
            for (int i = 0; i < dirs.Length; i++)
            {
                if (check(dirs[i], false))
                {
                    action(dirs[i], false);
                }
            }
        }

        if (recursive)
        {
            var dirs = Directory.GetDirectories(path);
            for (int i = 0; i < dirs.Length; i++)
            {
                FileWalker(dirs[i], check, action, option, recursive);
            }
        }
    }
}
