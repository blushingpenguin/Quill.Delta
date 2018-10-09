namespace Quill.Delta
{
    public class InsertDataString : InsertData
    {
        public string Value { get; private set; }

        protected InsertDataString(DataType type, string value) :
           base(type)
        {
            Value = value;
        }
    }
}
