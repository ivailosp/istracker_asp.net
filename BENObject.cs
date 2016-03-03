using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace istracker_asp.net
{
    enum BENObjectType
    {
        Integer,
        String,
        List,
        Dictionary,
    };
    class BENObject : Object
    {
        private Object data;
        private BENObjectType type;
        public BENObject(byte[] str)
        {
            data = str;
            type = BENObjectType.String;
        }
        public BENObject(string str)
        {
            data = Encoding.Default.GetBytes(str);
            type = BENObjectType.String;
        }
        public BENObject(long num)
        {
            data = num;
            type = BENObjectType.Integer;
        }
        public BENObject(Dictionary<BENObject, BENObject> dict)
        {
            data = dict;
            type = BENObjectType.Dictionary;
        }
        public BENObject(List<BENObject> list)
        {
            data = list;
            type = BENObjectType.List;
        }
        private void CreateFromString(ref byte[] str, ref int index)
        {
            if (str != null && str.Length - index > 2)
            {
                switch (Convert.ToChar(str[index]))
                {
                    case 'i':
                        data = handleInteger(ref str, ref index);
                        type = BENObjectType.Integer;
                        break;
                    case 'l':
                        data = handleList(ref str, ref index);
                        type = BENObjectType.List;
                        break;
                    case 'd':
                        data = handleDictionary(ref str, ref index);
                        type = BENObjectType.Dictionary;
                        break;
                    default:
                        data = handleString(ref str, ref index);
                        type = BENObjectType.String;
                        break;
                }
            }
            else {
                data = null;
            }
        }
        public BENObject(ref byte[] str, ref int index)
        {
            CreateFromString(ref str, ref index);
        }
        public BENObject(ref byte[] str)
        {
            int index = 0;
            CreateFromString(ref str, ref index);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            switch (type)
            {
                case BENObjectType.Integer:
                    sb.AppendFormat("i{0}e", getInt());
                    return sb.ToString();
                case BENObjectType.List:
                    {
                        sb.Append('l');
                        foreach (BENObject obj in (List<BENObject>)data)
                        {
                            sb.Append(obj.ToString());
                        }
                        sb.Append('e');
                        return sb.ToString();
                    }
                case BENObjectType.Dictionary:
                    {
                        sb.Append('d');
                        foreach (var item in (Dictionary<BENObject, BENObject>)data)
                        {
                            sb.Append(item.Key.ToString());
                            sb.Append(item.Value.ToString());
                        }
                        sb.Append('e');
                        return sb.ToString();
                    }
                default:
                    {
                        string ret = getString();
                        sb.AppendFormat("{0}:{1}", ret.Length, ret);
                        return sb.ToString();
                    }
            }
        }
        public byte[] ToBytes()
        {
            return Encoding.Default.GetBytes(ToString());
        }
        private long handleInteger(ref byte[] str, ref int index)
        {
            long ret = 0;
            byte delimiter = Convert.ToByte('e');
            index++;
            while (str[index] != delimiter)
            {
                ret *= 10;
                ret += str[index] - Convert.ToByte('0');
                index++;
            }
            index++;
            return ret;
        }
        private byte[] handleString(ref byte[] str, ref int index)
        {
            int length = 0;
            byte[] ret;
            byte delimiter = Convert.ToByte(':');
            while (str[index] != delimiter)
            {
                length *= 10;
                length += str[index] - Convert.ToByte('0');
                index++;
            }
            index++;
            ret = new byte[length];
            Buffer.BlockCopy(str, index, ret, 0, length);
            index += length;
            return ret;
        }
        private List<BENObject> handleList(ref byte[] str, ref int index)
        {
            List<BENObject> ret = new List<BENObject>();
            index++;
            byte delimiter = Convert.ToByte('e');
            while (str[index] != delimiter)
            {
                ret.Add(new BENObject(ref str, ref index));
            }
            index++;
            return ret;
        }
        private Dictionary<BENObject, BENObject> handleDictionary(ref byte[] str, ref int index)
        {
            Dictionary<BENObject, BENObject> ret = new Dictionary<BENObject, BENObject>();
            index++;
            byte delimiter = Convert.ToByte('e');
            while (str[index] != delimiter)
            {
                ret.Add(new BENObject(ref str, ref index), new BENObject(ref str, ref index));
            }
            index++;
            return ret;
        }
        public Object getData()
        {
            return data;
        }
        public long getInt()
        {
            return (long)data;
        }
        public BENObjectType getType()
        {
            return type;
        }
        public void setData(Object data)
        {
            this.data = data;
        }
        public void setType(BENObjectType type)
        {
            this.type = type;
        }
        public Dictionary<BENObject, BENObject> getDictonary()
        {
            return (Dictionary<BENObject, BENObject>)data;
        }
        public List<BENObject> getList()
        {
            return (List<BENObject>)data;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                if (type != BENObjectType.String)
                {
                    int hash = 17;
                    hash = hash * 23 + data.GetHashCode();
                    hash = hash * 23 + type.GetHashCode();
                    return hash;
                }
                else
                {
                    const int p = 16777619;
                    int hash = (int)2166136261;
                    byte[] ar = (byte[])data;

                    for (int i = 0; i < ar.Length; i++)
                        hash = (hash ^ ar[i]) * p;

                    hash += hash << 13;
                    hash ^= hash >> 7;
                    hash += hash << 3;
                    hash ^= hash >> 17;
                    hash += hash << 5;
                    return hash;
                }
            }
        }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            BENObject item = obj as BENObject;
            if (item.type != this.type)
            {
                return false;
            }
            if (type == BENObjectType.String)
            {
                byte[] b1 = (byte[])item.data;
                byte[] b2 = (byte[])data;
                return b1.SequenceEqual(b2);
            }
            return item.data == this.data;
        }
        public string getString()
        {
            return Encoding.Default.GetString((byte[])data);
        }
    }
}