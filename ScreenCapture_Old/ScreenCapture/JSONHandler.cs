using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFCaptureSample
{
    public class JSONHandler
    {
        StringBuilder stringBuilder = new StringBuilder();
        int ItemCount = 0;
        string path;
        public JSONHandler(string path)
        {
            this.path = path;
            stringBuilder.Append("[");
        }
        public void AddNew(string New)
        {
            if (stringBuilder == null)
                throw new Exception("JSON Handler Error!");
            if (ItemCount++ != 0)
                stringBuilder.Append(",");
            stringBuilder.AppendLine();
            stringBuilder.Append(New);
        }
        public void ToFile()
        {
            string temp = stringBuilder.AppendLine().Append("]").ToString();
            stringBuilder = null;
            File.WriteAllText(path + "Controller.json", temp);
        }
    }
}
