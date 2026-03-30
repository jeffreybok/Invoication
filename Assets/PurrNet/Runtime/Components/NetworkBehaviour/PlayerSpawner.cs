using System.Collections.Generic;
using PurrNet.Logging;
using PurrNet.Modules;
using UnityEngine;

namespace PurrNet
{
    public struct SpawnPoint
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    public interface IProvideSpawnPoints
    {
        public SpawnPoint NextSpawnPoint(PlayerID player, SceneID scene);
    }

    public interface IProvidePrefabInstantiated
    {
        public void OnPrefabInstantiated(GameObject prefabInstance, PlayerID player, SceneID scene);
    }

    public class PlayerSpawner : PurrMonoBehaviour
    {
        [SerializeField, HideInInspector] private NetworkIdentity playerPrefab;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private bool _ignoreNetworkRules;

        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
        private int _currentSpawnPoint;

        private IProvideSpawnPoints _spawnPointProvider;
        private IProvidePrefabInstantiated _prefabInstantiatedProvider;

        public void SetRespawnPointProvider(IProvideSpawnPoints provider)
        {
            _spawnPointProvider = provider;
        }

        public void ResetSpawnPointProvider()
        {
            _spawnPointProvider = null;
        }

        public void SetPrefabInstantiatedProvider(IProvidePrefabInstantiated provider)
        {
            _prefabInstantiatedProvider = provider;
        }

        public void ResetPrefabInstantiatedProvider()
        {
            _prefabInstantiatedProvider = null;
        }

        private void Awake()
        {
            CleanupSpawnPoints();
        }

        private void CleanupSpawnPoints()
        {
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                if (!spawnPoints[i])
                {
                    spawnPoints.RemoveAt(i);
                    i--;
                }
            }
        }

        private void OnValidate()
        {
            if (playerPrefab)
            {
                _playerPrefab = playerPrefab.gameObject;
                playerPrefab = null;
            }
        }

        public override void Subscribe(NetworkManager manager, bool asServer)
        {
            if (!asServer) return;

            if (!manager.TryGetModule(out ScenePlayersModule scenePlayersModule, true)) return;

            scenePlayersModule.onPlayerLoadedScene += OnPlayerLoadedScene;

            if (!manager.TryGetModule(out ScenesModule scenes, true)) return;

            if (!scenes.TryGetSceneID(gameObject.scene, out var sceneID)) return;

            if (scenePlayersModule.TryGetPlayersInScene(sceneID, out var players))
            {
                foreach (var player in players)
                {
                    OnPlayerLoadedScene(player, sceneID, true);
                }
            }
        }

        public override void Unsubscribe(NetworkManager manager, bool asServer)
        {
            if (asServer && manager.TryGetModule(out ScenePlayersModule scenePlayersModule, true))
            {
                scenePlayersModule.onPlayerLoadedScene -= OnPlayerLoadedScene;
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.main &&
                NetworkManager.main.TryGetModule(out ScenePlayersModule scenePlayersModule, true))
            {
                scenePlayersModule.onPlayerLoadedScene -= OnPlayerLoadedScene;
            }
        }

        private void OnPlayerLoadedScene(PlayerID player, SceneID scene, bool asServer)
        {
            var main = NetworkManager.main;
            if (!main) return;

            if (!main.TryGetModule(out ScenesModule scenes, true)) return;

            var unityScene = gameObject.scene;

            if (!scenes.TryGetSceneID(unityScene, out var sceneID)) return;
            if (sceneID != scene) return;
            if (!asServer) return;

            bool isDestroyOnDisconnectEnabled = main.networkRules.ShouldDespawnOnOwnerDisconnect();

            if (!_ignoreNetworkRules &&
                !isDestroyOnDisconnectEnabled &&
                main.TryGetModule(out GlobalOwnershipModule ownership, true))
            {
                if (ownership.PlayerOwnsSomething(player)) return;
            }

            if (_playerPrefab == null) return;

            CleanupSpawnPoints();

            GameObject newPlayer;

            if (_spawnPointProvider != null)
            {
                var point = _spawnPointProvider.NextSpawnPoint(player, scene);
                newPlayer = UnityProxy.Instantiate(_playerPrefab, point.position, point.rotation, unityScene);
            }
            else if (spawnPoints.Count > 0)
            {
                var spawnPoint = spawnPoints[_currentSpawnPoint];
                if (spawnPoint == null) return;

                _currentSpawnPoint = (_currentSpawnPoint + 1) % spawnPoints.Count;

                newPlayer = UnityProxy.Instantiate(_playerPrefab, spawnPoint.position, spawnPoint.rotation, unityScene);
            }
            else
            {
                _playerPrefab.transform.GetPositionAndRotation(out var position, out var rotation);
                newPlayer = UnityProxy.Instantiate(_playerPrefab, position, rotation, unityScene);
            }

            if (newPlayer == null) return;

            _prefabInstantiatedProvider?.OnPrefabInstantiated(newPlayer, player, scene);

            if (newPlayer.TryGetComponent(out NetworkIdentity identity))
            {
                identity.GiveOwnership(player);
            }
        }
    }
}