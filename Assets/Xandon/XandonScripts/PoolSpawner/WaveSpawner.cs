using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.Networking;

public class WaveSpawner : NetworkBehaviour {

    //Tower defense code
    //public KeyCode spawnKey;
    //public GameObject enemy;
    //public Transform enemyContainer;
    //private Transform nexus;
    //end of tower defense code

    public enum SpawnState { SPAWNING, WAITING, COUNTING };
	//public GameObject prefab;

	[System.Serializable]
	public class Wave
	{
		public string name;
		public GameObject enemy;
		public int count;
		public float rate;
    }

	public Wave[] waves;
	private int nextWave = 0;
	public int NextWave
	{
		get { return nextWave + 1; }
	}

	public Transform[] spawnPoints;

	public float timeBetweenWaves = 5f;
    public float minScale;
    public float maxScale;
    private float waveCountdown;
	public float WaveCountdown
	{
		get { return waveCountdown; }
	}

	private float searchCountdown = 1f;

	private SpawnState state = SpawnState.COUNTING;
	public SpawnState State
	{
		get { return state; }
	}

    public override void OnStartServer()
    {
        if (isServer) {
            //nexus = GameObject.FindGameObjectWithTag("Nexus").transform;
            if (spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points referenced.");
            }

            waveCountdown = timeBetweenWaves;
        }
	}

	void Update()
	{
        if (isServer)
        {
            if (state == SpawnState.WAITING)
            {
                if (!EnemyIsAlive())
                {
                    WaveCompleted();
                }
                else
                {
                    WaveCompleted();
                    return;
                }
            }

            if (waveCountdown <= 0)
            {
                if (state != SpawnState.SPAWNING)
                {
                    StartCoroutine(SpawnWave(waves[nextWave]));
                }
            }
            else
            {
                waveCountdown -= Time.deltaTime;
            }
        }
	}

	void WaveCompleted()
	{
		Debug.Log("Wave Completed!");

		state = SpawnState.COUNTING;
		waveCountdown = timeBetweenWaves;

		if (nextWave + 1 > waves.Length - 1)
		{
			nextWave = 0;
			Debug.Log("ALL WAVES COMPLETE! Looping...");
		}
		else
		{
			nextWave++;
		}
	}

	bool EnemyIsAlive()
	{
		searchCountdown -= Time.deltaTime;
		if (searchCountdown <= 0f)
		{
			searchCountdown = 1f;
			if (GameObject.FindGameObjectWithTag("Enemy") == null)
			{
				return false;
			}
		}
		return true;
	}

	IEnumerator SpawnWave(Wave _wave)
	{
		Debug.Log("Spawning Wave: " + _wave.name);
		state = SpawnState.SPAWNING;

		for (int i = 0; i < _wave.count; i++)
		{
			SpawnEnemy(_wave.enemy);
			yield return new WaitForSeconds( 1f/_wave.rate );
		}

		state = SpawnState.WAITING;

		yield break;
	}

	void SpawnEnemy(GameObject _enemy)
	{
		Debug.Log("Spawning Enemy: " + _enemy.name);
		Transform _sp = spawnPoints[ Random.Range (0, spawnPoints.Length) ];
        float scale = (Random.Range(minScale, maxScale));
        PoolManager.instance.ReuseObject (_enemy, _sp.position, _sp.rotation, scale);
    }
}
