using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SpatialAccess.Services.Common
{
    internal class FileHelper
    {
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        /// <param name="extensions"></param>
        public static void DeleteFile(string folderPath, string fileName, params string[] extensions)
        {
            foreach (var extension in extensions)
            {
                string filePath = folderPath + @"\" + fileName + extension;
                if (System.IO.File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}
