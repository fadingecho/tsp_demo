using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
	class Road
	{
		public float dis;        //边权
		private float herustic;  //启发值
		private float pheromone; //信息素
		private float city_value;//pheromone ^ delta * herustic ^ beta
	}
}
