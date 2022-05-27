using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceivedWaterBall : MonoBehaviour, IReceivedWaterBall
{
    //Only received water ball, not change about ex:scale,position...
    public Transform ReceivedObject; 

    public float speed = 2f;
    public float FinalCount = 10f;
    protected float m_Count = 0f;
    public bool isMax { get => m_Count >= FinalCount; }
    [Range(0.1f, 15f)] public float tipIconOffset = 0.75f;

    public virtual bool ReceivedWater(WaterBall _waterBall) //true => can continu received
    {
        if (isMax)
            return false;
        else
            m_Count = Mathf.Clamp(m_Count, 0, FinalCount);

        m_Count += speed * Time.deltaTime;
        return true;
    }
    protected virtual void Start()
    {
        if (ReceivedObject == null)
            ReceivedObject = gameObject.transform;
    }

    public void SetIconAppear(GameObject _needWaterIcon)
    {
        _needWaterIcon.transform.position = transform.position + Vector3.up * tipIconOffset;
    }

    private void OnDrawGizmosSelected()
    {
        Color _color = new Color(0, 0, 0.5f, 0.5f);
        Gizmos.color = _color;
        Gizmos.DrawSphere(transform.position + Vector3.up * tipIconOffset, 0.5f);
    }
}
