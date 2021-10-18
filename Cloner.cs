using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
//using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Utilities
{
    public class Cloner
    {
        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                XmlSerializer x = new XmlSerializer(obj.GetType());
                x.Serialize(ms, obj);
                ms.Position = 0;
                return (T)x.Deserialize(ms);
            }
        }
    }
}
