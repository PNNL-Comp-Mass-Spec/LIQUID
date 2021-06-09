using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace LiquidBackend.Scoring
{
    public static class ScoreModelSerialization
    {
        public static void Serialize(ScoreModel scoreModel, string fileLocation)
        {
            if (File.Exists(fileLocation)) File.Delete(fileLocation);

            var serializer = new DataContractSerializer(typeof(ScoreModel));

            var xmlWriterSettings = new XmlWriterSettings { Indent = true };

            using var xmlWriter = XmlWriter.Create(fileLocation, xmlWriterSettings);

            serializer.WriteObject(xmlWriter, scoreModel);
        }

        public static ScoreModel Deserialize(string fileLocation)
        {
            var serializer = new DataContractSerializer(typeof(ScoreModel));
            var fs = new FileStream(fileLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());

            return serializer.ReadObject(reader) as ScoreModel;
        }
    }
}
