using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
	public class Road : MonoBehaviour
	{
		[HideInInspector]
		public float dis;        //边权
		[HideInInspector]
		public float herustic;  //启发值
		[HideInInspector]
		public float pheromone; //信息素
		[HideInInspector]
		public float city_value;//pheromone ^ delta * herustic ^ beta

		[HideInInspector]
		public City city1;
		[HideInInspector]
		public City city2;

		public Material material;

		private GameObject road_line;
		void Start()
		{
			//https://blog.csdn.net/weixin_42513339/article/details/88625777
			Vector3 rightPosition = (city1.transform.position + city2.transform.position) / 2;
			Vector3 rightRotation = city2.transform.position - city1.transform.position;
			float HalfLength = Vector3.Distance(city1.transform.position, city2.transform.position) / 2;
			float LThickness = 40f;//线的粗细

			//创建圆柱体
			road_line = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			road_line.gameObject.transform.parent = transform;
			road_line.transform.position = rightPosition;
			road_line.transform.rotation = Quaternion.FromToRotation(Vector3.up, rightRotation);
			road_line.transform.localScale = new Vector3(LThickness, HalfLength, LThickness);

			road_line.GetComponent<MeshRenderer>().material = material;
			road_line.GetComponent<MeshRenderer>().material.color = new Color(pheromone*5000, pheromone * 5000, pheromone * 5000, pheromone * 5000);
		}

		private void Update()
		{
			road_line.GetComponent<MeshRenderer>().material.color = new Color(pheromone * 5000, pheromone * 5000, pheromone * 5000, pheromone * 5000);
		}
	}
}
