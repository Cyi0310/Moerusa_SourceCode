using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonsterManager : MonoBehaviour
{
    protected List<Monster> Monsters = new List<Monster>();

    public float waitToAttackTime = 2f;

    private bool isChildrenAttack = false;

    public UnityEvent allDieEvent;

    void Start()
    {
        Initial();
    }
    
    protected virtual void Initial() {
        Monster[] _monsters = gameObject.GetComponentsInChildren<Monster>();
        for (int i = 0; i < _monsters.Length; i++)
            Monsters.Add(_monsters[i]);

        StartCoroutine(WaitMonstersAllDie());
    }
    IEnumerator WaitMonstersAllDie()
    {
        yield return new WaitUntil(() => Monsters.Count == 0);
        //Event
        allDieEvent.Invoke();
        Debug.Log("play");
        //timelineClip.Play();
    }

    protected void Update()
    {
        int targetAmount = 0;
        for (int i = 0; i < Monsters.Count; i++)
        {
            if (!Monsters[i].enabled || Monsters[i] == null)
            {
                Monsters.Remove(Monsters[i]);
                targetAmount = 0;
                for (int j = 0; j < Monsters.Count; j++)
                {
                    if (Monsters[j].isFindTarget)
                        targetAmount++;
                }
                break;
            }

            if (Monsters[i].isFindTarget)
                targetAmount++;
        }

        if (isChildrenAttack)
            return;

        isChildrenAttack = targetAmount > 0;

        if (isChildrenAttack)  
            StartCoroutine(ChildrenAttack());
    }
        
    IEnumerator ChildrenAttack()
    {
        yield return new WaitForSeconds(Random.Range(waitToAttackTime / 2, waitToAttackTime));
        int randomNumber = Random.Range(0, Monsters.Count);        
        Monsters[randomNumber].isPrepareAttack = true;
        float _time = 0f;
        bool _childrenMonsterAttacking =
            !Monsters[randomNumber].isAttack && _time < waitToAttackTime * waitToAttackTime && Monsters[randomNumber] != null;
        while (_childrenMonsterAttacking)
        {
            _time += Time.deltaTime;
            yield return null;
        }
        Monsters[randomNumber].isPrepareAttack = false;
        isChildrenAttack = false;
    }
    public bool AllMonstersIsDie { get => Monsters.Count == 0; }
    public void OpenTheDoor(Platform _Platform,bool Condition)
    {
        _Platform.enabled = Condition;
    }

    public void RemoveChildrenMonster(Monster _monster)
    {
        for (int i = 0; i < Monsters.Count; i++)
        {
            if (Monsters[i] == _monster)
            {
                Monsters.Remove(Monsters[i]);
                break;
            }
        }
    }
}
