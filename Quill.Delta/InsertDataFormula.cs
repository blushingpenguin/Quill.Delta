namespace Quill.Delta
{
    public class InsertDataFormula : InsertDataString
    {
        public InsertDataFormula(string value) :
            base(DataType.Formula, value)
        {
        }
    }
}
