using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TryGetExeLargeIcon
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //选择文件对话框
            var opfd = new System.Windows.Forms.OpenFileDialog { Filter = "资源文件|*.exe;*.dll" };
            if (opfd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            var file = opfd.FileName;

            //指定存放图标的文件夹
            const string folderToSave = "D:\\temp\\";
            if (!Directory.Exists(folderToSave)) Directory.CreateDirectory(folderToSave);

            //选中文件中的图标总数
            var iconTotalCount = PrivateExtractIcons(file, 0, 0, 0, null, null, 0, 0);

            //用于接收获取到的图标指针
            IntPtr[] hIconsDefault = new IntPtr[iconTotalCount];
            //对应的图标id
            int[] idsDefault = new int[iconTotalCount];
            //成功获取到的图标个数
            var successCount = PrivateExtractIcons(file, 0, 0, 0, hIconsDefault, idsDefault, iconTotalCount, 0);

            IntPtr[] hIconsLarge = new IntPtr[iconTotalCount];
            int[] idsLarge = new int[iconTotalCount];
            PrivateExtractIcons(file, 0, 256, 256, hIconsLarge, idsLarge, iconTotalCount, 0);

            //遍历并保存图标
            for (var i = 0; i < successCount; i++)
            {
                //指针为空，跳过
                if (hIconsDefault[i] == IntPtr.Zero) continue;

                IntPtr hicon, defaultSizeIcon = hIconsDefault[i], largeIcon = hIconsLarge[i];
                //如果id相同，说明是读取同一个图标文件，取默认大小的
                if (idsDefault[i] == idsLarge[i])
                {
                    hicon = defaultSizeIcon;
                    DestroyIcon(largeIcon);
                }
                //如果id不同，说明存在大图，取大图的
                else
                {
                    hicon = largeIcon;
                    DestroyIcon(defaultSizeIcon);
                }

                using (var ico = Icon.FromHandle(hicon))
                {
                    using (var myIcon = ico.ToBitmap())
                    {
                        myIcon.Save(folderToSave + hicon.ToString("000") + ".png", ImageFormat.Png);
                    }
                }
                //内存回收
                DestroyIcon(hicon);
            }
        }


        //details: https://msdn.microsoft.com/en-us/library/windows/desktop/ms648075(v=vs.85).aspx
        //Creates an array of handles to icons that are extracted from a specified file.
        //This function extracts from executable (.exe), DLL (.dll), icon (.ico), cursor (.cur), animated cursor (.ani), and bitmap (.bmp) files. 
        //Extractions from Windows 3.x 16-bit executables (.exe or .dll) are also supported.
        [DllImport("User32.dll")]
        public static extern int PrivateExtractIcons(
            string lpszFile, //file name
            int nIconIndex,  //The zero-based index of the first icon to extract.
            int cxIcon,      //The horizontal icon size wanted.
            int cyIcon,      //The vertical icon size wanted.
            IntPtr[] phicon, //(out) A pointer to the returned array of icon handles.
            int[] piconid,   //(out) A pointer to a returned resource identifier.
            int nIcons,      //The number of icons to extract from the file. Only valid when *.exe and *.dll
            int flags        //Specifies flags that control this function.
        );

        //details:https://msdn.microsoft.com/en-us/library/windows/desktop/ms648063(v=vs.85).aspx
        //Destroys an icon and frees any memory the icon occupied.
        [DllImport("User32.dll")]
        public static extern bool DestroyIcon(
            IntPtr hIcon //A handle to the icon to be destroyed. The icon must not be in use.
        );
    }
}
