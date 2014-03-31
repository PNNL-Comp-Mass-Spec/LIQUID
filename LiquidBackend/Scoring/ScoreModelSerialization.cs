using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LiquidBackend.Scoring
{
	public class ScoreModelSerialization
	{
		public static void Serialize(ScoreModel scoreModel, string fileLocation)
		{
			if (File.Exists(fileLocation)) File.Delete(fileLocation);

			DataContractSerializer serializer = new DataContractSerializer(typeof(ScoreModel));

			var xmlWriterSettings = new XmlWriterSettings { Indent = true };

			using (XmlWriter xmlWriter = XmlWriter.Create(fileLocation, xmlWriterSettings))
			{
				serializer.WriteObject(xmlWriter, scoreModel);
			}
		}

		public static ScoreModel Deserialize(string fileLocation)
		{
			DataContractSerializer serializer = new DataContractSerializer(typeof(ScoreModel));
			FileStream fs = new FileStream(fileLocation, FileMode.Open);
			XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());

			ScoreModel scoreModel = serializer.ReadObject(reader) as ScoreModel;
			return scoreModel;
		}
	}
}
