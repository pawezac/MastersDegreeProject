using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

namespace LandmassProceduralGeneration
{
    public class ChunkPool : Utils.Singleton<ChunkPool>
    {
        IObjectPool<TerrainChunk> m_Pool;

        Transform PoolTransform => transform;

        public IObjectPool<TerrainChunk> Pool
        {
            get
            {
                if (m_Pool == null)
                {
                    m_Pool = new ObjectPool<TerrainChunk>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 10, 50);
                }
                return m_Pool;
            }
        }

        private void OnDestroyPoolObject(TerrainChunk chunk)
        {
            Destroy(chunk.meshObject);
        }

        private void OnReturnedToPool(TerrainChunk chunk)
        {
            chunk.InitializeOnRelease(PoolTransform);
            chunk.SetVisible(false);
            chunk.Released = true;
        }

        private void OnTakeFromPool(TerrainChunk chunk)
        {
            chunk.SetVisible(true);
            chunk.Released = false;
        }

        private TerrainChunk CreatePooledItem()
        {
            var chunk = new TerrainChunk(PoolTransform);
            chunk.SetVisible(false);
            chunk.Released = false;
            return chunk;
        }
    }
}