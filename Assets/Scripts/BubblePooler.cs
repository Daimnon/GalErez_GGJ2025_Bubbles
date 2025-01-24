using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubblePooler : MonoBehaviour
{
    [SerializeField] private Bubble _prefab;
    [SerializeField] private int _initialPoolSize = 20;
    private List<Bubble> _pool;

    private void Awake()
    {
        _pool = new();
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < _initialPoolSize; i++)
        {
            Bubble newBubble = Instantiate(_prefab, transform);
            newBubble.gameObject.SetActive(false);
            _pool.Add(newBubble);
        }
    }

    public Bubble GetFromPool(Vector3 newPos)
    {
        if (_pool.Count < 1) return Instantiate(_prefab, newPos, Quaternion.identity);

        Bubble bubble = _pool[0];
        bubble.transform.SetParent(null);
        bubble.transform.SetPositionAndRotation(newPos, Quaternion.identity);
        bubble.gameObject.SetActive(true);
        _pool.Remove(bubble);
        return bubble;
    }
    public void ReturnToPool(Bubble bubble)
    {
        bubble.gameObject.SetActive(false);
        bubble.transform.SetParent(transform);
        bubble.transform.position = Vector3.zero;
        _pool.Add(bubble);
    }
}
