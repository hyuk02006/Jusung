using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee,Range};
    public Type type;
    public int damage;
    public float rate;
    public int maxAmmo;
    public int curAmmo;

    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;
    public Transform bulletPos;
    public GameObject bullet;
    public Transform bulletCasePos;
    public GameObject bulletCase;

    public void Use()
    {
        if(type ==Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");            
        }
        else if (type == Type.Range && curAmmo >0)
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
    }

    IEnumerator Swing()
    {
        //1
        yield return new WaitForSeconds(0.1f); //0.1초 대기
        meleeArea.enabled = true;
        trailEffect.enabled = true;
        //2 
        yield return new WaitForSeconds(0.3f); //0.1초 대기
        meleeArea.enabled = false;
        //3
        yield return new WaitForSeconds(0.3f); //0.1초 대기
        trailEffect.enabled = false;
    }

    IEnumerator Shot()
    {
        //1.총알발사
        GameObject intantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = intantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 55;

        yield return null;
        //2.탄피 배출
        GameObject intantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody bulletCaseRigid = intantBullet.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        bulletCaseRigid.AddForce(caseVec, ForceMode.Impulse);
        bulletCaseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
    }
    //Use() 메인루틴 -> Swing() 서브루틴 -> Use() 메인루틴 
    //Use() 메인루틴 + Swing() 코루틴 (Co-Op)
}
