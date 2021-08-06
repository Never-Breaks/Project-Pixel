using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public BasicAI ai;
    private void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Spell")
        {
            //each spell will differ in damage and right now I don't know what the name of the
            //damage script is so i'll fix this later

            //ai.TakeDamage(col.getcomponet("script").damage);
        }
    }
}
