using System;
using System.Collections.Generic;
using System.IO;

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
