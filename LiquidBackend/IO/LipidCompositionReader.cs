using LiquidBackend.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidBackend.IO
{
	public class LipidCompositionReader<T> : FileReader<T> where T : LipidCompositionRule, new()
	{
		protected override Dictionary<string, int> CreateColumnMapping(string columnString)
		{
			throw new NotImplementedException();
		}

		protected override T ParseLine(string line, IDictionary<string, int> columnMapping)
		{
			throw new NotImplementedException();
		}
	}
}
