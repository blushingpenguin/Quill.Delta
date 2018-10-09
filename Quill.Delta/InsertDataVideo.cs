namespace Quill.Delta
{
    public class InsertDataVideo : InsertDataString
    {
        public InsertDataVideo(string value) :
            base(DataType.Video, value)
        {
        }
    }
}
