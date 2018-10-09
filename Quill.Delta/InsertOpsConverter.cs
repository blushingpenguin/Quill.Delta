using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quill.Delta
{
    /// <summary>
    /// Converts raw delta insert ops to array of denormalized DeltaInsertOp objects
    /// </summary>
    public class InsertOpsConverter
    {
        public static IList<DeltaInsertOp> Convert(JArray deltaOps)
        {
            if (deltaOps == null)
            {
                return new List<DeltaInsertOp>();
            }
            var results = new List<DeltaInsertOp>();
            foreach (var op in deltaOps.SelectMany(x => InsertOpDenormalizer.Denormalize(x)))
            {
                var insertVal = InsertOpsConverter.ConvertInsertVal(op["insert"]);
                if (insertVal == null)
                {
                    continue;
                }
                var attributes = OpAttributeSanitizer.Sanitize(op["attributes"]);
                results.Add(new DeltaInsertOp(insertVal, attributes));
            }
            return results;
        }

        internal static InsertData ConvertInsertVal(JToken insert)
        {
            if (insert == null)
            {
                return null;
            }
            if (insert.Type == JTokenType.String)
            {
                string val = insert.Value<string>();
                return String.IsNullOrEmpty(val) ? null :
                    new InsertDataText(val);
            }
            if (insert.Type != JTokenType.Object)
            {
                return null;
            }

            var insertObj = (JObject)insert;
            if (insertObj.Count == 0)
            {
                return null;
            }

            var data = insertObj["image"];
            if (data != null)
            {
                return new InsertDataImage(data.Value<string>());
            }
            data = insertObj["video"];
            if (data != null)
            {
                return new InsertDataVideo(data.Value<string>());
            }
            data = insertObj["formula"];
            if (data != null)
            {
                return new InsertDataFormula(data.Value<string>());
            }
            var prop = insertObj.Properties().First();
            return new InsertDataCustom(prop.Name, prop.Value);
        }
    }
}
