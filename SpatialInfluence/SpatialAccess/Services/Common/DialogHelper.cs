using System.Windows.Forms;

namespace SpatialAccess.Services.Common
{
    /// <summary>
    /// 对话框帮助类
    /// </summary>
    internal class DialogHelper
    {
        /// <summary>
        /// 根据扩展名获取的文件过滤器
        /// </summary>
        /// <param name="extension">扩展名</param>
        /// <returns>Dialog的过滤器</returns>
        private static string Filter(string extension)
        {
            return extension.ToUpper() + "|*." + extension;
        }
        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="extension">扩展名</param>
        /// <param name="title">对话框</param>
        /// <returns>打开文件</returns>
        public static string OpenFile(string extension, string title = "")
        {
            var dialog = new OpenFileDialog { 
                Filter = Filter(extension), 
                Title = title, RestoreDirectory = true 
            };
            return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : string.Empty;
        }

        /// <summary>
        /// 打开文件夹对话框
        /// </summary>
        /// <param name="isNewFolderButton">选择是否有新建问文件夹的button</param>
        /// <returns>返回文件夹的路径</returns>
        public static string OpenFolderDialog(bool isNewFolderButton=true)
        {
            var folderDialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = isNewFolderButton
            };
            return folderDialog.ShowDialog() == DialogResult.OK ?
                folderDialog.SelectedPath : string.Empty;
        }
    }
}
