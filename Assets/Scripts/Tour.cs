using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
	public class Tour
	{
        public List<Vector2Int> trace; //path[i]，存储一条边(r->s)
        public float coverage;

        public Tour(int city_cnt)
		{
			this.trace = new List<Vector2Int>(city_cnt + 5);
			this.coverage = float.PositiveInfinity;
		}

		public void Clean()
        {
            coverage = float.PositiveInfinity;
            trace.Clear();
        }

        //计算路径长度
        public void Calc()
        {
            if(trace.Count == 0)
			{
                coverage = float.PositiveInfinity;
                return;
			}
            coverage = 0;
            foreach(Vector2Int p in trace) 
            {
                coverage += GameManager.Instance.roads[p.x, p.y].dis;
            }
        }

        //向部分解中加入一条边
        public void Push_back(int city1, int city2)
        {
            trace.Add(new Vector2Int(city1, city2));
        }
        public int Size()
        {
            return trace.Count;
        }
        public int Get_city1_idx(int i)
        {
            return trace[i].x;
        }
        public int Get_city2_idx(int i)
        {
            return trace[i].y;
        }
      
        public static bool operator< (Tour t1, Tour t2)
        {
            return t1.coverage < t2.coverage;
        }

        public static bool operator >(Tour t1, Tour t2)
        {
            return t1.coverage > t2.coverage;
        }
    }
}
