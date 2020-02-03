using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Compiler
{
    class FileParser
    {
        public static string GetStringFromFile(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            string fileContents;
            using (StreamReader reader = new StreamReader(fileStream))
            {
                fileContents = reader.ReadToEnd();
                reader.Dispose();
                reader.Close();
            }

            fileStream.Dispose();
            fileStream.Close();

            return fileContents;
        }
    }
}
