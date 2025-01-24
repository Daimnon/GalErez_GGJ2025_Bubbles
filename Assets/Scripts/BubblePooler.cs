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
            newBubble.BubblePoller = this;
            newBubble.gameObject.SetActive(false);
            newBubble.IsFrozen = true;
            newBubble.ResetAnimationState();
            _pool.Add(newBubble);
        }
    }

    public Bubble GetFromPool(Vector3 newPos)
    {
        if (_pool.Count < 1) InitializePool();

        Bubble bubble = _pool[0];
        bubble.transform.SetParent(null);
        bubble.transform.SetPositionAndRotation(newPos, Quaternion.identity);
        bubble.gameObject.SetActive(true);
        bubble.IsFrozen = false;
        _pool.Remove(bubble);
        return bubble;
    }
    public void ReturnToPool(Bubble bubble)
    {
        bubble.gameObject.SetActive(false);
        bubble.transform.SetParent(transform);
        bubble.transform.position = Vector3.zero;
        bubble.transform.localScale = new(0.1f, 0.1f, 0.1f);
        bubble.IsFrozen = true;
        bubble.ResetAnimationState();
        _pool.Add(bubble);
    }
}
