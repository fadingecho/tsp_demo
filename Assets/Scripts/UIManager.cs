using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public class UIManager : MonoBehaviour
	{
		public static UIManager Instance = null;

		private Text it_times_text;
		private Text best_coverage_text;
		private Text algo_state_text;

		void Awake()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
				Destroy(gameObject);

		}

		private void Start()
		{
			it_times_text = GameObject.Find("it_times").GetComponent<Text>();
			best_coverage_text = GameObject.Find("best_coverage").GetComponent<Text>();
			algo_state_text = GameObject.Find("algo_state").GetComponent<Text>(); 
		}

		void Update()
		{
			it_times_text.text = "迭代次数" + GameManager.Instance.get_it_times().ToString();
			best_coverage_text.text = "最短距离" + GameManager.Instance.get_best_coverage().ToString();
			algo_state_text.text = GameManager.Instance.algo_state.ToString();
	}
	}
}