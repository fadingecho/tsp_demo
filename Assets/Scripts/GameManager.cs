using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts
{
	public class GameManager : MonoBehaviour
	{
		public static GameManager Instance = null;
		public GameObject city_prefab;

		private int city_cnt;
		public float[,] dis;		//边权
		public City[] citys;		//城市数组

		private float alpha = 0.1F;	//挥发参数 evaporation parameter
		private int delta = 1;    //信息素浓度权重
		private int beta = 6;     //与距离相关的启发值的权重
		private float[,] herustic;	//启发值
		private float[,] pheromone; //信息素
		private float[,] city_value;//pheromone ^ delta * herustic ^ beta

		private Ant[] ants;
		private float ant_cnt;

		private readonly string tsp_data_path = "KroA100";  //数据文件
		private string[] tsp_data;

		private int max_it_time;
		private int it_time = 0;
		private Tour best_tour;

		public int City_cnt { get => city_cnt; set => city_cnt = value; }
		public float[,] City_value { get => city_value; set => city_value = value; }

		void Awake()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
				Destroy(gameObject);
		}

		void Start()
		{
			//step1 : read tsp data
			read_tsp();

			//step2 : visualize the graph
			init();

			//step3 : iterate all agents
			StartCoroutine(ACO_iterate());
		}

		IEnumerator ACO_iterate()
		{
			float last = float.PositiveInfinity;
			int bad_times = 0;
			for(;it_time < max_it_time;it_time ++ )
			{
				if (bad_times > city_cnt) break;
				ACO_reset();
				construct_solution();
				update_pheromone();
				Debug.Log("times: " + it_time + " best: " + best_tour.coverage);
				if (last > best_tour.coverage)
				{
					last = best_tour.coverage;
					bad_times = 0;
				}
				else bad_times++;
				yield return null;
			}	
		}

		private void update_pheromone()
		{
			Tour now_best = new Tour(city_cnt);
			now_best.Clean();
			for(int ai = 1;ai <= ant_cnt;ai ++ )
			{
				Tour t = ants[ai].tour;
				string msg = t.Calc();
				Debug.Log(ai + "\n" + " " + msg);
				if (t < now_best)
					now_best = t;
			}
			if(now_best < best_tour)
			{
				best_tour = now_best;
			}
			for (int c1 = 1; c1 <= city_cnt; c1++)
				for (int c2 = 1; c2 <= city_cnt; c2++)
					pheromone[c1, c2] *= (1 - alpha);
			for (int i = 0; i < now_best.Size();i ++ )
			{
				pheromone[now_best.trace[i].x, now_best.trace[i].y] += 1f / now_best.coverage;
				pheromone[now_best.trace[i].y, now_best.trace[i].x] = pheromone[now_best.trace[i].x, now_best.trace[i].y];
			}
			return;
		}

		private void construct_solution()
		{
			for(int i = 0;i < city_cnt;i ++ )	
				for(int ai = 1;ai <= ant_cnt;ai ++ )
				{
					Ant ant = ants[ai];
					int next_city = ant.select_next();
					ant.is_city_visited[next_city] = true;
					ant.v2 = next_city;
					ant.tour.Push_back(ant.v1, ant.v2);
					ant.v1 = ant.v2;
				}
			return;
		}

		private void read_tsp()
		{
			Debug.Log("read_tsp");
			TextAsset textAsset = Resources.Load<TextAsset>(tsp_data_path);
			if (textAsset == null) Debug.Log("null tsp_data_path, now path is " + tsp_data_path);
			tsp_data = textAsset.ToString().Split('\n');
		}

		//解析数据，显示城市
		private void init()
		{
			//初始化城市列表
			city_cnt = int.Parse(tsp_data[0]);
			citys = new City[city_cnt + 10];
			Debug.Log("city number is " + city_cnt);
			
			//创建每个城市
			GameObject city_rt = new GameObject("city_rt");
			for (int i = 1; i <= city_cnt; i++)
			{
				//取数据
				string[] city_data = tsp_data[i].Split(' ');

				GameObject gameObject = Instantiate(city_prefab, city_rt.transform);
				gameObject.name = "city" + city_data[0];

				City city = gameObject.GetComponent<City>();
				city.Id = i;
				city.transform.position = new Vector2(int.Parse(city_data[1]), int.Parse(city_data[2]));
				citys[i] = city;
			}

			//蚁群
			ants = new Ant[city_cnt + 10];
			ant_cnt = city_cnt;
			for(int i = 1;i <= ant_cnt;i ++ )
			{
				ants[i] = new Ant(city_cnt);
			}
			
			//预计算每个城市之间的距离，统计有边权的边数目和总权重
			int weight_cnt = 0;
			float avg_weight = 0;
			city_value = new float[city_cnt + 10, city_cnt + 10];
			dis = new float[city_cnt + 10, city_cnt + 10];
			for (int i = 1; i <= city_cnt; i++)
				for (int j = 1; j <= city_cnt; j++)
				{
					dis[i, j] = Vector2.Distance(citys[i].transform.position, citys[j].transform.position);
					if(dis[i, j] > float.Epsilon) { weight_cnt++; avg_weight += dis[i, j]; } 
				}

			//初始化边（费洛蒙和启发值）
			float p0 = weight_cnt / (avg_weight * city_cnt);
			pheromone = new float[city_cnt + 10, city_cnt + 10];
			herustic = new float[city_cnt + 10, city_cnt + 10];
			for (int i = 1;i <= city_cnt;i ++ )
				for(int j = 1;j <= city_cnt;j ++ )
				{
					pheromone[i, j] = p0;
					herustic[i, j] = 1 / (dis[i, j] + float.Epsilon);
				}
			best_tour = new Tour(city_cnt);
			best_tour.Clean();
			it_time = 0;
			max_it_time = city_cnt * city_cnt;
		}

		private void ACO_reset()
		{
			for (int i = 1; i <= ant_cnt; i++)
			{
				Ant a = ants[i];
				a.tour.Clean();
				a.start_v = i;
				//用布尔数组表示访问的城市集合
				for(int j = 1;j <= city_cnt;j ++ )
				{
					a.is_city_visited[j] = false;
				}
				a.is_city_visited[i] = true;
				a.v1 = a.start_v;
			}
				
			for(int i = 1;i <= city_cnt;i ++ )
				for(int j = 1;j <= city_cnt;j ++ )
				{
					city_value[i, j] = Tool.Pow_f(pheromone[i, j], delta) * Tool.Pow_f(herustic[i, j], beta);
				}
			
		}
	}
}