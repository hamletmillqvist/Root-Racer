using System;

namespace RootRacer.Utils
{
	public static class FloatExtentions
	{
		public static bool IsCloseEnough(this float thisValue, float otherValue, float acceptedDifference)
		{
			var difference = Math.Abs(thisValue - otherValue);
			return difference <= acceptedDifference;
		}
	}
}