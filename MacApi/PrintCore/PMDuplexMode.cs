using System;

namespace MacApi.PrintCore
{
	public enum PMDuplexMode : uint
	{
		None = 1u,
		NoTumble,
		Tumble,
		SimplexTumble
	}
}
