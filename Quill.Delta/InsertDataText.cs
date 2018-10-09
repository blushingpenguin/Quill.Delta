namespace Quill.Delta
{
    public class InsertDataText : InsertDataString
    {
        public InsertDataText(string value) :
            base(DataType.Text, value)
        {
        }
    }
}
