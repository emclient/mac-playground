﻿using System;

namespace MacBridge.MacApi.PrintCore
{
	public enum PMDuplexMode : uint
	{
		None = 1u,
		NoTumble,
		Tumble,
		SimplexTumble
	}
}
