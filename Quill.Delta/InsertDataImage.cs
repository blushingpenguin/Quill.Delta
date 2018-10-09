namespace Quill.Delta
{
    public class InsertDataImage : InsertDataString
    {
        public InsertDataImage(string value) :
            base(DataType.Image, value)
        {
        }
    }
}
