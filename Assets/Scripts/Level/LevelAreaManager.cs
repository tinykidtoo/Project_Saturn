using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Level {
    public class LevelAreaManager : MonoBehaviour {

        [SerializeField] private GameObject entryBlocker;
        [SerializeField] private GameObject exitBlocker;

        [SerializeField] private GameObject spawnPointHolder;
        
        private List<WaveData> waveData;
        private readonly List<GameObject> spawned = new List<GameObject>();
        private List<Vector3> spawnPoints;
        private int wave;
        private bool playerEntered;
        private bool finished;

        private void Start() {
            waveData = gameObject.GetComponents<WaveData>().ToList();
            var children = spawnPointHolder.GetComponentsInChildren<Transform>().ToList();
            children.Remove(spawnPointHolder.transform);
            spawnPoints = children.Select(trans => trans.position).ToList();
        }
        
        private void Update() {
            if (finished || !playerEntered) return;
            
            spawned.RemoveAll(spawn => spawn == null);
            if (spawned.Count > 0) return;

            if (wave < waveData.Count) {
                var data = waveData.First(wd => wd.wave == wave);
                DoSpawning(data);
                wave++;
            }
            else {
                finished = true;
                exitBlocker.SetActive(false);
            }

        }

        private void DoSpawning(WaveData data) {
            for (var k = 0; k < data.wavePrefabs.Count; k++) {
                var pos = spawnPoints[k];
                Debug.Log("spawning " + k + " at " + pos);
                var prefab = data.wavePrefabs[k];

                var spawn = Instantiate(prefab);
                spawn.transform.position = pos;
                spawned.Add(spawn);
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (playerEntered || !other.gameObject.CompareTag("Player")) return;

            playerEntered = true;
            entryBlocker.SetActive(true);
            exitBlocker.SetActive(true);
        }
    }
}