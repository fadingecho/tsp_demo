using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
	public class Ant
	{
		public int start_v;
		public int v1;
		public int v2;
		public bool[] is_city_visited;

		public Tour tour;

		public Ant(int city_cnt)
		{
			tour = new Tour(city_cnt);
			is_city_visited = new bool[city_cnt + 5];
		}

		public int select_next()
		{
			float rnd = Random.Range(0f, 1f);
			float sum_prob = 0, sum = 0;
			for(int ci = 1;ci <= GameManager.Instance.City_cnt;ci++)
			{
				if (is_city_visited[ci] == false)
					sum += GameManager.Instance.City_value[v1, ci];
			}
			rnd *= sum;
			for(int ci = 1;ci <= GameManager.Instance.City_cnt;ci ++ )
			{
				if (is_city_visited[ci] == false)
				{
					sum_prob += GameManager.Instance.City_value[v1, ci];
					if(sum_prob >= rnd)
					{
						return ci;
					}
				}
			}
			//如果没有没去过的，回到出发点
			return start_v;
		}
	}
}
