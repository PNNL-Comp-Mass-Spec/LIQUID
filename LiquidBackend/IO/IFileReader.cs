using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidBackend.IO
{
	/// <summary>
	/// File reader interface.
	/// </summary>
	/// <typeparam name="T">Generic type</typeparam>
	public interface IFileReader<T>
	{
		List<T> ReadFile(FileInfo fileInfo, IProgress<int> progress);
	}
}
