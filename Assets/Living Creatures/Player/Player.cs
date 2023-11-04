using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using WatchDog;

public class Player : Creature, IDamagable
{
    private static Player PlayerInstance;
    public static Player GetPlayerInstance
    {
        get
        {
            if (PlayerInstance == null)
            {
                Watchdog.CriticalErrorCallback.Invoke(new(new EventMessage("No player found! Please create a player instance and restart!")));
                return null;
            }

            return PlayerInstance;
        }
    }

    public void Damage(int damageTaken, DamageTypes[] damageTypes = null)
    {
        //Compare damage type to the resistences and weaknesses
        throw new System.NotImplementedException();
    }

    public void Heal(int healthReplenished, HealingTypes healingType)
    {
        throw new System.NotImplementedException();
    }

    public void Killed()
    {
        throw new System.NotImplementedException();
    }

    public override async Task OnAwake()
    {
        if (PlayerInstance == null)
            PlayerInstance = this;
        else
        {
            Watchdog.CriticalErrorCallback.Invoke(new(new EventMessage("Multiple instances of the player detected!")));
            Destroy(this);
            return;
        }
    }

}
