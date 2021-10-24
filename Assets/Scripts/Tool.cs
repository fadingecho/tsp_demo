using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
	public static class Tool
	{
		public static float Pow_f(float x, int y)
		{
			float ans = 1;
			while (y != 0)
			{
				if ((y & 1) == 1)
					ans *= x;
				x *= x;
				y >>= 1;
			}
			return ans;
		}

	}
}
