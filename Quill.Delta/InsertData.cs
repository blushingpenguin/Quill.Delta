namespace Quill.Delta
{
    public abstract class InsertData
    {
        public DataType Type { get; private set; }

        protected InsertData(DataType type)
        {
            Type = type;
        }
    }
}
