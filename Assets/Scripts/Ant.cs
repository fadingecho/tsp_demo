using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
	public class Ant : MonoBehaviour
	{
		public int start_v;
		public int v1;
		public int v2;
		public bool[] is_city_visited;

		public Tour tour;

		private GameObject sphere;
		private static Vector3 sphere_offset = new Vector3(0, 0, -50);
		public readonly static float time_per_road = 0.3f;

		private Vector3 velocity = Vector3.zero;

		private void Awake()
		{
			tour = new Tour(GameManager.Instance.City_cnt);
			is_city_visited = new bool[GameManager.Instance.City_cnt + 5];

			sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			sphere.transform.SetParent(transform);
			sphere.transform.localScale = 30 * Vector3.one;
			sphere.transform.localPosition = Vector3.zero;
			sphere.GetComponent<MeshRenderer>().material.color = Color.black;
		}

		private void Update()
		{
			transform.position = Vector3.SmoothDamp(
				transform.position,
				GameManager.Instance.citys[v2].transform.position + sphere_offset,
				ref velocity,
				time_per_road
				);
		}

		public void visist_next()
		{
			int next_city = select_next();
			is_city_visited[next_city] = true;
			v2 = next_city;
			tour.Push_back(v1, v2);
			v1 = v2;
		}

		public void aco_reset(int start_city_index)
		{
			tour.Clean();
			start_v = start_city_index;
			//用布尔数组表示访问的城市集合
			for (int j = 1; j <= GameManager.Instance.City_cnt; j++)
			{
				is_city_visited[j] = false;
			}
			is_city_visited[start_city_index] = true;
			v1 = start_v;

			//显示位置
			transform.position = GameManager.Instance.citys[start_city_index].transform.position + sphere_offset;
		}

		private int select_next()
		{
			float rnd = Random.Range(0f, 1f);
			float sum_prob = 0, sum = 0;
			for(int ci = 1;ci <= GameManager.Instance.City_cnt;ci++)
			{
				if (is_city_visited[ci] == false)
					sum += GameManager.Instance.roads[v1, ci].city_value;
			}
			rnd *= sum;
			for(int ci = 1;ci <= GameManager.Instance.City_cnt;ci ++ )
			{
				if (is_city_visited[ci] == false)
				{
					sum_prob += GameManager.Instance.roads[v1, ci].city_value;
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
