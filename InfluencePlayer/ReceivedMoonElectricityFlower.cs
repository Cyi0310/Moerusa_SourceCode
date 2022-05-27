using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceivedMoonElectricityFlower : ReceivedWaterBall
{
    public enum ElectricityFlowerType
    {
        NotHaveElectricity,
        HaveElectricity
    }
    public ElectricityFlowerType electricityFlowerType = ElectricityFlowerType.NotHaveElectricity;
    
    [SerializeField] protected Material HDRMaterial, NormalMaterial;

    protected List<ReceivedMoonElectricityFlower> receivedMoonElectricityFlowers = new List<ReceivedMoonElectricityFlower>();
    protected List<IEnumerator> IE_ReceivedMoonElectricityFlowers = new List<IEnumerator>();

    protected ParticleSystem electricityEffect;
    protected MeshRenderer flowerMeshRenderer;
    protected DangerSpace dangerSpace;

    public override bool ReceivedWater(WaterBall _waterBall)
    {
        //Size change
        if (speed <= 0) 
            Debug.LogError("speed[variable] need to more than 0");
        m_Count = Mathf.Clamp(m_Count, 0, FinalCount);
        ReceivedObject.localScale = Vector3.one * m_Count;

        return base.ReceivedWater(_waterBall);
    }

    protected override void Start()
    {
        base.Start();

        electricityEffect = gameObject.GetComponentInChildren<ParticleSystem>();
        dangerSpace = gameObject.GetComponent<DangerSpace>();
        flowerMeshRenderer = gameObject.transform.GetChild(1).GetComponent<MeshRenderer>();

        bool isElectricityFlower = electricityFlowerType == ElectricityFlowerType.HaveElectricity;
        SetElectricity(isElectricityFlower);

        //Initial size 
        m_Count = ReceivedObject.localScale.x;
    }

    public void SetElectricity(bool electricityCondition)
    {
        if (!electricityCondition)
        {
            electricityFlowerType = ElectricityFlowerType.NotHaveElectricity;
            flowerMeshRenderer.material = NormalMaterial;
            electricityEffect.Stop();
        }
        else
        {
            electricityFlowerType = ElectricityFlowerType.HaveElectricity;
            flowerMeshRenderer.material = HDRMaterial;
            electricityEffect.Play();
        }
        dangerSpace.isDamage = electricityCondition;
    }
    private void OnTriggerEnter(Collider other)
    {
        ReceivedMoonElectricityFlower moonElectricityFlowerReceived = other.GetComponent<ReceivedMoonElectricityFlower>();
        if (other.GetType() != this.GetType() && moonElectricityFlowerReceived != null)
        {
            receivedMoonElectricityFlowers.Add(moonElectricityFlowerReceived);
            IE_ReceivedMoonElectricityFlowers.Add(WaitToElectricity(moonElectricityFlowerReceived));
            StartCoroutine(IE_ReceivedMoonElectricityFlowers[IE_ReceivedMoonElectricityFlowers.Count - 1]);
        }
    }   
    private void OnTriggerExit(Collider other)
    {
        for (int i = 0; i < receivedMoonElectricityFlowers.Count; i++)
        {
            if(receivedMoonElectricityFlowers[i].gameObject == other) 
            { 
                receivedMoonElectricityFlowers.Remove(receivedMoonElectricityFlowers[i]);
                StopCoroutine(IE_ReceivedMoonElectricityFlowers[i]);
                IE_ReceivedMoonElectricityFlowers.Remove(IE_ReceivedMoonElectricityFlowers[i]);
            }
        }
    }
    IEnumerator WaitToElectricity(ReceivedMoonElectricityFlower moonElectricityFlowerReceived)
    {
        //自己有電 且 隔壁沒電
        yield return new WaitUntil(() => electricityFlowerType == ElectricityFlowerType.HaveElectricity && moonElectricityFlowerReceived.electricityFlowerType == ElectricityFlowerType.NotHaveElectricity);

        yield return null;
        moonElectricityFlowerReceived.SetElectricity(true);
    }
}
