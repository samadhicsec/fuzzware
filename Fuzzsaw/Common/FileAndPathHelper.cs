using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Fuzzware.Fuzzsaw.Common
{
    public static class FileAndPathHelper
    {
        /// <summary>
        /// Gets a user selected directory
        /// </summary>
        /// <param name="Desc">The title for the directory select dialog</param>
        /// <param name="SelectedPath">The directory path pre-selected in the dialog</param>
        /// <param name="GetRelativePath">Return the path relative to the SelectedPath</param>
        /// <returns></returns>
        public static String GetFolderUsingDialog(String Desc, String SelectedPath, bool GetRelativePath)
        {
            // Create a folder broswer
            using (FolderBrowserDialog fd = new FolderBrowserDialog())
            {
                fd.Reset();
                fd.Description = Desc;
                fd.ShowNewFolderButton = true;
                fd.RootFolder = Environment.SpecialFolder.Desktop;
                if (!String.IsNullOrEmpty(SelectedPath))
                {
                    // If the selected path does not end in Path.DirectorySeparatorChar then Path.GetDirectoryName
                    // will return its parent directory
                    if (!SelectedPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                        SelectedPath += Path.DirectorySeparatorChar.ToString();
                    fd.SelectedPath = Path.GetDirectoryName(Path.GetFullPath(SelectedPath));
                }
                else
                    fd.SelectedPath = Path.GetDirectoryName(Environment.CurrentDirectory);
                DialogResult oDR = fd.ShowDialog();

                // Get the selected folder, if there was one
                if ((oDR == DialogResult.OK) && (!String.IsNullOrEmpty(fd.SelectedPath)))
                {
                    String Folder = fd.SelectedPath;
                    if (GetRelativePath)
                    {
                        if (Folder.StartsWith(Path.GetFullPath(SelectedPath)))
                        {
                            Folder = Folder.Substring(Path.GetFullPath(SelectedPath).Length);
                            if (Folder.StartsWith("" + Path.DirectorySeparatorChar))
                                Folder = Folder.Substring(1);
                        }
                    }
                    return Folder;
                }
            }
            return "";
        }

        /// <summary>
        /// Allows us to move a source file to a destination file, even if the destination file exists, in which case it is overwritten.
        /// </summary>
        /// <param name="sourceFileName">The source file</param>
        /// <param name="destFileName">The destination file</param>
        public static void Move(string sourceFileName, string destFileName)
        {
            // Move the source file to the same directory as the destination file, so they are on the same volume (otherwise 
            // File.Replace won't work)
            String movedfile = Path.Combine(Path.GetDirectoryName(destFileName), Path.GetRandomFileName());
            File.Move(sourceFileName, movedfile);
            if (File.Exists(destFileName))
                File.Replace(movedfile, destFileName, null);
            else
                File.Move(movedfile, destFileName);
        }

    }
}
