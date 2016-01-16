using System.Windows.Forms;

namespace HighTrainSpatialInfluence.Services.Common
{
    /// <summary>
    /// 对话框帮助类
    /// </summary>
    internal class DialogHelper
    {
        /// <summary>
        /// 文件对话框的过滤器
        /// </summary>
        private readonly string _filter;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileType">文件类型</param>
        public DialogHelper(string fileType)
        {
            _filter = fileType.ToUpper() + "|*." + fileType;
        }
        public DialogHelper() { }

        /// <summary>
        /// 选择打开文件
        /// </summary>
        /// <returns>打开文件的路径</returns>
        public string OpenFile()
        {
            return OpenFile(string.Empty);
        }

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="title">对话框的标题</param>
        /// <returns>文件的路径</returns>
        public string OpenFile(string title)
        {
            var dialog = new OpenFileDialog { Filter = _filter, Title = title, RestoreDirectory = true };
            return dialog.ShowDialog() == DialogResult.OK ?
                dialog.FileName : string.Empty;
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <returns>保存文件的路径</returns>
        public string SaveFile()
        {
            return SaveFile(string.Empty);
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="title">对话框的标题</param>
        /// <returns>保存的路径</returns>
        public string SaveFile(string title)
        {
            var dialog = new SaveFileDialog { Filter = _filter, RestoreDirectory = true };
            return dialog.ShowDialog() == DialogResult.OK ?
                dialog.FileName : string.Empty;
        }

        /// <summary>
        /// 打开文件夹对话框
        /// </summary>
        /// <returns>文件路径</returns>
        public string OpenFolderDialog()
        {
            return OpenFolderDialog(true);
        }
        /// <summary>
        /// 打开文件夹对话框
        /// </summary>
        /// <param name="isNewFolderButton">选择是否有新建问文件夹的button</param>
        /// <returns>返回文件夹的路径</returns>
        public string OpenFolderDialog(bool isNewFolderButton)
        {
            var folderDialog = new FolderBrowserDialog { ShowNewFolderButton = isNewFolderButton };
            return folderDialog.ShowDialog() == DialogResult.OK ?
                folderDialog.SelectedPath : string.Empty;
        }
    }
}
