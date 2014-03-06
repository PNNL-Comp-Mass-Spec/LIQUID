using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidBackend.Domain
{
	public enum ActivationMethodCombination
	{
		HcdOnly,
		CidOnly,
		HcdThenCid,
		CidThenHcd,
		Unsupported
	}
}
