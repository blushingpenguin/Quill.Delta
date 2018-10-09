using Newtonsoft.Json.Linq;

namespace Quill.Delta
{
    public class InsertDataJToken : InsertData
    {
        public JToken Value { get; private set; }

        public InsertDataJToken(DataType type, JToken value) :
            base(type)
        {
            Value = value;
        }
    }
}
