namespace Quill.Delta
{
    public class InsertDataCustom : InsertData
    {
        public string CustomType { get; private set; }
        public object Value { get; private set; }

        public InsertDataCustom(string type, object value) :
            base(DataType.Custom)
        {
            CustomType = type;
            Value = value;
        }
    };
}
