using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
	public class GameManager : MonoBehaviour
	{
		public static GameManager Instance = null;
		public GameObject city_prefab;
		public GameObject road_prefab;
		public GameObject ant_prefab;

		private int city_cnt;
		//public float[,] dis;		//边权
		public City[] citys;        //城市数组
		public Road[,] roads;

		private float alpha = 0.1F;	//挥发参数 evaporation parameter
		private int delta = 1;    //信息素浓度权重
		private int beta = 6;     //与距离相关的启发值的权重

		private Ant[] ants;
		private float ant_cnt;

		private readonly string tsp_data_path = "KroA100";  //数据文件
		private string[] tsp_data;

		private int max_it_time;
		private int it_time = 0;
		private Tour best_tour;

		public int City_cnt { get => city_cnt; set => city_cnt = value; }

		private WaitForSeconds ant_wait_time = new WaitForSeconds(Ant.time_per_road + 0.3f);
		private WaitForSeconds it_wait_time = new WaitForSeconds(0.5f);
		private WaitForSeconds stage_wait_time = new WaitForSeconds(1.5f);

		public enum Algo_state
		{
			init,
			reset,
			construct_solution,
			update_pheromone
		}
		public Algo_state algo_state = Algo_state.init;

		void Awake()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
				Destroy(gameObject);

			//step1 : read tsp data
			read_tsp();

			//step2 : visualize the graph
			init();
		}

		void Start()
		{
			//step3 : iterate all agents
			StartCoroutine(ACO_iterate());
		}
		private void read_tsp()
		{
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
				city.transform.position = new Vector3(float.Parse(city_data[1]), float.Parse(city_data[2]), 0);
				citys[i] = city;
			}

			//蚁群
			ants = new Ant[city_cnt + 10];
			ant_cnt = city_cnt;
			GameObject ant_rt = new GameObject("ant_rt");
			for(int i = 1;i <= ant_cnt;i ++ )
			{
				GameObject gameObject = Instantiate(ant_prefab, ant_rt.transform);
				ants[i] = gameObject.GetComponent<Ant>();
				ants[i].v1 = ants[i].v2 = i;
			}
			
			//预计算每个城市之间的距离，统计有边权的边数目和总权重
			int weight_cnt = 0;
			float avg_weight = 0;
			roads = new Road[city_cnt + 10, city_cnt + 10];

			GameObject road_rt = new GameObject("road_rt");
			for (int i = 1; i <= city_cnt; i++)
				for (int j = i; j <= city_cnt; j++)
				{
					GameObject gameObject = Instantiate(road_prefab, road_rt.transform);
					Road r = gameObject.GetComponent<Road>();
					r.dis = Vector2.Distance(citys[i].transform.position, citys[j].transform.position);
					if (r.dis > float.Epsilon) { weight_cnt++; avg_weight += r.dis; }
					r.city1 = citys[i];
					r.city2 = citys[j];
					//无向图，所以边表对称的元素指向同一引用
					roads[i, j] = r;
					roads[j, i] = r;
				}

			//初始化边（费洛蒙和启发值）
			float p0 = weight_cnt / (avg_weight * city_cnt);
			for (int i = 1;i <= city_cnt;i ++ )
				for(int j = 1;j <= city_cnt;j ++ )
				{
					roads[i, j].pheromone = p0;
					roads[i, j].herustic = 1 / (roads[i, j].dis + float.Epsilon);
				}
			best_tour = new Tour(city_cnt);
			best_tour.Clean();
			it_time = 0;
			max_it_time = city_cnt * city_cnt;
		}
		IEnumerator ACO_iterate()
		{
			float last = float.PositiveInfinity;
			int bad_times = 0;
			for(;it_time < max_it_time;it_time ++ )
			{
				if (bad_times > city_cnt) break;
				ACO_reset();
				yield return stage_wait_time;
				yield return StartCoroutine(construct_solution());
				update_pheromone();
				
				if (last > best_tour.coverage)
				{
					last = best_tour.coverage;
					bad_times = 0;
				}
				else bad_times++;
				yield return stage_wait_time;
			}
		}

		private void update_pheromone()
		{
			algo_state = Algo_state.update_pheromone;
			Tour now_best = new Tour(city_cnt);
			now_best.Clean();
			for(int ai = 1;ai <= ant_cnt;ai ++ )
			{
				Tour t = ants[ai].tour;
				t.Calc();
				if (t < now_best)
					now_best = t;
			}
			if(now_best < best_tour)
			{
				//赋值拷贝过去
				best_tour.coverage = now_best.coverage;

				best_tour.trace.Clear();
				now_best.trace.ForEach(i => best_tour.trace.Add(i));
			}
			for (int c1 = 1; c1 <= city_cnt; c1++)
				for (int c2 = 1; c2 <= city_cnt; c2++)
					roads[c1, c2].pheromone *= (1 - alpha);
			for (int i = 0; i < now_best.Size();i ++ )
			{
				roads[now_best.trace[i].x, now_best.trace[i].y].pheromone += 1f / now_best.coverage;
			}
			return;
		}

		IEnumerator construct_solution()
		{
			algo_state = Algo_state.construct_solution;
			for (int i = 0; i < city_cnt; i++)
			{
				for (int ai = 1;ai <= ant_cnt;ai ++ )
				{
					ants[ai].visist_next();
				}
				yield return ant_wait_time;
			}
			yield return ant_wait_time ;
		}

		private void ACO_reset()
		{
			algo_state = Algo_state.reset;
			for (int i = 1; i <= city_cnt; i++)
			{
				ants[i].aco_reset(i);
			}
				
			for(int i = 1;i <= city_cnt;i ++ )
				for(int j = 1;j <= city_cnt;j ++ )
				{
					roads[i, j].city_value = Tool.Pow_f(roads[i, j].pheromone, delta) * Tool.Pow_f(roads[i, j].herustic, beta);
				}
			
		}

		public float get_best_coverage()
		{
			return best_tour.coverage;
		}

		public int get_it_times()
		{
			return it_time;
		}
	}
}