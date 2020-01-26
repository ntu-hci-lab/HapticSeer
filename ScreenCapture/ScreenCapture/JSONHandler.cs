using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFCaptureSample
{
    public class JSONHandler
    {
        StringBuilder stringBuilder = new StringBuilder();
        int ItemCount = 0;
        public JSONHandler()
        {
            stringBuilder.Append("[");
        }
        public void AddNew(string New)
        {
            if (ItemCount++ != 0)
                stringBuilder.Append(",");
            stringBuilder.AppendLine();
            stringBuilder.Append(New);
        }
        public string Output()
        {
            string temp = stringBuilder.Append("]").ToString();
            stringBuilder = new StringBuilder();
            stringBuilder.AppendLine().Append("[");
            return temp;
        }
    }
}
