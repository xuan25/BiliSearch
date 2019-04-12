using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Json
{
    public class JsonString : IJson
    {
        public string Value;
        public JsonString(string value)
        {
            Value = value;
        }

        public bool Contains(object index)
        {
            throw new NotImplementedException();
        }

        public double ToDouble()
        {
            throw new NotImplementedException();
        }

        public long ToLong()
        {
            throw new NotImplementedException();
        }

        public string ToString()
        {
            return Value;
        }

        public bool ToBool()
        {
            throw new System.NotImplementedException();
        }

        public IJson GetValue(object index)
        {
            throw new NotImplementedException();
        }

        public bool SetValue(object index, object value)
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class JsonLong : IJson
    {
        public long Value;
        public JsonLong(long value)
        {
            Value = value;
        }

        public bool Contains(object index)
        {
            throw new NotImplementedException();
        }

        public double ToDouble()
        {
            throw new NotImplementedException();
        }

        public long ToLong()
        {
            return Value;
        }

        public string ToString()
        {
            throw new NotImplementedException();
        }

        public bool ToBool()
        {
            throw new System.NotImplementedException();
        }

        public IJson GetValue(object index)
        {
            throw new NotImplementedException();
        }

        public bool SetValue(object index, object value)
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class JsonDouble : IJson
    {
        public double Value;
        public JsonDouble(double value)
        {
            Value = value;
        }

        public bool Contains(object index)
        {
            throw new NotImplementedException();
        }

        public double ToDouble()
        {
            return Value;
        }

        public long ToLong()
        {
            throw new NotImplementedException();
        }

        public string ToString()
        {
            throw new NotImplementedException();
        }

        public bool ToBool()
        {
            throw new System.NotImplementedException();
        }

        public IJson GetValue(object index)
        {
            throw new NotImplementedException();
        }

        public bool SetValue(object index, object value)
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class JsonBool : IJson
    {
        public bool Value;
        public JsonBool(bool value)
        {
            Value = value;
        }

        public bool Contains(object index)
        {
            throw new NotImplementedException();
        }

        public double ToDouble()
        {
            throw new NotImplementedException();
        }

        public long ToLong()
        {
            throw new NotImplementedException();
        }

        public string ToString()
        {
            throw new NotImplementedException();
        }

        public bool ToBool()
        {
            throw new System.NotImplementedException();
        }

        public IJson GetValue(object index)
        {
            throw new NotImplementedException();
        }

        public bool SetValue(object index, object value)
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
